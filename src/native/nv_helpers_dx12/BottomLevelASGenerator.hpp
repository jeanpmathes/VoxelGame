/*-----------------------------------------------------------------------
Copyright (c) 2014-2018, NVIDIA. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
* Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
* Neither the name of its contributors may be used to endorse
or promote products derived from this software without specific
prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-----------------------------------------------------------------------*/

/*
Contacts for feedback:
- pgautron@nvidia.com (Pascal Gautron)
- mlefrancois@nvidia.com (Martin-Karl Lefrancois)

The bottom-level hierarchy is used to store the triangle data in a way suitable
for fast ray-triangle intersection at runtime. To be built, this data structure
requires some scratch space which has to be allocated by the application.
Similarly, the resulting data structure is stored in an application-controlled
buffer.

To be used, the application must first add all the vertex buffers to be
contained in the final structure, using AddVertexBuffer. After all buffers have
been added, ComputeASBufferSizes will prepare the build, and provide the
required sizes for the scratch data and the final result. The Build call will
finally compute the acceleration structure and store it in the result buffer.

Note that the build is enqueued in the command list, meaning that the scratch
buffer needs to be kept until the command list execution is finished.


Example:

BottomLevelASGenerator bottomLevelAS;
bottomLevelAS.AddVertexBuffer(vertexBuffer, 0, vertexCount, sizeof(Vertex),
identityMat.Get(), 0); bottomLevelAS.AddVertexBuffer(vertexBuffer2, 0,
vertexCount2, sizeof(Vertex), identityMat.Get(), 0);
...
UINT64 scratchSizeInBytes = 0;
UINT64 resultSizeInBytes = 0;
bottomLevelAS.ComputeASBufferSizes(GetRTDevice(), false, &scratchSizeInBytes,
&resultSizeInBytes); AccelerationStructureBuffers buffers; buffers.pScratch =
nv_helpers_dx12::CreateBuffer(..., scratchSizeInBytes, ...); buffers.pResult =
nv_helpers_dx12::CreateBuffer(..., resultSizeInBytes, ...);

bottomLevelAS.Generate(m_commandList.Get(), rtCmdList, buffers.pScratch.Get(),
buffers.pResult.Get(), false, nullptr);

return buffers;

*/

#pragma once

#include "d3d12.h"

#include <vector>

namespace nv_helpers_dx12
{
    /// Helper class to generate bottom-level acceleration structures for raytracing
    class BottomLevelASGenerator
    {
    public:
        /// Add a vertex buffer in GPU memory into the acceleration structure. The
        /// vertices are supposed to be represented by 3 float32 value. Indices are
        /// implicit.
        /// \param vertexBuffer Buffer containing the vertex coordinates, possibly interleaved with other vertex data
        /// \param vertexOffsetInBytes Offset of the first vertex in the vertex buffer
        /// \param vertexCount Number of vertices to consider in the buffer
        /// \param vertexSizeInBytes Size of a vertex including all its other data, used to stride in the buffer
        /// \param transformBuffer Buffer containing a 4x4 transform matrix in GPU memory, to be applied to the vertices.
        /// \param transformOffsetInBytes Offset of the transform matrix in the transform buffer
        /// \param isOpaque If true, the geometry is considered opaque, optimizing the search for a closest hit
        void AddVertexBuffer(ID3D12Resource* vertexBuffer,
                             UINT64 vertexOffsetInBytes,
                             uint32_t vertexCount,
                             UINT vertexSizeInBytes,
                             ID3D12Resource* transformBuffer,
                             UINT64 transformOffsetInBytes,
                             bool isOpaque = true
        );

