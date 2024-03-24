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

#pragma once

#include "d3d12.h"

#include <vector>

#include <DirectXMath.h>
#include <wrl/client.h>

#include "Tools/Allocation.hpp"

namespace nv_helpers_dx12
{
    /**
     * \brief Helper class to generate top-level acceleration structures for raytracing
     */
    class TopLevelASGenerator
    {
    public:
        /**
         * \brief Clear all added instances.
         */
        void Clear();
        
        /**
         * \brief Add an instance to the top-level acceleration structure. The instance is represented by a bottom-level AS, a transform, an instance ID and the index of the hit group indicating which shaders are executed upon hitting any geometry within the instance.
         * \param bottomLevelAS Bottom-level acceleration structure containing the actual geometric data of the instance.
         * \param transform Transform matrix to apply to the instance, allowing the same bottom-level AS to be used at several world-space positions.
         * \param instanceID Instance ID, which can be used in the shaders to identify this specific instance.
         * \param hitGroupIndex Hit group index, corresponding the the index of the hit group in the Shader Binding Table that will be called upon hitting the geometry.
         * \param inclusionMask Instance mask, which can be used in the shaders to hide instances.
         * \param flags Instance flags, such as D3D12_RAYTRACING_INSTANCE_FLAG_TRIANGLE_CULL_DISABLE.
         */
        void AddInstance(
            D3D12_GPU_VIRTUAL_ADDRESS       bottomLevelAS,
            DirectX::XMFLOAT4X4 const&      transform,
            UINT                            instanceID,
            UINT                            hitGroupIndex,
            BYTE                            inclusionMask,
            D3D12_RAYTRACING_INSTANCE_FLAGS flags = D3D12_RAYTRACING_INSTANCE_FLAG_NONE);

        /**
         * \brief Compute the size of the scratch space required to build the acceleration structure, as well as the size of the resulting structure. The allocation of the buffers is then left to the application.
         * \param device Device on which the build will be performed.
         * \param allowUpdate If true, the resulting acceleration structure will allow iterative updates.
         * \param scratchSizeInBytes Required scratch memory on the GPU to build the acceleration structure.
         * \param resultSizeInBytes Required GPU memory to store the acceleration structure.
         * \param descriptorsSizeInBytes Required GPU memory to store instance descriptors, containing the matrices, indices etc.
         */
        void ComputeASBufferSizes(
            ComPtr<ID3D12Device5> const& device,
            bool                         allowUpdate,
            UINT64*                      scratchSizeInBytes,
            UINT64*                      resultSizeInBytes,
            UINT64*                      descriptorsSizeInBytes);

        /**
         * \brief Enqueue the construction of the acceleration structure on a command list, using application-provided buffers and possibly a pointer to the previous acceleration structure in case of iterative updates. Note that the update can be done in place: the result and previousResult pointers can be the same.
         * \param commandList Command list on which the build will be enqueued
         * \param scratchBuffer Scratch buffer used by the builder to store temporary data
         * \param resultBuffer Result buffer storing the acceleration structure
         * \param descriptorsBuffer Auxiliary result buffer containing the instance descriptors, has to be in upload heap
         * \param updateOnly If true, simply refit the existing acceleration
         * \param previousResult Optional previous acceleration structure, used if an iterative update is requested
         */
        void Generate(
            ComPtr<ID3D12GraphicsCommandList4> const& commandList,
            Allocation<ID3D12Resource> const&         scratchBuffer,
            Allocation<ID3D12Resource> const&         resultBuffer,
            Allocation<ID3D12Resource> const&         descriptorsBuffer,
            bool                                      updateOnly     = false,
            Allocation<ID3D12Resource> const&         previousResult = {}) const;

    private:
        struct Instance
        {
            Instance(
                D3D12_GPU_VIRTUAL_ADDRESS       blAS,
                DirectX::XMFLOAT4X4 const&      tr,
                UINT                            iID,
                UINT                            hgId,
                BYTE                            mask,
                D3D12_RAYTRACING_INSTANCE_FLAGS f);

            D3D12_GPU_VIRTUAL_ADDRESS       bottomLevelAS;
            DirectX::XMFLOAT4X4 const*      transform;
            UINT                            instanceID;
            UINT                            hitGroupIndex;
            D3D12_RAYTRACING_INSTANCE_FLAGS flags;
            BYTE                            inclusionMask;
        };

        D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAGS m_flags =
            D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BUILD_FLAG_NONE;
        std::vector<Instance> m_instances{};

        UINT64 m_scratchSizeInBytes              = 0;
        UINT64 m_instanceDescriptionsSizeInBytes = 0;
        UINT64 m_resultSizeInBytes               = 0;
    };
}
