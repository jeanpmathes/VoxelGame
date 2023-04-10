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

using Preset = std::tuple<ComPtr<ID3D12RootSignature>, std::vector<D3D12_INPUT_ELEMENT_DESC>, UINT>;

static Preset GetDraw2dPreset(uint64_t cbufferSize, ComPtr<ID3D12Device5> device)
{
    std::vector<D3D12_INPUT_ELEMENT_DESC> space3dInput =
    {
        {
            "POSITION", 0, DXGI_FORMAT_R32G32_FLOAT,
            0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        },
        {
            "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
            0, 8, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        },
        {
            "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
            0, 16, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
        },
    };

    ComPtr<ID3D12RootSignature> rootSignature;

    constexpr UINT minParameterCount = 2;
    CD3DX12_DESCRIPTOR_RANGE1 ranges[minParameterCount + 1];
    CD3DX12_ROOT_PARAMETER1 rootParameters[minParameterCount + 1];

    UINT parameter = 0;

    if (cbufferSize > 0)
    {
        ranges[parameter].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);
        rootParameters[parameter].InitAsDescriptorTable(1, &ranges[parameter], D3D12_SHADER_VISIBILITY_ALL);

        parameter++;
    }

    ranges[parameter].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 1);
    rootParameters[parameter].InitAsDescriptorTable(1, &ranges[parameter], D3D12_SHADER_VISIBILITY_ALL);

    parameter++;

    ranges[parameter].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1, 0);
    rootParameters[parameter].InitAsDescriptorTable(1, &ranges[parameter], D3D12_SHADER_VISIBILITY_PIXEL);

    parameter++;

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
    rootSignatureDesc.Init_1_1(parameter, rootParameters, 1, &sampler,
                               D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

    ComPtr<ID3DBlob> signature;
    ComPtr<ID3DBlob> error;
    TRY_DO(
        D3DX12SerializeVersionedRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1_1, &signature, &error
        ));
    TRY_DO(device->CreateRootSignature(0,
        signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&rootSignature)));

    return {rootSignature, space3dInput, parameter};
}

static Preset GetPostProcessingPreset(uint64_t cbufferSize, ComPtr<ID3D12Device5> device)
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

    constexpr UINT minParameterCount = 1;
    CD3DX12_DESCRIPTOR_RANGE1 ranges[minParameterCount + 1];
    CD3DX12_ROOT_PARAMETER1 rootParameters[minParameterCount + 1];

    UINT parameter = 0;

    if (cbufferSize > 0)
    {
        ranges[parameter].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);
        rootParameters[parameter].InitAsDescriptorTable(1, &ranges[parameter], D3D12_SHADER_VISIBILITY_ALL);

        parameter++;
    }

    ranges[parameter].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1, 0);
    rootParameters[parameter].InitAsDescriptorTable(1, &ranges[parameter], D3D12_SHADER_VISIBILITY_PIXEL);

    parameter++;
    
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
    rootSignatureDesc.Init_1_1(parameter, rootParameters, 1, &sampler,
                               D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

    ComPtr<ID3DBlob> signature;
    ComPtr<ID3DBlob> error;
    TRY_DO(
        D3DX12SerializeVersionedRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1_1, &signature, &error
        ));
    TRY_DO(device->CreateRootSignature(0,
        signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&rootSignature)));

    return {rootSignature, postProcessingInput, parameter};
}

static Preset GetShaderPreset(const ShaderPreset preset, const uint64_t cbufferSize, const ComPtr<ID3D12Device5> device)
{
    switch (preset)
    {
    case ShaderPreset::POST_PROCESSING: // NOLINT(bugprone-branch-clone)
        return GetPostProcessingPreset(cbufferSize, device);
    case ShaderPreset::DRAW_2D:
        return GetDraw2dPreset(cbufferSize, device);
    default:
        throw NativeException("Invalid shader preset.");
    }
}

static void ApplyPresetToPipeline(const ShaderPreset preset, D3D12_GRAPHICS_PIPELINE_STATE_DESC* desc)
{
    switch (preset)
    {
    case ShaderPreset::POST_PROCESSING: break;
    case ShaderPreset::DRAW_2D:
        {
            desc->DepthStencilState.DepthEnable = false;
            desc->RasterizerState.CullMode = D3D12_CULL_MODE_NONE;
        }
        break;
    default:
        throw NativeException("Invalid shader preset.");
    }
}

std::unique_ptr<RasterPipeline> RasterPipeline::Create(
    NativeClient& client,
    const PipelineDescription& description,
    NativeErrorMessageFunc callback)
{
    // todo: shader compile error should not cause crash
    // todo: test intentionally wrong shader code
    // todo: test missing shader file

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
    UINT parameterCount;

    std::tie(rootSignature, inputLayout, parameterCount) = GetShaderPreset(
        description.shaderPreset, description.bufferSize,
        client.GetDevice());

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

    ApplyPresetToPipeline(description.shaderPreset, &psoDesc);

    ComPtr<ID3D12PipelineState> pipelineState;
    TRY_DO(client.GetDevice()->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&pipelineState)));

    REQUIRE(parameterCount > 0);

    ComPtr<ID3D12DescriptorHeap> descriptorHeap = nv_helpers_dx12::CreateDescriptorHeap(
        client.GetDevice().Get(), parameterCount,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true);

    std::unique_ptr<ShaderBuffer> shaderBuffer;

    if (description.bufferSize > 0)
    {
        shaderBuffer = std::make_unique<ShaderBuffer>(client, description.bufferSize);
        shaderBuffer->CreateResourceView(descriptorHeap);
    }

    return std::make_unique<RasterPipeline>(
        client, description.shaderPreset, std::move(shaderBuffer),
        descriptorHeap, rootSignature, pipelineState);
}

