#include "stdafx.h"

#include <utility>

using Preset = std::tuple<ShaderResources, RasterPipeline::Bindings, std::vector<D3D12_INPUT_ELEMENT_DESC>>;

namespace 
{
    Preset GetDraw2dPreset(const ShaderBuffer* shaderBuffer, ComPtr<ID3D12Device5> device)
    {
        std::vector<D3D12_INPUT_ELEMENT_DESC> input =
        {
            {
                "POSITION", 0, DXGI_FORMAT_R32G32_FLOAT,
                0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            },
            {
                "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
                0, D3D12_APPEND_ALIGNED_ELEMENT, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            },
            {
                "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
                0, D3D12_APPEND_ALIGNED_ELEMENT, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            },
        };

        ShaderResources resources;
        RasterPipeline::Bindings bindings(ShaderPreset::DRAW_2D);

        resources.Initialize(
            [&](auto& graphics)
            {
                graphics.EnableInputAssembler();
                graphics.AddStaticSampler({.reg = 0});

                if (shaderBuffer != nullptr)
                {
                    graphics.AddConstantBufferView(shaderBuffer->GetGPUVirtualAddress(), {.reg = 0});
                }

                bindings.Draw2D().booleans = graphics.AddConstantBufferViewDescriptorSelectionList({.reg = 1});
                bindings.Draw2D().textures = graphics.AddShaderResourceViewDescriptorSelectionList({.reg = 0});
            },
            [&](auto&)
            {
            },
            device);

        return {std::move(resources), std::move(bindings), input};
    }

    Preset GetPostProcessingPreset(const ShaderBuffer* shaderBuffer, ComPtr<ID3D12Device5> device)
    {
        std::vector<D3D12_INPUT_ELEMENT_DESC> input =
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

        ShaderResources resources;
        RasterPipeline::Bindings bindings(ShaderPreset::POST_PROCESSING);

        resources.Initialize(
            [&](auto& graphics)
            {
                graphics.EnableInputAssembler();
                graphics.AddStaticSampler({.reg = 0});

                if (shaderBuffer != nullptr)
                {
                    graphics.AddConstantBufferView(shaderBuffer->GetGPUVirtualAddress(), {.reg = 0});
                }

                graphics.AddHeapDescriptorTable([&](auto& table)
                {
                    bindings.PostProcessing().input = table.AddShaderResourceView({.reg = 0});
                });
            },
            [&](auto&)
            {
            },
            device);

        return {std::move(resources), std::move(bindings), input};
    }

    Preset GetShaderPreset(const ShaderPreset preset, const ShaderBuffer* shaderBuffer,
                           const ComPtr<ID3D12Device5> device)
    {
        switch (preset)
        {
        case ShaderPreset::POST_PROCESSING: // NOLINT(bugprone-branch-clone)
            return GetPostProcessingPreset(shaderBuffer, device);
        case ShaderPreset::DRAW_2D:
            return GetDraw2dPreset(shaderBuffer, device);
        default:
            throw NativeException("Invalid shader preset.");
        }
    }

    void ApplyPresetToPipeline(const ShaderPreset preset, D3D12_GRAPHICS_PIPELINE_STATE_DESC* desc)
    {
        switch (preset)
        {
        case ShaderPreset::POST_PROCESSING:
            {
                desc->DepthStencilState.DepthEnable = true;
                desc->DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;
                desc->DepthStencilState.DepthFunc = D3D12_COMPARISON_FUNC_LESS_EQUAL;
            }
        case ShaderPreset::DRAW_2D:
            {
                desc->DepthStencilState.DepthEnable = false;
                desc->RasterizerState.CullMode = D3D12_CULL_MODE_NONE;

                desc->BlendState.RenderTarget[0].BlendEnable = true;
                desc->BlendState.RenderTarget[0].SrcBlend = D3D12_BLEND_SRC_ALPHA;
                desc->BlendState.RenderTarget[0].DestBlend = D3D12_BLEND_INV_SRC_ALPHA;
            }
            break;
        default:
            throw NativeException("Invalid shader preset.");
        }
    }

    std::optional<std::pair<ComPtr<ID3DBlob>, ComPtr<ID3DBlob>>> CompileShaders(
        NativeClient& client,
        const PipelineDescription& description,
        NativeErrorFunc callback)
    {
        const ComPtr<IDxcBlob> vertexShader = CompileShader(
        description.vertexShaderPath,
        L"VSMain",
        L"vs_6_0",
        VG_SHADER_REGISTRY(client),
        callback);
        if (vertexShader == nullptr) return std::nullopt;

        ComPtr<ID3DBlob> vertexShaderBlob;
        TRY_DO(vertexShader->QueryInterface(IID_PPV_ARGS(&vertexShaderBlob)));

        const ComPtr<IDxcBlob> pixelShader = CompileShader(
            description.pixelShaderPath,
            L"PSMain",
            L"ps_6_0",
            VG_SHADER_REGISTRY(client),
            callback);
        if (pixelShader == nullptr) return std::nullopt;

        ComPtr<ID3DBlob> pixelShaderBlob;
        TRY_DO(pixelShader->QueryInterface(IID_PPV_ARGS(&pixelShaderBlob)));

        return std::make_pair(vertexShaderBlob, pixelShaderBlob);
    }
}

