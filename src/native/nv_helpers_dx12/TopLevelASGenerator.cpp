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

The top-level hierarchy is used to store a set of instances represented by
bottom-level hierarchies in a way suitable for fast intersection at runtime. To
be built, this data structure requires some scratch space which has to be
allocated by the application. Similarly, the resulting data structure is stored
in an application-controlled buffer.

To be used, the application must first add all the instances to be contained in
the final structure, using AddInstance. After all instances have been added,
ComputeASBufferSizes will prepare the build, and provide the required sizes for
the scratch data and the final result. The Build call will finally compute the
acceleration structure and store it in the result buffer.

Note that the build is enqueued in the command list, meaning that the scratch
buffer needs to be kept until the command list execution is finished.

*/

#include "TopLevelASGenerator.hpp"

#include <stdexcept>

#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment) (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

namespace nv_helpers_dx12
{
    void TopLevelASGenerator::AddInstance(
        const D3D12_GPU_VIRTUAL_ADDRESS bottomLevelAS,
        const DirectX::XMFLOAT4X4& transform,
        const UINT instanceID,
        const UINT hitGroupIndex,
        const BYTE inclusionMask,
        const D3D12_RAYTRACING_INSTANCE_FLAGS flags
    )
    {
        m_instances.emplace_back(Instance(bottomLevelAS, transform, instanceID, hitGroupIndex, inclusionMask, flags));
    }

    void TopLevelASGenerator::ComputeASBufferSizes(
        const ComPtr<ID3D12Device5>& device,
        const bool allowUpdate,
        UINT64* scratchSizeInBytes,
        UINT64* resultSizeInBytes,
        UINT64* descriptorsSizeInBytes
    )
    {
        m_flags = allowUpdate
                      ? D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE
                      : D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;
        
        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_INPUTS
            prebuildDesc = {};
        prebuildDesc.Type = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL;
        prebuildDesc.DescsLayout = D3D12_ELEMENTS_LAYOUT_ARRAY;
        prebuildDesc.NumDescs = static_cast<UINT>(m_instances.size());
        prebuildDesc.Flags = m_flags;
        
        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_PREBUILD_INFO info = {};

        device->GetRaytracingAccelerationStructurePrebuildInfo(&prebuildDesc, &info);

        info.ResultDataMaxSizeInBytes =
            ROUND_UP(info.ResultDataMaxSizeInBytes, D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        info.ScratchDataSizeInBytes =
            ROUND_UP(info.ScratchDataSizeInBytes, D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

        m_resultSizeInBytes = info.ResultDataMaxSizeInBytes;
        m_scratchSizeInBytes = info.ScratchDataSizeInBytes;
        m_instanceDescriptionsSizeInBytes =
            ROUND_UP(sizeof(D3D12_RAYTRACING_INSTANCE_DESC) * m_instances.size(),
                     D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

        if (m_instanceDescriptionsSizeInBytes == 0)
        {
            m_instanceDescriptionsSizeInBytes = D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT;
        }

        *scratchSizeInBytes = m_scratchSizeInBytes;
        *resultSizeInBytes = m_resultSizeInBytes;
        *descriptorsSizeInBytes = m_instanceDescriptionsSizeInBytes;
    }

    void TopLevelASGenerator::Generate(
        const ComPtr<ID3D12GraphicsCommandList4>& commandList,
        const Allocation<ID3D12Resource>& scratchBuffer,
        const Allocation<ID3D12Resource>& resultBuffer,
        const Allocation<ID3D12Resource>& descriptorsBuffer,
        const bool updateOnly,
        const Allocation<ID3D12Resource>& previousResult
    ) const
    {
        D3D12_RAYTRACING_INSTANCE_DESC* instanceDescription;
        if (const HRESULT ok = descriptorsBuffer.resource->Map(
            0, nullptr, reinterpret_cast<void**>(&instanceDescription)); FAILED(ok) || !instanceDescription)
        {
            throw std::logic_error("Cannot map the instance descriptor buffer - is it in the upload heap?");
        }

        const auto instanceCount = static_cast<UINT>(m_instances.size());
        
        if (!updateOnly)
        {
            ZeroMemory(instanceDescription, m_instanceDescriptionsSizeInBytes);
        }

        for (uint32_t i = 0; i < instanceCount; i++)
        {
            instanceDescription[i].InstanceID = m_instances[i].instanceID;
            instanceDescription[i].InstanceContributionToHitGroupIndex = m_instances[i].hitGroupIndex;
            instanceDescription[i].Flags = m_instances[i].flags;

            const DirectX::XMMATRIX instance = XMLoadFloat4x4(m_instances[i].transform);
            DirectX::XMMATRIX m = XMMatrixTranspose(instance);
            std::memcpy(instanceDescription[i].Transform, &m, sizeof instanceDescription[i].Transform);
            
            instanceDescription[i].AccelerationStructure = m_instances[i].bottomLevelAS;
            instanceDescription[i].InstanceMask = m_instances[i].inclusionMask;
        }

        descriptorsBuffer.resource->Unmap(0, nullptr);
        
        const D3D12_GPU_VIRTUAL_ADDRESS sourceAS = updateOnly
                                                       ? previousResult.GetGPUVirtualAddress<ID3D12Resource>()
                                                       : 0;

        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS flags = m_flags;

        if (flags == D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE && updateOnly)
        {
            flags = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_PERFORM_UPDATE;
        }
        
        if (m_flags != D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE && updateOnly)
        {
            throw std::logic_error("Cannot update a top-level AS not originally built for updates");
        }
        if (updateOnly && !previousResult.IsSet())
        {
            throw std::logic_error("Top-level hierarchy update requires the previous hierarchy");
        }

        flags |= D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_PREFER_FAST_TRACE;
        
        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_DESC buildDesc;
        buildDesc.Inputs.Type = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL;
        buildDesc.Inputs.DescsLayout = D3D12_ELEMENTS_LAYOUT_ARRAY;
        buildDesc.Inputs.InstanceDescs = descriptorsBuffer.GetGPUVirtualAddress<ID3D12Resource>();
        buildDesc.Inputs.NumDescs = instanceCount;
        buildDesc.DestAccelerationStructureData = {
            resultBuffer.GetGPUVirtualAddress<ID3D12Resource>()
        };
        buildDesc.ScratchAccelerationStructureData = {
            scratchBuffer.GetGPUVirtualAddress<ID3D12Resource>()
        };
        buildDesc.SourceAccelerationStructureData = sourceAS;
        buildDesc.Inputs.Flags = flags;
        
        commandList->BuildRaytracingAccelerationStructure(&buildDesc, 0, nullptr);
        
        D3D12_RESOURCE_BARRIER uavBarrier;
        uavBarrier.Type = D3D12_RESOURCE_BARRIER_TYPE_UAV;
        uavBarrier.UAV.pResource = resultBuffer.Get();
        uavBarrier.Flags = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        commandList->ResourceBarrier(1, &uavBarrier);
    }

    TopLevelASGenerator::Instance::Instance(
        const D3D12_GPU_VIRTUAL_ADDRESS blAS,
        const DirectX::XMFLOAT4X4& tr,
        const UINT iID,
        const UINT hgId,
        const BYTE mask,
        const D3D12_RAYTRACING_INSTANCE_FLAGS f)
        : bottomLevelAS(blAS), transform(&tr), instanceID(iID), hitGroupIndex(hgId), flags(f), inclusionMask(mask)
    {
    }
}