RasterPipeline::RasterPipeline(NativeClient& client, ShaderPreset preset, std::unique_ptr<ShaderBuffer> buffer,
                               ComPtr<ID3D12DescriptorHeap> descriptorHeap,
                               ComPtr<ID3D12RootSignature> rootSignature,
                               ComPtr<ID3D12PipelineState> pipelineState)
    : Object(client)
      , m_preset(preset)
      , m_descriptorHeap(descriptorHeap)
      , m_rootSignature(rootSignature)
      , m_pipelineState(pipelineState)
      , m_shaderBuffer(std::move(buffer))
{
    NAME_D3D12_OBJECT_WITH_ID(m_descriptorHeap);
    NAME_D3D12_OBJECT_WITH_ID(m_rootSignature);
    NAME_D3D12_OBJECT_WITH_ID(m_pipelineState);
}

void RasterPipeline::SetPipeline(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    commandList->SetPipelineState(m_pipelineState.Get());
}

ComPtr<ID3D12RootSignature> RasterPipeline::GetRootSignature() const
{
    return m_rootSignature;
}

ShaderBuffer* RasterPipeline::GetShaderBuffer() const
{
    return m_shaderBuffer.get();
}

void RasterPipeline::CreateResourceView(ComPtr<ID3D12Resource> resource) const
{
    REQUIRE(m_preset == ShaderPreset::POST_PROCESSING);

    GetClient().GetDevice()->CreateShaderResourceView(resource.Get(), nullptr, GetCpuResourceHandle(0));
}

void RasterPipeline::CreateResourceViews(
    const std::vector<D3D12_CONSTANT_BUFFER_VIEW_DESC>& cbuffers,
    const std::vector<std::tuple<ComPtr<ID3D12Resource>, D3D12_SHADER_RESOURCE_VIEW_DESC>>& textures)
{
    REQUIRE(m_preset == ShaderPreset::DRAW_2D);

    const UINT lastIndex = static_cast<UINT>(cbuffers.size() + textures.size()) - 1;
    const UINT requiredSlots = GetResourceSlot(lastIndex) + 1;

    m_descriptorHeap = nv_helpers_dx12::CreateDescriptorHeap(
        GetClient().GetDevice().Get(), requiredSlots,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true);
    NAME_D3D12_OBJECT_WITH_ID(m_descriptorHeap);

    if (m_shaderBuffer != nullptr)
    {
        m_shaderBuffer->CreateResourceView(m_descriptorHeap);
    }
    
    UINT slot = 0;

    for (size_t index = 0; index < cbuffers.size(); index++)
    {
        GetClient().GetDevice()->CreateConstantBufferView(&cbuffers[index], GetCpuResourceHandle(slot));
        slot++;
    }

    for (size_t index = 0; index < textures.size(); index++)
    {
        const auto& [resource, desc] = textures[index];
        GetClient().GetDevice()->CreateShaderResourceView(resource.Get(), &desc, GetCpuResourceHandle(slot));
        slot++;
    }
}

void RasterPipeline::SetupHeaps(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    ID3D12DescriptorHeap* heaps[] = {m_descriptorHeap.Get()};
    commandList->SetDescriptorHeaps(_countof(heaps), heaps);
}

void RasterPipeline::SetupRootDescriptorTable(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    if (m_shaderBuffer != nullptr)
    {
        commandList->SetGraphicsRootDescriptorTable(0, m_descriptorHeap->GetGPUDescriptorHandleForHeapStart());
    }

    if (m_preset == ShaderPreset::POST_PROCESSING)
    {
        commandList->SetGraphicsRootDescriptorTable(GetResourceSlot(0), GetGpuResourceHandle(0));
    }
}

void RasterPipeline::BindDescriptor(ComPtr<ID3D12GraphicsCommandList4> commandList,
                                    const UINT slot, const UINT descriptor) const
{
    commandList->SetGraphicsRootDescriptorTable(GetResourceSlot(slot), GetGpuResourceHandle(descriptor));
}

UINT RasterPipeline::GetResourceSlot(UINT index) const
{
    const UINT offset = m_shaderBuffer == nullptr ? 0 : 1;
    return offset + index;
}

D3D12_CPU_DESCRIPTOR_HANDLE RasterPipeline::GetCpuResourceHandle(UINT index) const
{
    const UINT offset = GetResourceSlot(index);

    return CD3DX12_CPU_DESCRIPTOR_HANDLE(m_descriptorHeap->GetCPUDescriptorHandleForHeapStart(),
                                         offset,
                                         GetClient().GetCbvSrvUavHeapIncrement());
}

D3D12_GPU_DESCRIPTOR_HANDLE RasterPipeline::GetGpuResourceHandle(UINT index) const
{
    const UINT offset = GetResourceSlot(index);

    return CD3DX12_GPU_DESCRIPTOR_HANDLE(m_descriptorHeap->GetGPUDescriptorHandleForHeapStart(),
                                         offset,
                                         GetClient().GetCbvSrvUavHeapIncrement());
}