std::unique_ptr<RasterPipeline> RasterPipeline::Create(
    NativeClient& client,
    const PipelineDescription& description,
    NativeErrorFunc callback)
{
    auto shaders = CompileShaders(client, description, callback);
    if (!shaders.has_value()) return nullptr;
    auto [vertexShaderBlob, pixelShaderBlob] = shaders.value();

    std::unique_ptr<ShaderBuffer> shaderBuffer;
    if (description.bufferSize > 0)
    {
        shaderBuffer = std::make_unique<ShaderBuffer>(client, description.bufferSize);
    }

    auto [resources, bindings, inputLayout] = GetShaderPreset(
        description.shaderPreset, shaderBuffer.get(),
        client.GetDevice());

    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.pRootSignature = resources.GetGraphicsRootSignature().Get();
    psoDesc.InputLayout = {inputLayout.data(), static_cast<UINT>(inputLayout.size())};
    psoDesc.VS = CD3DX12_SHADER_BYTECODE(vertexShaderBlob.Get());
    psoDesc.PS = CD3DX12_SHADER_BYTECODE(pixelShaderBlob.Get());
    psoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
    psoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
    psoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
    psoDesc.DSVFormat = DXGI_FORMAT_D32_FLOAT;
    psoDesc.DepthStencilState.DepthEnable = FALSE;
    psoDesc.DepthStencilState.StencilEnable = FALSE;
    psoDesc.SampleMask = UINT_MAX;
    psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.NumRenderTargets = 1;
    psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
    psoDesc.SampleDesc.Count = 1;

    ApplyPresetToPipeline(description.shaderPreset, &psoDesc);

    ComPtr<ID3D12PipelineState> pipelineState;
    TRY_DO(client.GetDevice()->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&pipelineState)));

    return std::make_unique<RasterPipeline>(
        client,
        description.shaderPreset,
        std::move(shaderBuffer),
        std::move(resources),
        std::move(bindings),
        pipelineState);
}

RasterPipeline::RasterPipeline(
    NativeClient& client, const ShaderPreset preset,
    std::unique_ptr<ShaderBuffer> shaderBuffer,
    ShaderResources&& resources,
    Bindings bindings,
    ComPtr<ID3D12PipelineState> pipelineState)
    : Object(client)
      , m_preset(preset)
      , m_resources(std::move(resources))
      , m_bindings(std::move(bindings))
      , m_pipelineState(std::move(pipelineState))
      , m_shaderBuffer(std::move(shaderBuffer))
{
    NAME_D3D12_OBJECT_WITH_ID(m_pipelineState);
}

void RasterPipeline::SetPipeline(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    commandList->SetGraphicsRootSignature(m_resources.GetGraphicsRootSignature().Get());
    commandList->SetPipelineState(m_pipelineState.Get());
}

void RasterPipeline::BindResources(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    m_resources.Update();
    m_update = true;

    m_resources.Bind(commandList);
}

RasterPipeline::Bindings& RasterPipeline::GetBindings()
{
    return m_bindings;
}

ShaderBuffer* RasterPipeline::GetShaderBuffer() const
{
    return m_shaderBuffer.get();
}

void RasterPipeline::CreateConstantBufferView(
    const ShaderResources::Table::Entry entry, const UINT index,
    const ShaderResources::ConstantBufferViewDescriptor& descriptor)
{
    EnsureFirstUpdate();
    m_resources.CreateConstantBufferView(entry, index, descriptor);   
}

void RasterPipeline::CreateShaderResourceView(
    const ShaderResources::Table::Entry entry, const UINT index,
    const ShaderResources::ShaderResourceViewDescriptor& descriptor)
{
    EnsureFirstUpdate();
    m_resources.CreateShaderResourceView(entry, index, descriptor);
}

void RasterPipeline::CreateUnorderedAccessView(
    const ShaderResources::Table::Entry entry, const UINT index,
    const ShaderResources::UnorderedAccessViewDescriptor& descriptor)
{
    EnsureFirstUpdate();
    m_resources.CreateUnorderedAccessView(entry, index, descriptor);
}

void RasterPipeline::EnsureFirstUpdate()
{
    if (m_update) return;

    m_resources.Update();
    m_update = true;
}
