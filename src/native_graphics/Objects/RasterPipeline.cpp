#include "stdafx.h"

#include <utility>

using Preset = std::tuple<std::shared_ptr<ShaderResources>, std::shared_ptr<RasterPipeline::Bindings>, std::vector<
                              D3D12_INPUT_ELEMENT_DESC>>;

namespace
{
    void EnsureValidDescription(RasterPipelineDescription const& description)
    {
        Require(description.vertexShaderPath != nullptr);
        Require(description.pixelShaderPath != nullptr);

        Require(
            description.shaderPreset == ShaderPreset::POST_PROCESSING || description.shaderPreset ==
            ShaderPreset::DRAW_2D || description.shaderPreset == ShaderPreset::SPATIAL_EFFECT);

        Require(description.bufferSize < D3D12_REQ_IMMEDIATE_CONSTANT_BUFFER_ELEMENT_COUNT * 4 * 4);

        auto ensureValidEnum = [&]<typename E>(
            E const&                         field,
            std::vector<ShaderPreset> const& presets,
            std::vector<E> const&            values)
        {
            if (std::ranges::find(presets, description.shaderPreset) != presets.end()) Require(
                std::ranges::find(values, field) != values.end());
            else Require(field == E{});
        };

        ensureValidEnum(description.topology, {ShaderPreset::SPATIAL_EFFECT}, {Topology::TRIANGLE, Topology::LINE});

        ensureValidEnum(
            description.filter,
            {ShaderPreset::POST_PROCESSING, ShaderPreset::DRAW_2D},
            {Filter::LINEAR, Filter::CLOSEST});
    }

    D3D12_FILTER GetFilter(RasterPipelineDescription const& description)
    {
        switch (description.filter)
        {
        case Filter::LINEAR:
            return D3D12_FILTER_MIN_MAG_MIP_LINEAR;
        case Filter::CLOSEST:
            return D3D12_FILTER_MIN_MAG_MIP_POINT;
        }

        throw NativeException("Invalid filter.");
    }

