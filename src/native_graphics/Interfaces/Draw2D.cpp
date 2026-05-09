#include "stdafx.h"

draw2d::Pipeline::Pipeline(NativeClient& hostClient, RasterPipeline* raster, UINT id, Callback const callback)
    : raster(raster)
  , callback(callback)
  , client(&hostClient)
  , name(std::format(L"{} [{}]", raster->GetName(), id))
{
    Require(raster != nullptr);

    auto addBuffer = [this](BOOL const value)
    {
        constexpr UINT size = sizeof(BOOL);

        UINT64 alignedSize = size;

        Allocation<ID3D12Resource> const booleanConstantBuffer = util::AllocateConstantBuffer(*client, &alignedSize);
        NAME_D3D12_OBJECT(booleanConstantBuffer);

        this->cbuffers.push_back(booleanConstantBuffer);
        this->constantBufferViews.push_back({booleanConstantBuffer.GetGPUVirtualAddress(), static_cast<UINT>(alignedSize)});

        TryDo(util::MapAndWrite(booleanConstantBuffer, value));
    };

    addBuffer(TRUE);
    addBuffer(FALSE);

    this->raster->SetSelectionListContent(this->raster->GetBindings().Draw2D().booleans, this->constantBufferViews);
}

static constexpr UINT TRUE_DESCRIPTOR_INDEX  = 0;
static constexpr UINT FALSE_DESCRIPTOR_INDEX = 1;

void draw2d::Pipeline::PopulateCommandList(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    Drawer const drawer{
        .initializeTextures = [](Texture** textures, UINT const textureCount, Pipeline* ctx)
        {
            Require(textureCount > 0);
            Require(!ctx->initialized);

            ctx->textures.clear();
            ctx->textures.reserve(textureCount);

            for (UINT i = 0; i < textureCount; i++)
            {
                Texture& texture = *textures[i];
                ctx->textures.emplace_back(texture.GetResource(), &texture.GetView());

                texture.TransitionToUsable(ctx->currentCommandList);
            }

            ctx->raster->SetSelectionListContent(ctx->raster->GetBindings().Draw2D().textures, ctx->textures);

            Initialize(ctx);
        },
        .uploadBuffer = [](Vertex const* vertices, UINT const vertexCount, Pipeline* ctx)
        {
            Require(vertices != nullptr);
            Require(vertexCount > 0);

            Require(!ctx->vertexBufferBound);
            ctx->vertexCount = vertexCount;

            UINT const vertexBufferSize = vertexCount * sizeof(Vertex);

            util::ReAllocateBuffer(&ctx->uploadBuffer, *ctx->client, vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
            NAME_D3D12_OBJECT(ctx->uploadBuffer);

            TryDo(util::MapAndWrite(ctx->uploadBuffer, vertices, vertexCount));

            util::ReAllocateBuffer(&ctx->vertexBuffer, *ctx->client, vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_COMMON, D3D12_HEAP_TYPE_DEFAULT);
            NAME_D3D12_OBJECT(ctx->vertexBuffer);

            auto transition = CD3DX12_RESOURCE_BARRIER::Transition(ctx->vertexBuffer.Get(), D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATE_COPY_DEST);
            ctx->currentCommandList->ResourceBarrier(1, &transition);

            ctx->currentCommandList->CopyBufferRegion(ctx->vertexBuffer.Get(), 0, ctx->uploadBuffer.Get(), 0, vertexBufferSize);

            transition = CD3DX12_RESOURCE_BARRIER::Transition(ctx->vertexBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
            ctx->currentCommandList->ResourceBarrier(1, &transition);

            ctx->vertexBufferView                = {};
            ctx->vertexBufferView.BufferLocation = ctx->vertexBuffer.GetGPUVirtualAddress();
            ctx->vertexBufferView.StrideInBytes  = sizeof(Vertex);
            ctx->vertexBufferView.SizeInBytes    = vertexBufferSize;

            ctx->BindVertexBuffer();
        },
        .drawBuffer = [](UINT const firstVertex, UINT const vertexCount, UINT const textureIndex, BOOL const useTexture, Pipeline* ctx)
        {
            Require(vertexCount > 0);

            Require(ctx->vertexCount >= firstVertex + vertexCount);

            if (!ctx->initialized) Initialize(ctx);
            if (!ctx->vertexBufferBound) ctx->BindVertexBuffer();

            if (ctx->currentUseTexture != useTexture)
            {
                ctx->currentUseTexture = useTexture;
                ctx->BindBoolean();
            }

            if (ctx->currentTextureIndex != textureIndex && static_cast<bool>(useTexture))
            {
                ctx->currentTextureIndex = textureIndex;
                ctx->BindTextures();
            }

            ctx->currentCommandList->DrawInstanced(vertexCount, 1, firstVertex, 0);
        },
        .ctx = this
    };

    currentCommandList = commandList.Get();
    callback(drawer);
    currentCommandList = nullptr;

    initialized       = false;
    vertexBufferBound = false;
}

LPCWSTR draw2d::Pipeline::GetName() const { return name.c_str(); }

void draw2d::Pipeline::Initialize(Pipeline* ctx)
{
    // Each draw call requires a initialized descriptor heap.
    // But only one descriptor heap is used for all draw calls.
    // Therefore, the heap is initialized either on texture initialization or on the first draw call of a frame.

    ctx->raster->SetPipeline(ctx->currentCommandList);
    ctx->raster->BindResources(ctx->currentCommandList);

    ctx->currentTextureIndex = 0;
    ctx->BindTextures();

    ctx->currentUseTexture = FALSE;
    ctx->BindBoolean();

    ctx->initialized = true;
}

void draw2d::Pipeline::BindBoolean() const
{
    this->raster->BindSelectionIndex(
        this->currentCommandList,
        this->raster->GetBindings().Draw2D().booleans,
        this->currentUseTexture ? TRUE_DESCRIPTOR_INDEX : FALSE_DESCRIPTOR_INDEX);
}

void draw2d::Pipeline::BindTextures() const
{
    this->raster->BindSelectionIndex(this->currentCommandList, this->raster->GetBindings().Draw2D().textures, this->currentTextureIndex);
}

void draw2d::Pipeline::BindVertexBuffer()
{
    this->currentCommandList->IASetVertexBuffers(0, 1, &this->vertexBufferView);
    this->vertexBufferBound = true;
}
