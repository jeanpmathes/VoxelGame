﻿#include "stdafx.h"

draw2d::Pipeline::Pipeline(NativeClient& client, RasterPipeline* raster, UINT id, Callback const callback)
    : m_raster(raster)
  , m_callback(callback)
  , m_client(&client)
  , m_name(std::format(L"{} [{}]", m_raster->GetName(), id))
{
    Require(m_raster != nullptr);

    auto addBuffer = [this](BOOL const value)
    {
        constexpr UINT size = sizeof(BOOL);

        UINT64 alignedSize = size;

        Allocation<ID3D12Resource> const booleanConstantBuffer = util::AllocateConstantBuffer(*m_client, &alignedSize);
        NAME_D3D12_OBJECT(booleanConstantBuffer);

        this->m_cbuffers.push_back(booleanConstantBuffer);
        this->m_constantBufferViews.push_back(
            {booleanConstantBuffer.GetGPUVirtualAddress(), static_cast<UINT>(alignedSize)});

        TryDo(util::MapAndWrite(booleanConstantBuffer, value));
    };

    addBuffer(TRUE);
    addBuffer(FALSE);

    this->m_raster->SetSelectionListContent(
        this->m_raster->GetBindings().Draw2D().booleans,
        this->m_constantBufferViews);
}

static constexpr UINT TRUE_DESCRIPTOR_INDEX  = 0;
static constexpr UINT FALSE_DESCRIPTOR_INDEX = 1;

void draw2d::Pipeline::PopulateCommandList(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    Drawer const drawer{
        .initializeTextures = [](Texture** textures, UINT const textureCount, Pipeline* ctx)
        {
            Require(textureCount > 0);
            Require(!ctx->m_initialized);

            ctx->m_textures.clear();
            ctx->m_textures.reserve(textureCount);

            for (UINT i = 0; i < textureCount; i++)
            {
                Texture& texture = *textures[i];
                ctx->m_textures.emplace_back(texture.GetResource(), &texture.GetView());

                texture.TransitionToUsable(ctx->m_currentCommandList);
            }

            ctx->m_raster->SetSelectionListContent(ctx->m_raster->GetBindings().Draw2D().textures, ctx->m_textures);

            Initialize(ctx);
        },
        .uploadBuffer = [](Vertex const* vertices, UINT const vertexCount, Pipeline* ctx)
        {
            Require(vertices != nullptr);
            Require(vertexCount > 0);

            Require(!ctx->m_vertexBufferBound);
            ctx->m_vertexCount = vertexCount;

            UINT const vertexBufferSize = vertexCount * sizeof(Vertex);

            util::ReAllocateBuffer(
                &ctx->m_uploadBuffer,
                *ctx->m_client,
                vertexBufferSize,
                D3D12_RESOURCE_FLAG_NONE,
                D3D12_RESOURCE_STATE_GENERIC_READ,
                D3D12_HEAP_TYPE_UPLOAD);
            NAME_D3D12_OBJECT(ctx->m_uploadBuffer);

            TryDo(util::MapAndWrite(ctx->m_uploadBuffer, vertices, vertexCount));

            util::ReAllocateBuffer(
                &ctx->m_vertexBuffer,
                *ctx->m_client,
                vertexBufferSize,
                D3D12_RESOURCE_FLAG_NONE,
                D3D12_RESOURCE_STATE_COMMON,
                D3D12_HEAP_TYPE_DEFAULT);
            NAME_D3D12_OBJECT(ctx->m_vertexBuffer);

            auto transition = CD3DX12_RESOURCE_BARRIER::Transition(
                ctx->m_vertexBuffer.Get(),
                D3D12_RESOURCE_STATE_COMMON,
                D3D12_RESOURCE_STATE_COPY_DEST);
            ctx->m_currentCommandList->ResourceBarrier(1, &transition);

            ctx->m_currentCommandList->CopyBufferRegion(
                ctx->m_vertexBuffer.Get(),
                0,
                ctx->m_uploadBuffer.Get(),
                0,
                vertexBufferSize);

            transition = CD3DX12_RESOURCE_BARRIER::Transition(
                ctx->m_vertexBuffer.Get(),
                D3D12_RESOURCE_STATE_COPY_DEST,
                D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
            ctx->m_currentCommandList->ResourceBarrier(1, &transition);

            ctx->m_vertexBufferView                = {};
            ctx->m_vertexBufferView.BufferLocation = ctx->m_vertexBuffer.GetGPUVirtualAddress();
            ctx->m_vertexBufferView.StrideInBytes  = sizeof(Vertex);
            ctx->m_vertexBufferView.SizeInBytes    = vertexBufferSize;

            ctx->BindVertexBuffer();
        },
        .drawBuffer = [](
        UINT const firstVertex,
        UINT const vertexCount,
        UINT const textureIndex,
        BOOL const useTexture,
        Pipeline*  ctx)
        {
            Require(vertexCount > 0);

            Require(ctx->m_vertexCount >= firstVertex + vertexCount);

            if (!ctx->m_initialized) Initialize(ctx);
            if (!ctx->m_vertexBufferBound) ctx->BindVertexBuffer();

            if (ctx->m_currentUseTexture != useTexture)
            {
                ctx->m_currentUseTexture = useTexture;
                ctx->BindBoolean();
            }

            if (ctx->m_currentTextureIndex != textureIndex && static_cast<bool>(useTexture))
            {
                ctx->m_currentTextureIndex = textureIndex;
                ctx->BindTextures();
            }

            ctx->m_currentCommandList->DrawInstanced(vertexCount, 1, firstVertex, 0);
        },
        .ctx = this
    };

    m_currentCommandList = commandList.Get();
    m_callback(drawer);
    m_currentCommandList = nullptr;

    m_initialized       = false;
    m_vertexBufferBound = false;
}

LPCWSTR draw2d::Pipeline::GetName() const { return m_name.c_str(); }

void draw2d::Pipeline::Initialize(Pipeline* ctx)
{
    // Each draw call requires a initialized descriptor heap.
    // But only one descriptor heap is used for all draw calls.
    // Therefore, the heap is initialized either on texture initialization or on the first draw call of a frame.

    ctx->m_raster->SetPipeline(ctx->m_currentCommandList);
    ctx->m_raster->BindResources(ctx->m_currentCommandList);

    ctx->m_currentTextureIndex = 0;
    ctx->BindTextures();

    ctx->m_currentUseTexture = FALSE;
    ctx->BindBoolean();

    ctx->m_initialized = true;
}

void draw2d::Pipeline::BindBoolean() const
{
    this->m_raster->BindSelectionIndex(
        this->m_currentCommandList,
        this->m_raster->GetBindings().Draw2D().booleans,
        this->m_currentUseTexture ? TRUE_DESCRIPTOR_INDEX : FALSE_DESCRIPTOR_INDEX);
}

void draw2d::Pipeline::BindTextures() const
{
    this->m_raster->BindSelectionIndex(
        this->m_currentCommandList,
        this->m_raster->GetBindings().Draw2D().textures,
        this->m_currentTextureIndex);
}

void draw2d::Pipeline::BindVertexBuffer()
{
    this->m_currentCommandList->IASetVertexBuffers(0, 1, &this->m_vertexBufferView);
    this->m_vertexBufferBound = true;
}
