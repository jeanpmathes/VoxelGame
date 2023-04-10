#include "stdafx.h"

draw2d::Pipeline::Pipeline(NativeClient& client, RasterPipeline* raster, Callback callback)
    : m_raster(raster), m_callback(callback), m_device(client.GetDevice())
{
    auto addBuffer = [this, &client](const BOOL value)
    {
        constexpr UINT size = sizeof(BOOL);
        uint64_t alignedSize = size;

        const ComPtr<ID3D12Resource> constantBuffer = nv_helpers_dx12::CreateConstantBuffer(
            client.GetDevice().Get(), &alignedSize,
            D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
            nv_helpers_dx12::kUploadHeapProps);

        D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc;
        cbvDesc.BufferLocation = constantBuffer->GetGPUVirtualAddress();
        cbvDesc.SizeInBytes = static_cast<UINT>(alignedSize);

        this->m_cbuffers.push_back(constantBuffer);
        this->m_constantBufferViews.push_back(cbvDesc);

        BOOL* pData;
        TRY_DO(constantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));
        *pData = value;
        constantBuffer->Unmap(0, nullptr);
    };

    addBuffer(TRUE);
    addBuffer(FALSE);
}

void draw2d::Pipeline::PopulateCommandListSetup(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    m_raster->SetPipeline(commandList);
    commandList->SetGraphicsRootSignature(m_raster->GetRootSignature().Get());

    if (m_initialized)
    {
        m_raster->SetupHeaps(commandList);
        m_raster->SetupRootDescriptorTable(commandList);
    }
}

void draw2d::Pipeline::PopulateCommandListDrawing(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    constexpr UINT booleanSlot = 0;
    constexpr UINT textureSlot = 1;

    constexpr UINT trueDescriptorIndex = 0;
    constexpr UINT falseDescriptorIndex = 1;
    constexpr UINT firstTextureDescriptorIndex = 2;

    const Drawer drawer
    {
        .initializeTextures = [](Texture** textures, const UINT textureCount, Pipeline* ctx)
        {
            REQUIRE(textureCount > 0);
            
            ctx->m_textures.clear();
            ctx->m_textures.reserve(textureCount);

            for (UINT i = 0; i < textureCount; i++)
            {
                Texture& texture = *textures[i];
                ctx->m_textures.push_back(std::make_tuple(texture.GetResource(), texture.GetView()));
            }

            ctx->m_raster->CreateResourceViews(ctx->m_constantBufferViews, ctx->m_textures);

            ctx->m_raster->SetupHeaps(ctx->m_currentCommandList);
            ctx->m_raster->SetupRootDescriptorTable(ctx->m_currentCommandList);
            
            ctx->m_currentTextureIndex = 0;
            ctx->m_currentUseTexture = FALSE;

            ctx->m_raster->BindDescriptor(ctx->m_currentCommandList, booleanSlot, falseDescriptorIndex);
            ctx->m_raster->BindDescriptor(ctx->m_currentCommandList, textureSlot, firstTextureDescriptorIndex);

            ctx->m_initialized = true;
        },
        .drawBuffer = [](
        const Vertex* vertices, const UINT vertexCount, const UINT textureIndex, const BOOL useTexture,
        Pipeline* ctx)
        {
            REQUIRE(ctx->m_initialized);
            
            if (ctx->m_currentUseTexture != useTexture)
            {
                ctx->m_currentUseTexture = useTexture;
                ctx->m_raster->BindDescriptor(ctx->m_currentCommandList,
                                              booleanSlot, useTexture ? trueDescriptorIndex : falseDescriptorIndex);
            }

            if (ctx->m_currentTextureIndex != textureIndex && useTexture)
            {
                ctx->m_currentTextureIndex = textureIndex;
                ctx->m_raster->BindDescriptor(ctx->m_currentCommandList,
                                              textureSlot, firstTextureDescriptorIndex + textureIndex);
            }

            const ComPtr<ID3D12Resource> vertexBuffer = nv_helpers_dx12::CreateBuffer(
                ctx->m_device.Get(), vertexCount * sizeof(Vertex),
                D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
                nv_helpers_dx12::kUploadHeapProps);

            ctx->m_vertexBuffers.push_back(vertexBuffer);

            Vertex* pData;
            TRY_DO(vertexBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));
            memcpy(pData, vertices, vertexCount * sizeof(Vertex));
            vertexBuffer->Unmap(0, nullptr);

            D3D12_VERTEX_BUFFER_VIEW vertexBufferView;
            vertexBufferView.BufferLocation = vertexBuffer->GetGPUVirtualAddress();
            vertexBufferView.StrideInBytes = sizeof(Vertex);
            vertexBufferView.SizeInBytes = vertexCount * sizeof(Vertex);

            ctx->m_currentCommandList->IASetVertexBuffers(0, 1, &vertexBufferView);
            ctx->m_currentCommandList->DrawInstanced(vertexCount, 1, 0, 0);
        },
        .ctx = this
    };

    m_vertexBuffers.clear();

    if (m_initialized)
    {
        m_raster->BindDescriptor(commandList, booleanSlot, falseDescriptorIndex);
        m_raster->BindDescriptor(commandList, textureSlot, firstTextureDescriptorIndex);
    }

    commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    m_currentCommandList = commandList.Get();
    m_callback(drawer);
    m_currentCommandList = nullptr;
}
