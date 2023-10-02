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



Example:

TopLevelASGenerator topLevelAS;
topLevelAS.AddInstance(instances1, matrix1, instanceId1, hitGroupIndex1);
topLevelAS.AddInstance(instances2, matrix2, instanceId2, hitGroupIndex2);
...
UINT64 scratchSize, resultSize, instanceDescsSize;
topLevelAS.ComputeASBufferSizes(GetRTDevice(), true, &scratchSize, &resultSize,
&instanceDescsSize); AccelerationStructureBuffers buffers; buffers.pScratch =
nv_helpers_dx12::CreateBuffer(..., scratchSizeInBytes, ...); buffers.pResult =
nv_helpers_dx12::CreateBuffer(..., resultSizeInBytes, ...);
buffers.pInstanceDesc = nv_helpers_dx12::CreateBuffer(..., resultSizeInBytes,
...); topLevelAS.Generate(m_commandList.Get(), rtCmdList,
m_topLevelAS.pScratch.Get(), m_topLevelAS.pResult.Get(),
m_topLevelAS.pInstanceDesc.Get(), updateOnly, updateOnly ?
m_topLevelAS.pResult.Get() : nullptr);

return buffers;

*/

#pragma once

#include "d3d12.h"

#include <DirectXMath.h>

#include <vector>
#include <wrl/client.h>

namespace nv_helpers_dx12
{
    /// Helper class to generate top-level acceleration structures for raytracing
    class TopLevelASGenerator
    {
    public:
        /// Add an instance to the top-level acceleration structure. The instance is
        /// represented by a bottom-level AS, a transform, an instance ID and the
        /// index of the hit group indicating which shaders are executed upon hitting
        /// any geometry within the instance
        /// \param bottomLevelAS Bottom-level acceleration structure containing the actual geometric data of the instance
        /// \param transform Transform matrix to apply to the instance, allowing the same bottom-level AS to be used at several world-space positions
        /// \param instanceID Instance ID, which can be used in the shaders to identify this specific instance
        /// \param hitGroupIndex Hit group index, corresponding the the index of the hit group in the Shader Binding Table that will be called upon hitting the geometry
        /// \param inclusionMask Instance mask, which can be used in the shaders to hide instances.
        /// \param flags Instance flags, such as D3D12_RAYTRACING_INSTANCE_FLAG_TRIANGLE_CULL_DISABLE
        void
        AddInstance(D3D12_GPU_VIRTUAL_ADDRESS bottomLevelAS,
                    const DirectX::XMFLOAT4X4& transform,
                    UINT instanceID,
                    UINT hitGroupIndex,
                    BYTE inclusionMask,
                    D3D12_RAYTRACING_INSTANCE_FLAGS flags = D3D12_RAYTRACING_INSTANCE_FLAG_NONE
        );

        /// Compute the size of the scratch space required to build the acceleration
        /// structure, as well as the size of the resulting structure. The allocation
        /// of the buffers is then left to the application
        /// \param device Device on which the build will be performed
        /// \param allowUpdate If true, the resulting acceleration structure will allow iterative updates
        /// \param scratchSizeInBytes Required scratch memory on the GPU to build the acceleration structure
        /// \param resultSizeInBytes Required GPU memory to store the acceleration structure
        /// \param descriptorsSizeInBytes Required GPU memory to store instance descriptors, containing the matrices, indices etc.
        void ComputeASBufferSizes(
            ID3D12Device5* device,
            bool allowUpdate,
            UINT64* scratchSizeInBytes,
            UINT64* resultSizeInBytes,
            UINT64* descriptorsSizeInBytes
        );

        /// Enqueue the construction of the acceleration structure on a command list,
        /// using application-provided buffers and possibly a pointer to the previous
        /// acceleration structure in case of iterative updates. Note that the update
        /// can be done in place: the result and previousResult pointers can be the
        /// same.
        /// \param commandList Command list on which the build will be enqueued
        /// \param scratchBuffer Scratch buffer used by the builder to store temporary data
        /// \param resultBuffer Result buffer storing the acceleration structure
        /// \param descriptorsBuffer Auxiliary result buffer containing the instance descriptors, has to be in upload heap
        /// \param updateOnly If true, simply refit the existing acceleration
        /// \param previousResult Optional previous acceleration structure, used if an iterative update is requested
        void Generate(
            ID3D12GraphicsCommandList4* commandList,
            ID3D12Resource* scratchBuffer,
            ID3D12Resource* resultBuffer,
            ID3D12Resource* descriptorsBuffer,
            bool updateOnly = false,
            ID3D12Resource* previousResult = nullptr
        );

    private:
        /// Helper struct storing the instance data
        struct Instance
        {
            Instance(D3D12_GPU_VIRTUAL_ADDRESS blAS, const DirectX::XMFLOAT4X4& tr, UINT iID, UINT hgId, BYTE mask,
                     D3D12_RAYTRACING_INSTANCE_FLAGS f);
            /// Bottom-level AS
            D3D12_GPU_VIRTUAL_ADDRESS bottomLevelAS;
            /// Transform matrix
            const DirectX::XMFLOAT4X4& transform;
            /// Instance ID visible in the shader
            UINT instanceID;
            /// Hit group index used to fetch the shaders from the SBT
            UINT hitGroupIndex;
            /// Instance flags, such as D3D12_RAYTRACING_INSTANCE_FLAG_TRIANGLE_CULL_DISABLE
            D3D12_RAYTRACING_INSTANCE_FLAGS flags;
            /// Instance mask, which can be used in the shaders to hide instances.
            BYTE inclusionMask;
        };

        /// Construction flags, indicating whether the AS supports iterative updates
        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS m_flags =
            D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;
        /// Instances contained in the top-level AS
        std::vector<Instance> m_instances{};

        /// Size of the temporary memory used by the TLAS builder
        UINT64 m_scratchSizeInBytes = 0;
        /// Size of the buffer containing the instance descriptors
        UINT64 m_instanceDescriptionsSizeInBytes = 0;
        /// Size of the buffer containing the TLAS
        UINT64 m_resultSizeInBytes = 0;
    };
} // namespace nv_helpers_dx12
