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

#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment)                                         \
  (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

namespace nv_helpers_dx12
{
    void BottomLevelASGenerator::AddVertexBuffer(
        Allocation<ID3D12Resource> const& vertexBuffer, UINT64 const vertexOffsetInBytes, uint32_t const vertexCount,
        UINT const                        vertexSizeInBytes, Allocation<ID3D12Resource> const& transformBuffer,
        UINT64 const                      transformOffsetInBytes, bool const isOpaque)
    {
        AddVertexBuffer(
            vertexBuffer,
            vertexOffsetInBytes,
            vertexCount,
            vertexSizeInBytes,
            {},
            0,
            0,
            transformBuffer,
            transformOffsetInBytes,
            isOpaque);
    }

    void BottomLevelASGenerator::AddVertexBuffer(
        Allocation<ID3D12Resource> const& vertexBuffer, UINT64 const vertexOffsetInBytes, uint32_t const vertexCount,
        UINT const vertexSizeInBytes, Allocation<ID3D12Resource> const& indexBuffer, UINT64 const indexOffsetInBytes,
        uint32_t const indexCount, Allocation<ID3D12Resource> const& transformBuffer,
        UINT64 const transformOffsetInBytes, bool const isOpaque)
    {
        // Create the DX12 descriptor representing the input data, assumed to be
        // triangles, with 3xf32 vertex coordinates and 32-bit indices.

        D3D12_RAYTRACING_GEOMETRY_DESC descriptor;
        descriptor.Type                                = D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES;
        descriptor.Triangles.VertexBuffer.StartAddress = vertexBuffer.GetGPUVirtualAddress<ID3D12Resource>() +
            vertexOffsetInBytes;
        descriptor.Triangles.VertexBuffer.StrideInBytes = vertexSizeInBytes;
        descriptor.Triangles.VertexCount                = vertexCount;
        descriptor.Triangles.VertexFormat               = DXGI_FORMAT_R32G32B32_FLOAT;
        descriptor.Triangles.IndexBuffer                = indexBuffer.IsSet()
                                                              ? (indexBuffer.GetGPUVirtualAddress<ID3D12Resource>() +
                                                                  indexOffsetInBytes)
                                                              : 0;
        descriptor.Triangles.IndexFormat  = indexBuffer.IsSet() ? DXGI_FORMAT_R32_UINT : DXGI_FORMAT_UNKNOWN;
        descriptor.Triangles.IndexCount   = indexCount;
        descriptor.Triangles.Transform3x4 = transformBuffer.IsSet()
                                                ? (transformBuffer.GetGPUVirtualAddress<ID3D12Resource>() +
                                                    transformOffsetInBytes)
                                                : 0;
        descriptor.Flags = isOpaque ? D3D12_RAYTRACING_GEOMETRY_FLAG_OPAQUE : D3D12_RAYTRACING_GEOMETRY_FLAG_NONE;

        m_geometryBuffers.push_back(descriptor);

        m_usedResources.push_back(vertexBuffer);
        if (indexBuffer.IsSet()) m_usedResources.push_back(indexBuffer);
        if (transformBuffer.IsSet()) m_usedResources.push_back(transformBuffer);
    }

    void BottomLevelASGenerator::AddBoundsBuffer(
        Allocation<ID3D12Resource> const& boundsBuffer, UINT64 const boundsOffsetInBytes, uint32_t const boundsCount,
        UINT const                        boundsSizeInBytes)
    {
        // Create the DX12 descriptor representing the input data, assumed to be
        // AABBs, with 2x3xf32 coordinates.

        D3D12_RAYTRACING_GEOMETRY_DESC descriptor = {};
        descriptor.Type = D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS;
        descriptor.AABBs.AABBs.StartAddress = boundsBuffer.GetGPUVirtualAddress<ID3D12Resource>() + boundsOffsetInBytes;
        descriptor.AABBs.AABBs.StrideInBytes = boundsSizeInBytes;
        descriptor.AABBs.AABBCount = boundsCount;

        m_geometryBuffers.push_back(descriptor);

        m_usedResources.push_back(boundsBuffer);
    }

    void BottomLevelASGenerator::ComputeASBufferSizes(
        ID3D12Device5* device, bool const allowUpdate, UINT64* scratchSizeInBytes, UINT64* resultSizeInBytes)
    {
        // The generated AS can support iterative updates. This may change the final
        // size of the AS as well as the temporary memory requirements, and hence has
        // to be set before the actual build.
        m_flags = allowUpdate
                      ? D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE
                      : D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;

        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_INPUTS prebuildDesc;
        prebuildDesc.Type           = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL;
        prebuildDesc.DescsLayout    = D3D12_ELEMENTS_LAYOUT_ARRAY;
        prebuildDesc.NumDescs       = static_cast<UINT>(m_geometryBuffers.size());
        prebuildDesc.pGeometryDescs = m_geometryBuffers.data();
        prebuildDesc.Flags          = m_flags;

        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_PREBUILD_INFO info = {};
        device->GetRaytracingAccelerationStructurePrebuildInfo(&prebuildDesc, &info);

        *scratchSizeInBytes  = ROUND_UP(info.ScratchDataSizeInBytes, D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        *resultSizeInBytes   = ROUND_UP(info.ResultDataMaxSizeInBytes, D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        m_scratchSizeInBytes = *scratchSizeInBytes;
        m_resultSizeInBytes  = *resultSizeInBytes;
    }

    void BottomLevelASGenerator::Generate(
        ID3D12GraphicsCommandList4*     commandList, D3D12_GPU_VIRTUAL_ADDRESS const scratchBuffer,
        D3D12_GPU_VIRTUAL_ADDRESS const resultBuffer, bool const                     updateOnly,
        D3D12_GPU_VIRTUAL_ADDRESS const previousResult) const
    {
        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS flags = m_flags;
        bool const isUpdateAllowed = flags & D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE;

        // The stored flags represent whether the AS has been built for updates or
        // not. If yes and an update is requested, the builder is told to only update
        // the AS instead of fully rebuilding it.
        if (updateOnly && isUpdateAllowed) flags |= D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_PERFORM_UPDATE;

        if (updateOnly && !isUpdateAllowed)
            throw std::logic_error("Cannot update a bottom-level AS not built for updates.");
        if (updateOnly && previousResult == 0)
            throw std::logic_error("Bottom-level hierarchy update requires the previous hierarchy.");

        if (m_resultSizeInBytes == 0 || m_scratchSizeInBytes == 0)
            throw std::logic_error(
                "Invalid scratch and result buffer sizes - ComputeASBufferSizes needs to be called before Build.");

        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_DESC buildDesc;
        buildDesc.Inputs.Type                      = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL;
        buildDesc.Inputs.DescsLayout               = D3D12_ELEMENTS_LAYOUT_ARRAY;
        buildDesc.Inputs.NumDescs                  = static_cast<UINT>(m_geometryBuffers.size());
        buildDesc.Inputs.pGeometryDescs            = m_geometryBuffers.data();
        buildDesc.DestAccelerationStructureData    = resultBuffer;
        buildDesc.ScratchAccelerationStructureData = scratchBuffer;
        buildDesc.SourceAccelerationStructureData  = previousResult;
        buildDesc.Inputs.Flags                     = flags;

        commandList->BuildRaytracingAccelerationStructure(&buildDesc, 0, nullptr);
    }
}
