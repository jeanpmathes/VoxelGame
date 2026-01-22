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

#include "DXRHelper.hpp"

namespace nv_helpers_dx12
{
    void TopLevelASGenerator::Clear()
    {
        m_instances.clear();

        m_resultSizeInBytes               = 0;
        m_scratchSizeInBytes              = 0;
        m_instanceDescriptionsSizeInBytes = 0;
    }

    void TopLevelASGenerator::AddInstance(
        D3D12_GPU_VIRTUAL_ADDRESS const       bottomLevelAS,
        DirectX::XMFLOAT4X4 const&            transform,
        UINT const                            instanceID,
        UINT const                            hitGroupIndex,
        BYTE const                            inclusionMask,
        D3D12_RAYTRACING_INSTANCE_FLAGS const flags)
    {
        m_instances.emplace_back(bottomLevelAS, transform, instanceID, hitGroupIndex, inclusionMask, flags);
    }

    void TopLevelASGenerator::ComputeASBufferSizes(
        ComPtr<ID3D12Device5> const& device,
        bool const                   allowUpdate,
        UINT64*                      scratchSizeInBytes,
        UINT64*                      resultSizeInBytes,
        UINT64*                      descriptorsSizeInBytes)
    {
        m_flags = allowUpdate
                      ? D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE
                      : D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;

        m_flags |= D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_PREFER_FAST_TRACE;

        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_INPUTS prebuildDesc = {};
        prebuildDesc.Type = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL;
        prebuildDesc.DescsLayout = D3D12_ELEMENTS_LAYOUT_ARRAY;
        prebuildDesc.NumDescs = static_cast<UINT>(m_instances.size());
        prebuildDesc.Flags = m_flags;

        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_PREBUILD_INFO info = {};

        device->GetRaytracingAccelerationStructurePrebuildInfo(&prebuildDesc, &info);

        info.ResultDataMaxSizeInBytes = RoundUp(
            info.ResultDataMaxSizeInBytes,
            D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        info.ScratchDataSizeInBytes = RoundUp(
            info.ScratchDataSizeInBytes,
            D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

        m_resultSizeInBytes               = info.ResultDataMaxSizeInBytes;
        m_scratchSizeInBytes              = info.ScratchDataSizeInBytes;
        m_instanceDescriptionsSizeInBytes = RoundUp(
            sizeof(D3D12_RAYTRACING_INSTANCE_DESC) * m_instances.size(),
            D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

        if (m_instanceDescriptionsSizeInBytes == 0) m_instanceDescriptionsSizeInBytes =
            D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT;

        *scratchSizeInBytes     = m_scratchSizeInBytes;
        *resultSizeInBytes      = m_resultSizeInBytes;
        *descriptorsSizeInBytes = m_instanceDescriptionsSizeInBytes;
    }

    void TopLevelASGenerator::Generate(
        ComPtr<ID3D12GraphicsCommandList4> const& commandList,
        Allocation<ID3D12Resource> const&         scratchBuffer,
        Allocation<ID3D12Resource> const&         resultBuffer,
        Allocation<ID3D12Resource> const&         descriptorsBuffer,
        bool const                                updateOnly,
        Allocation<ID3D12Resource> const&         previousResult) const
    {
        constexpr D3D12_RANGE none = {0, 0};

        D3D12_RAYTRACING_INSTANCE_DESC* instanceDescription;
        if (HRESULT const ok = descriptorsBuffer.resource->Map(
                0,
                &none,
                reinterpret_cast<void**>(&instanceDescription));
            FAILED(ok) || !instanceDescription)
            throw std::logic_error("Cannot map the instance descriptor buffer - is it in the upload heap?");

        auto const instanceCount = static_cast<UINT>(m_instances.size());

        if (!updateOnly)
            ZeroMemory(instanceDescription, m_instanceDescriptionsSizeInBytes);

        for (uint32_t i = 0; i < instanceCount; i++)
        {
            instanceDescription[i].InstanceID                          = m_instances[i].instanceID;
            instanceDescription[i].InstanceContributionToHitGroupIndex = m_instances[i].hitGroupIndex;
            instanceDescription[i].Flags                               = m_instances[i].flags;

            DirectX::XMMATRIX const instance   = XMLoadFloat4x4(m_instances[i].transform);
            DirectX::XMMATRIX       transposed = XMMatrixTranspose(instance);
            std::memcpy(instanceDescription[i].Transform, &transposed, sizeof instanceDescription[i].Transform);

            instanceDescription[i].AccelerationStructure = m_instances[i].bottomLevelAS;
            instanceDescription[i].InstanceMask          = m_instances[i].inclusionMask;
        }

        descriptorsBuffer.resource->Unmap(0, nullptr);

        D3D12_GPU_VIRTUAL_ADDRESS const sourceAS = updateOnly
                                                       ? previousResult.GetGPUVirtualAddress<ID3D12Resource>()
                                                       : 0;

        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS flags = m_flags;

        if (flags == D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_ALLOW_UPDATE && updateOnly) flags =
            D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_PERFORM_UPDATE;

        D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_DESC buildDesc;
        buildDesc.Inputs.Type                      = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL;
        buildDesc.Inputs.DescsLayout               = D3D12_ELEMENTS_LAYOUT_ARRAY;
        buildDesc.Inputs.InstanceDescs             = descriptorsBuffer.GetGPUVirtualAddress<ID3D12Resource>();
        buildDesc.Inputs.NumDescs                  = instanceCount;
        buildDesc.DestAccelerationStructureData    = {resultBuffer.GetGPUVirtualAddress<ID3D12Resource>()};
        buildDesc.ScratchAccelerationStructureData = {scratchBuffer.GetGPUVirtualAddress<ID3D12Resource>()};
        buildDesc.SourceAccelerationStructureData  = sourceAS;
        buildDesc.Inputs.Flags                     = flags;

        commandList->BuildRaytracingAccelerationStructure(&buildDesc, 0, nullptr);

        D3D12_RESOURCE_BARRIER uavBarrier;
        uavBarrier.Type          = D3D12_RESOURCE_BARRIER_TYPE_UAV;
        uavBarrier.UAV.pResource = resultBuffer.Get();
        uavBarrier.Flags         = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        commandList->ResourceBarrier(1, &uavBarrier);
    }

    TopLevelASGenerator::Instance::Instance(
        D3D12_GPU_VIRTUAL_ADDRESS const       blAS,
        DirectX::XMFLOAT4X4 const&            tr,
        UINT const                            iID,
        UINT const                            hgId,
        BYTE const                            mask,
        D3D12_RAYTRACING_INSTANCE_FLAGS const f)
        : bottomLevelAS(blAS)
      , transform(&tr)
      , instanceID(iID)
      , hitGroupIndex(hgId)
      , flags(f)
      , inclusionMask(mask)
    {
    }
}
