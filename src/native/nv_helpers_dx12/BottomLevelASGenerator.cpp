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
*/

#include "BottomLevelASGenerator.hpp"

#include <stdexcept>

// Helper to compute aligned buffer sizes
#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment)                                         \
  (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

namespace nv_helpers_dx12
{
    void BottomLevelASGenerator::AddVertexBuffer(
        ID3D12Resource* vertexBuffer,
        UINT64
        vertexOffsetInBytes,
        uint32_t vertexCount,
        UINT vertexSizeInBytes,
        ID3D12Resource* transformBuffer,
        UINT64 transformOffsetInBytes,
        bool isOpaque
    )
    {
        AddVertexBuffer(vertexBuffer, vertexOffsetInBytes, vertexCount,
                        vertexSizeInBytes, nullptr, 0, 0, transformBuffer,
                        transformOffsetInBytes, isOpaque);
    }

    void BottomLevelASGenerator::AddVertexBuffer(
        ID3D12Resource* vertexBuffer,
        UINT64
        vertexOffsetInBytes,
        uint32_t vertexCount,
        UINT vertexSizeInBytes,
        ID3D12Resource* indexBuffer,
        UINT64 indexOffsetInBytes,
        uint32_t indexCount,
        ID3D12Resource* transformBuffer,
        UINT64 transformOffsetInBytes,
        bool isOpaque
    )
    {
        // Create the DX12 descriptor representing the input data, assumed to be
        // triangles, with 3xf32 vertex coordinates and 32-bit indices
        D3D12_RAYTRACING_GEOMETRY_DESC descriptor;
        descriptor.Type = D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES;
        descriptor.Triangles.VertexBuffer.StartAddress =
            vertexBuffer->GetGPUVirtualAddress() + vertexOffsetInBytes;
        descriptor.Triangles.VertexBuffer.StrideInBytes = vertexSizeInBytes;
        descriptor.Triangles.VertexCount = vertexCount;
        descriptor.Triangles.VertexFormat = DXGI_FORMAT_R32G32B32_FLOAT;
        descriptor.Triangles.IndexBuffer =
            indexBuffer
                ? (indexBuffer->GetGPUVirtualAddress() + indexOffsetInBytes)
                : 0;
        descriptor.Triangles.IndexFormat =
            indexBuffer ? DXGI_FORMAT_R32_UINT : DXGI_FORMAT_UNKNOWN;
        descriptor.Triangles.IndexCount = indexCount;
        descriptor.Triangles.Transform3x4 =
            transformBuffer
                ? (transformBuffer->GetGPUVirtualAddress() + transformOffsetInBytes)
                : 0;
        descriptor.Flags = isOpaque
                               ? D3D12_RAYTRACING_GEOMETRY_FLAG_OPAQUE
                               : D3D12_RAYTRACING_GEOMETRY_FLAG_NONE;

        m_vertexBuffers.push_back(descriptor);
    }

    void BottomLevelASGenerator::AddBoundsBuffer(
        ID3D12Resource* boundsBuffer,
        const UINT64 boundsOffsetInBytes,
        const uint32_t boundsCount,
        const UINT boundsSizeInBytes)
    {
        // Create the DX12 descriptor representing the input data, assumed to be
        // AABBs, with 2x3xf32 coordinates
        D3D12_RAYTRACING_GEOMETRY_DESC descriptor = {};
        descriptor.Type = D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS;
        descriptor.AABBs.AABBs.StartAddress =
            boundsBuffer->GetGPUVirtualAddress() + boundsOffsetInBytes;
        descriptor.AABBs.AABBs.StrideInBytes = boundsSizeInBytes;
        descriptor.AABBs.AABBCount = boundsCount;

        m_vertexBuffers.push_back(descriptor);
    }

    void BottomLevelASGenerator::ComputeASBufferSizes(
        ID3D12Device5* device,
        bool allowUpdate,
        UINT64* scratchSizeInBytes,
        UINT64* resultSizeInBytes
    )
    {
        // The generated AS can support iterative updates. This may change the final
        // size of the AS as well as the temporary memory requirements, and hence has
        // to be set before the actual build
        m_flags =
            allowUpdate
                ? D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE
                : D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;

        // Describe the work being requested, in this case the construction of a
        // (possibly dynamic) bottom-level hierarchy, with the given vertex buffers

        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_INPUTS prebuildDesc;
        prebuildDesc.Type = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL;
        prebuildDesc.DescsLayout = D3D12_ELEMENTS_LAYOUT_ARRAY;
        prebuildDesc.NumDescs = static_cast<UINT>(m_vertexBuffers.size());
        prebuildDesc.pGeometryDescs = m_vertexBuffers.data();
        prebuildDesc.Flags = m_flags;

        // This structure is used to hold the sizes of the required scratch memory and
        // resulting AS
        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_PREBUILD_INFO info = {};

        // Building the acceleration structure (AS) requires some scratch space, as
        // well as space to store the resulting structure This function computes a
        // conservative estimate of the memory requirements for both, based on the
        // geometry size.
        device->GetRaytracingAccelerationStructurePrebuildInfo(&prebuildDesc, &info);

        // Buffer sizes need to be 256-byte-aligned
        *scratchSizeInBytes =
            ROUND_UP(info.ScratchDataSizeInBytes,
                     D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        *resultSizeInBytes = ROUND_UP(info.ResultDataMaxSizeInBytes,
                                      D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

        // Store the memory requirements for use during build
        m_scratchSizeInBytes = *scratchSizeInBytes;
        m_resultSizeInBytes = *resultSizeInBytes;
    }

    void BottomLevelASGenerator::Generate(
        ID3D12GraphicsCommandList4* commandList,
        D3D12_GPU_VIRTUAL_ADDRESS scratchBuffer,
        D3D12_GPU_VIRTUAL_ADDRESS resultBuffer,
        const bool updateOnly,
        D3D12_GPU_VIRTUAL_ADDRESS previousResult
    ) const
    {
        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS flags = m_flags;
        // The stored flags represent whether the AS has been built for updates or
        // not. If yes and an update is requested, the builder is told to only update
        // the AS instead of fully rebuilding it
        if (flags ==
            D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE &&
            updateOnly)
        {
            flags = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_PERFORM_UPDATE;
        }

        // Sanity checks
        if (m_flags !=
            D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE &&
            updateOnly)
        {
            throw std::logic_error(
                "Cannot update a bottom-level AS not originally built for updates");
        }
        if (updateOnly && previousResult == 0)
        {
            throw std::logic_error(
                "Bottom-level hierarchy update requires the previous hierarchy");
        }

        if (m_resultSizeInBytes == 0 || m_scratchSizeInBytes == 0)
        {
            throw std::logic_error(
                "Invalid scratch and result buffer sizes - ComputeASBufferSizes needs to be called before Build");
        }
        // Create a descriptor of the requested builder work, to generate a
        // bottom-level AS from the input parameters
        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_DESC buildDesc;
        buildDesc.Inputs.Type = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL;
        buildDesc.Inputs.DescsLayout = D3D12_ELEMENTS_LAYOUT_ARRAY;
        buildDesc.Inputs.NumDescs = static_cast<UINT>(m_vertexBuffers.size());
        buildDesc.Inputs.pGeometryDescs = m_vertexBuffers.data();
        buildDesc.DestAccelerationStructureData = resultBuffer;
        buildDesc.ScratchAccelerationStructureData = scratchBuffer;
        buildDesc.SourceAccelerationStructureData = previousResult;
        buildDesc.Inputs.Flags = flags;

        // Build the AS
        commandList->BuildRaytracingAccelerationStructure(&buildDesc, 0, nullptr);
    }
} // namespace nv_helpers_dx12
