#include "stdafx.h"

SharedIndexBuffer::SharedIndexBuffer(Space& space)
    : space(space)
{
}

std::pair<Allocation<ID3D12Resource>, UINT> SharedIndexBuffer::GetIndexBuffer(UINT const vertexCount, std::vector<D3D12_RESOURCE_BARRIER>* barriers)
{
    Require(vertexCount > 0);
    Require(vertexCount % 4 == 0);

    UINT const requiredQuadCount  = vertexCount / 4;
    UINT const requiredIndexCount = requiredQuadCount * 6;

    if (requiredIndexCount > sharedIndexCount)
    {
        UINT const requiredIndexBufferSize = requiredIndexCount * sizeof(UINT);

        Allocation<ID3D12Resource> sharedIndexUpload = util::AllocateBuffer(
            space.GetNativeClient(),
            requiredIndexBufferSize,
            D3D12_RESOURCE_FLAG_NONE,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            D3D12_HEAP_TYPE_UPLOAD);
        NAME_D3D12_OBJECT(sharedIndexUpload);

        UINT const availableQuadCount = sharedIndexCount / 6;
        for (UINT quad = availableQuadCount; quad < requiredQuadCount; quad++)
        {
            // The shaders operate on quad basis, so the index winding order does not matter there.
            // The quads itself are defined in CW order.

            // DirectX also uses CW order for triangles, but in a left-handed coordinate system.
            // Because VoxelGame uses a right-handed coordinate system, the BLAS creation requires special handling.

            indices.push_back(quad * 4 + 0);
            indices.push_back(quad * 4 + 1);
            indices.push_back(quad * 4 + 2);

            indices.push_back(quad * 4 + 0);
            indices.push_back(quad * 4 + 2);
            indices.push_back(quad * 4 + 3);
        }

        TryDo(util::MapAndWrite(sharedIndexUpload, indices.data(), requiredIndexCount));

        sharedIndexBuffer = util::AllocateBuffer(
            space.GetNativeClient(),
            requiredIndexBufferSize,
            D3D12_RESOURCE_FLAG_NONE,
            D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_HEAP_TYPE_DEFAULT);
        NAME_D3D12_OBJECT(sharedIndexBuffer);

        space.GetCommandList()->CopyBufferRegion(sharedIndexBuffer.Get(), 0, sharedIndexUpload.resource.Get(), 0, requiredIndexBufferSize);

        D3D12_RESOURCE_BARRIER const transitionCopyDestToShaderResource = {
            CD3DX12_RESOURCE_BARRIER::Transition(sharedIndexBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
        };
        barriers->push_back(transitionCopyDestToShaderResource);

        sharedIndexCount = requiredIndexCount;
        indexBufferUploads.emplace_back(sharedIndexBuffer, sharedIndexUpload);
    }

    return {sharedIndexBuffer, requiredIndexCount};
}

void SharedIndexBuffer::CleanupRender() { indexBufferUploads.clear(); }