        /// Add a vertex buffer along with its index buffer in GPU memory into the acceleration structure.
        /// The vertices are supposed to be represented by 3 float32 value, and the indices are 32-bit
        /// unsigned ints
        /// \param vertexBuffer Buffer containing the vertex coordinates, possibly interleaved with other vertex data
        /// \param vertexOffsetInBytes Offset of the first vertex in the vertex buffer
        /// \param vertexCount Number of vertices to consider in the buffer
        /// \param vertexSizeInBytes Size of a vertex including all its other data, used to stride in the buffer
        /// \param indexBuffer Buffer containing the vertex indices describing the triangles
        /// \param indexOffsetInBytes Offset of the first index in the index buffer
        /// \param indexCount Number of indices to consider in the buffer
        /// \param transformBuffer Buffer containing a 4x4 transform matrix in GPU memory, to be applied to the vertices.
        /// \param transformOffsetInBytes Offset of the transform matrix in the transform buffer
        /// \param isOpaque If true, the geometry is considered opaque, optimizing the search for a closest hit
        void AddVertexBuffer(ID3D12Resource* vertexBuffer,
                             UINT64 vertexOffsetInBytes,
                             uint32_t vertexCount,
                             UINT vertexSizeInBytes,
                             ID3D12Resource* indexBuffer,
                             UINT64 indexOffsetInBytes,
                             uint32_t indexCount,
                             ID3D12Resource* transformBuffer,
                             UINT64 transformOffsetInBytes,
                             bool isOpaque = true
        );

        /// Add a buffer containing axis-aligned bounding boxes in GPU memory into the acceleration structure.
        /// \param boundsBuffer Buffer containing the bounding boxes, as an array of float3 min and float3 max
        /// \param boundsOffsetInBytes Offset of the first bounding box in the buffer
        /// \param boundsCount Number of bounding boxes to consider in the buffer
        /// \param boundsSizeInBytes Size of a bounding box, used to stride in the buffer
        void AddBoundsBuffer(ID3D12Resource* boundsBuffer,
                             UINT64 boundsOffsetInBytes,
                             uint32_t boundsCount,
                             UINT boundsSizeInBytes
        );

        /// Compute the size of the scratch space required to build the acceleration structure, as well as
        /// the size of the resulting structure. The allocation of the buffers is then left to the
        /// application
        /// \param device Device on which the build will be performed
        /// \param allowUpdate If true, the resulting acceleration structure will allow iterative updates
        /// \param scratchSizeInBytes Required scratch memory on the GPU to build the acceleration structure
        /// \param resultSizeInBytes Required GPU memory to store the acceleration structure
        void ComputeASBufferSizes(
            ID3D12Device5* device,
            bool allowUpdate,
            UINT64* scratchSizeInBytes,
            UINT64* resultSizeInBytes
        );

        /// Enqueue the construction of the acceleration structure on a command list, using
        /// application-provided buffers and possibly a pointer to the previous acceleration structure in
        /// case of iterative updates. Note that the update can be done in place: the result and
        /// previousResult pointers can be the same.
        /// \param commandList Command list on which the build will be enqueued
        /// \param scratchBuffer Scratch buffer used by the builder to store temporary data
        /// \param resultBuffer Result buffer storing the acceleration structure
        /// \param updateOnly If true, simply refit the existing acceleration structure
        /// \param previousResult Optional previous acceleration structure, used if an iterative update is requested
        void Generate(
            ID3D12GraphicsCommandList4* commandList,
            ID3D12Resource* scratchBuffer,
            ID3D12Resource* resultBuffer,
            bool updateOnly = false,
            ID3D12Resource* previousResult = nullptr
        );

    private:
        /// Vertex buffer descriptors used to generate the AS
        std::vector<D3D12_RAYTRACING_GEOMETRY_DESC> m_vertexBuffers = {};

        /// Amount of temporary memory required by the builder
        UINT64 m_scratchSizeInBytes = 0;

        /// Amount of memory required to store the AS
        UINT64 m_resultSizeInBytes = 0;

        /// Flags for the builder, specifying whether to allow iterative updates, or
        /// when to perform an update
        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS m_flags =
            D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;
    };
} // namespace nv_helpers_dx12
