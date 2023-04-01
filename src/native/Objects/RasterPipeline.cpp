#include "stdafx.h"

static ComPtr<ID3DBlob> CompileShader(
    const wchar_t* path,
    const char* entryPoint, const char* target,
    NativeErrorMessageFunc callback)
{
    ComPtr<ID3DBlob> shaderBlob;
    ComPtr<ID3DBlob> errorBlob;

#if defined(_DEBUG)
    UINT compileFlags = D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION;
#else
    UINT compileFlags = 0;
#endif

    if (const auto result = D3DCompileFromFile(
            path,
            nullptr,
            D3D_COMPILE_STANDARD_FILE_INCLUDE,
            entryPoint,
            target,
            compileFlags,
            0,
            &shaderBlob,
            &errorBlob);
        FAILED(result) && errorBlob != nullptr && errorBlob->GetBufferPointer() != nullptr)
    {
        std::vector<char> message(errorBlob->GetBufferSize() + 1);
        memcpy(message.data(), errorBlob->GetBufferPointer(), errorBlob->GetBufferSize());
        message[errorBlob->GetBufferSize()] = 0;

        std::string formattedMessage = "Shader compilation failed: ";
        formattedMessage.append(message.data());

        callback(formattedMessage.c_str());

        return nullptr;
    }

    return shaderBlob;
}

using Preset = std::tuple<ComPtr<ID3D12RootSignature>, std::vector<D3D12_INPUT_ELEMENT_DESC>>;

static Preset GetSpace3dPreset(ComPtr<ID3D12Device5> device)
{
    std::vector<D3D12_INPUT_ELEMENT_DESC> space3dInput =
    {
        {
            "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT,
            0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        },
        {
            "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
            0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        }
    };

    ComPtr<ID3D12RootSignature> rootSignature;

    CD3DX12_DESCRIPTOR_RANGE1 ranges[1];
    CD3DX12_ROOT_PARAMETER1 rootParameters[1];

    ranges[0].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);
    rootParameters[0].InitAsDescriptorTable(1, &ranges[0], D3D12_SHADER_VISIBILITY_ALL);

    CD3DX12_VERSIONED_ROOT_SIGNATURE_DESC rootSignatureDesc;
    rootSignatureDesc.Init_1_1(_countof(rootParameters), rootParameters, 0, nullptr,
                               D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

    ComPtr<ID3DBlob> signature;
    ComPtr<ID3DBlob> error;
    TRY_DO(
        D3DX12SerializeVersionedRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1_1, &signature, &error
        ));
    TRY_DO(device->CreateRootSignature(0,
        signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&rootSignature)));

    return {rootSignature, space3dInput};
}

static Preset GetPostProcessingPreset(ComPtr<ID3D12Device5> device)
{
    std::vector<D3D12_INPUT_ELEMENT_DESC> postProcessingInput =
    {
        {
            "POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
            0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        },
        {
            "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
            0, D3D12_APPEND_ALIGNED_ELEMENT, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        }
    };

    ComPtr<ID3D12RootSignature> rootSignature;

    CD3DX12_DESCRIPTOR_RANGE1 ranges[1];
    CD3DX12_ROOT_PARAMETER1 rootParameters[1];

    ranges[0].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1, 0);
    rootParameters[0].InitAsDescriptorTable(1, &ranges[0], D3D12_SHADER_VISIBILITY_PIXEL);

    D3D12_STATIC_SAMPLER_DESC sampler;
    sampler.Filter = D3D12_FILTER_MIN_MAG_MIP_LINEAR;
    sampler.AddressU = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.AddressV = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.AddressW = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.MipLODBias = 0;
    sampler.MaxAnisotropy = 0;
    sampler.ComparisonFunc = D3D12_COMPARISON_FUNC_NEVER;
    sampler.BorderColor = D3D12_STATIC_BORDER_COLOR_TRANSPARENT_BLACK;
    sampler.MinLOD = 0.0f;
    sampler.MaxLOD = D3D12_FLOAT32_MAX;
    sampler.ShaderRegister = 0;
    sampler.RegisterSpace = 0;
    sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;

    CD3DX12_VERSIONED_ROOT_SIGNATURE_DESC rootSignatureDesc;
    rootSignatureDesc.Init_1_1(_countof(rootParameters), rootParameters, 1, &sampler,
                               D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

    ComPtr<ID3DBlob> signature;
    ComPtr<ID3DBlob> error;
    TRY_DO(
        D3DX12SerializeVersionedRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1_1, &signature, &error
        ));
    TRY_DO(device->CreateRootSignature(0,
        signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&rootSignature)));

    return {rootSignature, postProcessingInput};
}

