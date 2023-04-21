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
}

static constexpr UINT BOOLEAN_SLOT = 0;
static constexpr UINT TEXTURE_SLOT = 1;

static constexpr UINT TRUE_DESCRIPTOR_INDEX = 0;
static constexpr UINT FALSE_DESCRIPTOR_INDEX = 1;
static constexpr UINT FIRST_TEXTURE_DESCRIPTOR_INDEX = 2;

void draw2d::Pipeline::PopulateCommandListDrawing(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    const Drawer drawer
    {
        .initializeTextures = [](Texture** textures, const UINT textureCount, Pipeline* ctx)
        {
            REQUIRE(textureCount > 0);
            REQUIRE(ctx->m_initialized == false);
            
            ctx->m_textures.clear();
            ctx->m_textures.reserve(textureCount);

            for (UINT i = 0; i < textureCount; i++)
            {
                Texture& texture = *textures[i];
                ctx->m_textures.push_back(std::make_tuple(texture.GetResource(), texture.GetView()));

                texture.TransitionToUsable(ctx->m_currentCommandList);
            }

            ctx->m_raster->CreateResourceViews(ctx->m_constantBufferViews, ctx->m_textures);

            Initialize(ctx);
        },
        .uploadBuffer = [](const Vertex* vertices, const UINT vertexCount, Pipeline* ctx)
        {
            const UINT vertexBufferSize = vertexCount * sizeof(Vertex);

            ctx->m_uploadBuffer = nv_helpers_dx12::CreateBuffer(
                ctx->m_device.Get(), vertexBufferSize,
                D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
                nv_helpers_dx12::kUploadHeapProps);

            Vertex* pData;
            TRY_DO(ctx->m_uploadBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));
            memcpy(pData, vertices, vertexBufferSize);
            ctx->m_uploadBuffer->Unmap(0, nullptr);

            ctx->m_vertexBuffer = nv_helpers_dx12::CreateBuffer(
                ctx->m_device.Get(), vertexBufferSize,
                D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_COMMON,
                nv_helpers_dx12::kDefaultHeapProps);

            auto transition = CD3DX12_RESOURCE_BARRIER::Transition(ctx->m_vertexBuffer.Get(),
                                                                   D3D12_RESOURCE_STATE_COMMON,
                                                                   D3D12_RESOURCE_STATE_COPY_DEST);
            ctx->m_currentCommandList->ResourceBarrier(1, &transition);

            ctx->m_currentCommandList->CopyBufferRegion(ctx->m_vertexBuffer.Get(), 0,
                                                        ctx->m_uploadBuffer.Get(), 0,
                                                        vertexBufferSize);

            transition = CD3DX12_RESOURCE_BARRIER::Transition(ctx->m_vertexBuffer.Get(),
                                                              D3D12_RESOURCE_STATE_COPY_DEST,
                                                              D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
            ctx->m_currentCommandList->ResourceBarrier(1, &transition);

            D3D12_VERTEX_BUFFER_VIEW vertexBufferView;
            vertexBufferView.BufferLocation = ctx->m_vertexBuffer->GetGPUVirtualAddress();
            vertexBufferView.StrideInBytes = sizeof(Vertex);
            vertexBufferView.SizeInBytes = vertexBufferSize;

            ctx->m_currentCommandList->IASetVertexBuffers(0, 1, &vertexBufferView);
        },
        .drawBuffer = [](
        const UINT firstVertex, const UINT vertexCount, const UINT textureIndex, const BOOL useTexture,
        Pipeline* ctx)
        {
            if (!ctx->m_initialized) Initialize(ctx);
            
            if (ctx->m_currentUseTexture != useTexture)
            {
                ctx->m_currentUseTexture = useTexture;
                ctx->m_raster->BindDescriptor(ctx->m_currentCommandList,
                                              BOOLEAN_SLOT,
                                              useTexture ? TRUE_DESCRIPTOR_INDEX : FALSE_DESCRIPTOR_INDEX);
            }

            if (ctx->m_currentTextureIndex != textureIndex && useTexture)
            {
                ctx->m_currentTextureIndex = textureIndex;
                ctx->m_raster->BindDescriptor(ctx->m_currentCommandList,
                                              TEXTURE_SLOT, FIRST_TEXTURE_DESCRIPTOR_INDEX + textureIndex);
            }

            ctx->m_currentCommandList->DrawInstanced(vertexCount, 1, firstVertex, 0);
        },
        .ctx = this
    };

    commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    m_currentCommandList = commandList.Get();
    m_callback(drawer);
    m_currentCommandList = nullptr;
    m_initialized = false;
}

void draw2d::Pipeline::Initialize(Pipeline* ctx)
{
    // Each draw call requires a initialized descriptor heap.
    // But only one descriptor heap is used for all draw calls.
    // Therefore, the heap is initialized either on texture initialization or on the first draw call of a frame.

    ctx->m_raster->SetupHeaps(ctx->m_currentCommandList);
    ctx->m_raster->SetupRootDescriptorTable(ctx->m_currentCommandList);

    ctx->m_currentTextureIndex = 0;
    ctx->m_currentUseTexture = FALSE;

    ctx->m_raster->BindDescriptor(ctx->m_currentCommandList, BOOLEAN_SLOT, FALSE_DESCRIPTOR_INDEX);
    ctx->m_raster->BindDescriptor(ctx->m_currentCommandList, TEXTURE_SLOT, FIRST_TEXTURE_DESCRIPTOR_INDEX);

    ctx->m_initialized = true;
}