    Preset GetPostProcessingPreset(
        RasterPipelineDescription const& description,
        ShaderBuffer const*              shaderBuffer,
        NativeClient const&              client)
    {
        std::vector<D3D12_INPUT_ELEMENT_DESC> input = {
            {"POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0},
            {
                "TEXCOORD",
                0,
                DXGI_FORMAT_R32G32_FLOAT,
                0,
                D3D12_APPEND_ALIGNED_ELEMENT,
                D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                0
            }
        };

        auto resources = std::make_shared<ShaderResources>();
        auto bindings  = std::make_shared<RasterPipeline::Bindings>(ShaderPreset::POST_PROCESSING);

        resources->Initialize(
            [&client, &shaderBuffer, &description, &bindings](auto& graphics)
            {
                graphics.EnableInputAssembler();
                graphics.AddStaticSampler({.reg = 0}, GetFilter(description));

                if (shaderBuffer != nullptr) graphics.AddConstantBufferView(
                    shaderBuffer->GetGPUVirtualAddress(),
                    {.reg = 0});

                graphics.AddRootConstant(
                    [&client]() -> ShaderResources::Value32
                    {
                        return {.floating = static_cast<FLOAT>(client.GetTotalRenderTime())};
                    },
                    {.reg = 0, .space = 1});

                graphics.AddHeapDescriptorTable(
                    [&](auto& table) { bindings->PostProcessing().input = table.AddShaderResourceView({.reg = 0}); });
            },
            [](auto&)
            {
                // No compute resources.
            },
            client.GetDevice());

        return {std::move(resources), std::move(bindings), input};
    }

    Preset GetDraw2dPreset(
        RasterPipelineDescription const& description,
        ShaderBuffer const*              shaderBuffer,
        NativeClient const&              client)
    {
        std::vector<D3D12_INPUT_ELEMENT_DESC> input = {
            {"POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0},
            {
                "TEXCOORD",
                0,
                DXGI_FORMAT_R32G32_FLOAT,
                0,
                D3D12_APPEND_ALIGNED_ELEMENT,
                D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                0
            },
            {
                "COLOR",
                0,
                DXGI_FORMAT_R32G32B32A32_FLOAT,
                0,
                D3D12_APPEND_ALIGNED_ELEMENT,
                D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                0
            },
        };

        auto resources = std::make_shared<ShaderResources>();
        auto bindings  = std::make_shared<RasterPipeline::Bindings>(ShaderPreset::DRAW_2D);

        resources->Initialize(
            [&client, &shaderBuffer, &description, &bindings](auto& graphics)
            {
                graphics.EnableInputAssembler();
                graphics.AddStaticSampler({.reg = 0}, GetFilter(description));

                if (shaderBuffer != nullptr) graphics.AddConstantBufferView(
                    shaderBuffer->GetGPUVirtualAddress(),
                    {.reg = 0});

                graphics.AddRootConstant(
                    [&client]() -> ShaderResources::Value32
                    {
                        return {.floating = static_cast<FLOAT>(client.GetTotalRenderTime())};
                    },
                    {.reg = 0, .space = 1});

                bindings->Draw2D().booleans = graphics.AddConstantBufferViewDescriptorSelectionList({.reg = 1});
                bindings->Draw2D().textures = graphics.AddShaderResourceViewDescriptorSelectionList(
                    {.reg = 0},
                    ShaderResources::UNBOUNDED);
            },
            [](auto&)
            {
                // No compute resources.
            },
            client.GetDevice());

        return {std::move(resources), std::move(bindings), input};
    }

    Preset GetSpatialEffectPreset(RasterPipelineDescription const&, ShaderBuffer const*, NativeClient const& client)
    {
        std::vector<D3D12_INPUT_ELEMENT_DESC> input = {
            {"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0},
            {
                "DATA",
                0,
                DXGI_FORMAT_R32_UINT,
                0,
                D3D12_APPEND_ALIGNED_ELEMENT,
                D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                0
            },
        };

        Space* space = client.GetSpace();
        Require(space != nullptr);

        std::shared_ptr<ShaderResources>          resources = space->GetShaderResources();
        std::shared_ptr<RasterPipeline::Bindings> bindings  = space->GetEffectBindings();

        return {std::move(resources), std::move(bindings), input};
    }

    Preset GetShaderPreset(
        RasterPipelineDescription const& description,
        ShaderBuffer const*              shaderBuffer,
        NativeClient const&              client)
    {
        using enum ShaderPreset;

        switch (description.shaderPreset)
        {
        case POST_PROCESSING:
            return GetPostProcessingPreset(description, shaderBuffer, client);
        case DRAW_2D:
            return GetDraw2dPreset(description, shaderBuffer, client);
        case SPATIAL_EFFECT:
            return GetSpatialEffectPreset(description, shaderBuffer, client);
        default:
            throw NativeException("Invalid shader preset.");
        }
    }

    void ApplyDescriptionToPipeline(
        RasterPipelineDescription const&    description,
        D3D12_GRAPHICS_PIPELINE_STATE_DESC* desc,
        D3D12_PRIMITIVE_TOPOLOGY*           topology)
    {
        switch (description.shaderPreset)
        {
        case ShaderPreset::POST_PROCESSING:
        {
            *topology = D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP;

            desc->DepthStencilState.DepthEnable    = TRUE;
            desc->DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;
            desc->DepthStencilState.DepthFunc      = D3D12_COMPARISON_FUNC_LESS_EQUAL;
        }
        case ShaderPreset::DRAW_2D:
        {
            *topology = D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST;

            desc->RasterizerState.CullMode      = D3D12_CULL_MODE_NONE;
            desc->DepthStencilState.DepthEnable = FALSE;

            desc->BlendState.RenderTarget[0].BlendEnable    = TRUE;
            desc->BlendState.RenderTarget[0].SrcBlend       = D3D12_BLEND_SRC_ALPHA;
            desc->BlendState.RenderTarget[0].DestBlend      = D3D12_BLEND_INV_SRC_ALPHA;
            desc->BlendState.RenderTarget[0].BlendOp        = D3D12_BLEND_OP_ADD;
            desc->BlendState.RenderTarget[0].SrcBlendAlpha  = D3D12_BLEND_SRC_ALPHA;
            desc->BlendState.RenderTarget[0].DestBlendAlpha = D3D12_BLEND_INV_SRC_ALPHA;
            desc->BlendState.RenderTarget[0].BlendOpAlpha   = D3D12_BLEND_OP_ADD;
        }
        break;
        case ShaderPreset::SPATIAL_EFFECT:
        {
            switch (description.topology)
            {
            case Topology::TRIANGLE:
                *topology = D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
                desc->PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
                break;
            case Topology::LINE:
                *topology = D3D_PRIMITIVE_TOPOLOGY_LINELIST;
                desc->PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_LINE;
                break;
            }

            desc->RasterizerState.CullMode = D3D12_CULL_MODE_NONE;

            desc->DepthStencilState.DepthEnable    = TRUE;
            desc->DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;
            desc->DepthStencilState.DepthFunc      = D3D12_COMPARISON_FUNC_LESS_EQUAL;
        }
        break;
        default:
            throw NativeException("Invalid shader preset.");
        }
    }

    std::optional<std::pair<ComPtr<ID3DBlob>, ComPtr<ID3DBlob>>> CompileShaders(
        NativeClient&                    client,
        RasterPipelineDescription const& description,
        NativeErrorFunc                  callback)
    {
        ComPtr<IDxcBlob> const vertexShader = CompileShader(
            description.vertexShaderPath,
            L"VSMain",
            L"vs_6_0",
            VG_SHADER_REGISTRY(client),
            callback);
        if (vertexShader == nullptr) return std::nullopt;

        ComPtr<ID3DBlob> vertexShaderBlob;
        TryDo(vertexShader->QueryInterface(IID_PPV_ARGS(&vertexShaderBlob)));

        ComPtr<IDxcBlob> const pixelShader = CompileShader(
            description.pixelShaderPath,
            L"PSMain",
            L"ps_6_0",
            VG_SHADER_REGISTRY(client),
            callback);
        if (pixelShader == nullptr) return std::nullopt;

        ComPtr<ID3DBlob> pixelShaderBlob;
        TryDo(pixelShader->QueryInterface(IID_PPV_ARGS(&pixelShaderBlob)));

        return std::make_pair(vertexShaderBlob, pixelShaderBlob);
    }

    std::wstring CreateName(RasterPipelineDescription const& description)
    {
        using enum ShaderPreset;

        std::wstring preset;
        switch (description.shaderPreset)
        {
        case POST_PROCESSING:
            preset = L"PostProcessing";
            break;
        case DRAW_2D:
            preset = L"Draw2D";
            break;
        case SPATIAL_EFFECT:
            preset = L"SpatialEffect";
            break;
        }

        std::wstring const vertex = std::filesystem::path(description.vertexShaderPath).stem();
        std::wstring const pixel  = std::filesystem::path(description.pixelShaderPath).stem();

        std::wstring name;

        if (vertex != pixel) name = std::format(L"{} - ({}, {})", preset, vertex, pixel);
        else name                 = std::format(L"{} - {}", preset, vertex);

        return name;
    }
}

std::unique_ptr<RasterPipeline> RasterPipeline::Create(
    NativeClient&                    client,
    RasterPipelineDescription const& description,
    NativeErrorFunc                  callback)
{
    EnsureValidDescription(description);

    auto shaders = CompileShaders(client, description, callback);
    if (!shaders.has_value()) return nullptr;
    auto [vertexShaderBlob, pixelShaderBlob] = shaders.value();

    std::unique_ptr<ShaderBuffer> shaderBuffer;
    if (description.bufferSize > 0) shaderBuffer = std::make_unique<ShaderBuffer>(client, description.bufferSize);

    auto [resources, bindings, inputLayout] = GetShaderPreset(description, shaderBuffer.get(), client);

    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.pRootSignature                     = resources->GetGraphicsRootSignature().Get();
    psoDesc.InputLayout                        = {inputLayout.data(), static_cast<UINT>(inputLayout.size())};
    psoDesc.VS                                 = CD3DX12_SHADER_BYTECODE(vertexShaderBlob.Get());
    psoDesc.PS                                 = CD3DX12_SHADER_BYTECODE(pixelShaderBlob.Get());
    psoDesc.RasterizerState                    = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
    psoDesc.BlendState                         = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
    psoDesc.DepthStencilState                  = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
    psoDesc.DSVFormat                          = DXGI_FORMAT_D32_FLOAT;
    psoDesc.DepthStencilState.DepthEnable      = FALSE;
    psoDesc.DepthStencilState.StencilEnable    = FALSE;
    psoDesc.SampleMask                         = UINT_MAX;
    psoDesc.PrimitiveTopologyType              = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.NumRenderTargets                   = 1;
    psoDesc.RTVFormats[0]                      = DXGI_FORMAT_B8G8R8A8_UNORM;
    psoDesc.SampleDesc.Count                   = 1;

    D3D12_PRIMITIVE_TOPOLOGY topology = {};
    ApplyDescriptionToPipeline(description, &psoDesc, &topology);

    ComPtr<ID3D12PipelineState> pipelineState;
    TryDo(client.GetDevice()->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&pipelineState)));

    PipelineConfiguration configuration = {description.shaderPreset, topology, CreateName(description)};
    PipelineObjects       objects = {std::move(shaderBuffer), std::move(resources), std::move(bindings), pipelineState};

    return std::make_unique<RasterPipeline>(client, std::move(configuration), std::move(objects));
}

std::shared_ptr<RasterPipeline::Bindings> RasterPipeline::SetupEffectBindings(
    NativeClient const&           client,
    ShaderResources::Description& description)
{
    auto bindings = std::make_shared<Bindings>(ShaderPreset::SPATIAL_EFFECT);

    description.EnableInputAssembler();

    description.AddHeapDescriptorTable(
        [&bindings](auto& table)
        {
            bindings->SpatialEffect().customData   = table.AddConstantBufferView({.reg = 0});
            bindings->SpatialEffect().instanceData = table.AddConstantBufferView({.reg = 1});
        });

    description.AddRootConstant(
        [&client]() -> ShaderResources::Value32
        {
            return {.floating = static_cast<FLOAT>(client.GetTotalRenderTime())};
        },
        {.reg = 0, .space = 1});

    return bindings;
}

RasterPipeline::RasterPipeline(NativeClient& client, PipelineConfiguration configuration, PipelineObjects objects)
    : Object(client)
  , m_preset(configuration.preset)
  , m_topology(configuration.topology)
  , m_name(std::move(configuration.name))
  , m_resources(std::move(objects.resources))
  , m_bindings(std::move(objects.bindings))
  , m_pipelineState(std::move(objects.pipelineState))
  , m_shaderBuffer(std::move(objects.shaderBuffer)) { NAME_D3D12_OBJECT_WITH_ID(m_pipelineState); }

void RasterPipeline::SetPipeline(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    commandList->SetPipelineState(m_pipelineState.Get());

    if (m_preset != ShaderPreset::SPATIAL_EFFECT)
        // The space class already sets the root signature.
        commandList->SetGraphicsRootSignature(m_resources->GetGraphicsRootSignature().Get());

    commandList->IASetPrimitiveTopology(GetTopology());
}

void RasterPipeline::BindResources(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    if (m_preset == ShaderPreset::SPATIAL_EFFECT)
    {
        // The space class owns the resources and will bind them.
        m_update = true;

        if (m_shaderBuffer != nullptr)
            m_resources->CreateConstantBufferView(
                GetBindings().SpatialEffect().customData,
                0,
                m_shaderBuffer->GetDescriptor());
    }
    else
    {
        m_resources->Update();
        m_update = true;

        m_resources->Bind(commandList);
    }
}

RasterPipeline::Bindings& RasterPipeline::GetBindings() const { return *m_bindings; }

ShaderPreset RasterPipeline::GetPreset() const { return m_preset; }

LPCWSTR RasterPipeline::GetName() const { return m_name.c_str(); }


D3D12_PRIMITIVE_TOPOLOGY RasterPipeline::GetTopology() const { return m_topology; }

ShaderBuffer* RasterPipeline::GetShaderBuffer() const { return m_shaderBuffer.get(); }

void RasterPipeline::CreateConstantBufferView(
    ShaderResources::Table::Entry const                  entry,
    UINT const                                           index,
    ShaderResources::ConstantBufferViewDescriptor const& descriptor)
{
    EnsureFirstUpdate();
    m_resources->CreateConstantBufferView(entry, index, descriptor);
}

void RasterPipeline::CreateShaderResourceView(
    ShaderResources::Table::Entry const                  entry,
    UINT const                                           index,
    ShaderResources::ShaderResourceViewDescriptor const& descriptor)
{
    EnsureFirstUpdate();
    m_resources->CreateShaderResourceView(entry, index, descriptor);
}

void RasterPipeline::CreateUnorderedAccessView(
    ShaderResources::Table::Entry const                   entry,
    UINT const                                            index,
    ShaderResources::UnorderedAccessViewDescriptor const& descriptor)
{
    EnsureFirstUpdate();
    m_resources->CreateUnorderedAccessView(entry, index, descriptor);
}

void RasterPipeline::EnsureFirstUpdate()
{
    if (m_update) return;

    m_resources->Update();
    m_update = true;
}