static Preset GetShaderPreset(const ShaderPreset preset, const ComPtr<ID3D12Device5> device)
{
    switch (preset)
    {
    case ShaderPreset::SPACE_3D: // NOLINT(bugprone-branch-clone)
        return GetSpace3dPreset(device);
    case ShaderPreset::POST_PROCESSING:
        return GetPostProcessingPreset(device);
    default:
        throw NativeException("Invalid shader preset.");
    }
}

std::unique_ptr<RasterPipeline> RasterPipeline::Create(
    NativeClient& client,
    const PipelineDescription& description,
    NativeErrorMessageFunc callback)
{
    // todo: shader compile error should not cause crash, except UI shader and post shader which could throw a OperationNotSupported at C# side
    // todo: test intentionally wrong shader code

    ComPtr<ID3DBlob> vertexShader = CompileShader(
        description.vertexShaderPath,
        "VSMain",
        "vs_5_0",
        callback);
    if (vertexShader == nullptr) return nullptr;

    ComPtr<ID3DBlob> pixelShader = CompileShader(
        description.pixelShaderPath,
        "PSMain",
        "ps_5_0",
        callback);
    if (pixelShader == nullptr) return nullptr;

    ComPtr<ID3D12RootSignature> rootSignature;
    std::vector<D3D12_INPUT_ELEMENT_DESC> inputLayout;

    std::tie(rootSignature, inputLayout) = GetShaderPreset(description.shaderPreset, client.GetDevice());

    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.pRootSignature = rootSignature.Get();
    psoDesc.InputLayout = {inputLayout.data(), static_cast<UINT>(inputLayout.size())};
    psoDesc.VS = CD3DX12_SHADER_BYTECODE(vertexShader.Get());
    psoDesc.PS = CD3DX12_SHADER_BYTECODE(pixelShader.Get());
    psoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
    psoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
    psoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
    psoDesc.DSVFormat = DXGI_FORMAT_D32_FLOAT;
    psoDesc.DepthStencilState.StencilEnable = FALSE;
    psoDesc.SampleMask = UINT_MAX;
    psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.NumRenderTargets = 1;
    psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
    psoDesc.SampleDesc.Count = 1;

    ComPtr<ID3D12PipelineState> pipelineState;
    TRY_DO(client.GetDevice()->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&pipelineState)));

    return std::make_unique<RasterPipeline>(client, rootSignature, pipelineState);
}

RasterPipeline::RasterPipeline(NativeClient& client,
                               ComPtr<ID3D12RootSignature> rootSignature, ComPtr<ID3D12PipelineState> pipelineState)
    : Object(client), m_rootSignature(rootSignature), m_pipelineState(pipelineState)
{
    NAME_D3D12_OBJECT_WITH_ID(m_rootSignature);
    NAME_D3D12_OBJECT_WITH_ID(m_pipelineState);

    for (UINT n = 0; n < NativeClient::FRAME_COUNT; n++)
    {
        TRY_DO(
            client.GetDevice()->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocators
                    [n])
            ));
        NAME_D3D12_OBJECT_INDEXED_WITH_ID(m_commandAllocators, n);
    }

    TRY_DO(client.GetDevice()->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
        m_commandAllocators[0].Get(), m_pipelineState.Get(), IID_PPV_ARGS(&m_commandList)
    ));

    NAME_D3D12_OBJECT_WITH_ID(m_commandList);
    TRY_DO(m_commandList->Close());
}

void RasterPipeline::Reset(const UINT frameIndex) const
{
    TRY_DO(m_commandAllocators[frameIndex]->Reset());
    TRY_DO(m_commandList->Reset(m_commandAllocators[frameIndex].Get(), m_pipelineState.Get()));
}

ComPtr<ID3D12GraphicsCommandList4> RasterPipeline::GetCommandList() const
{
    return m_commandList;
}

ComPtr<ID3D12RootSignature> RasterPipeline::GetRootSignature() const
{
    return m_rootSignature;
}
