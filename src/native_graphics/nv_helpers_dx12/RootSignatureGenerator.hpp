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

#include <tuple>
#include <vector>
#include <wrl/client.h>

namespace nv_helpers_dx12
{
    /**
     * \brief Helper class to create root signatures.
     */
    class RootSignatureGenerator
    {
    public:
        /**
         * \brief Add a set of heap range descriptors as a parameter of the root signature.
         * \param ranges The set of ranges to add.
         */
        void AddHeapRangesParameter(std::vector<D3D12_DESCRIPTOR_RANGE> const& ranges);

        using HeapRange = std::tuple<UINT,                        // BaseShaderRegister,
                                     UINT,                        // NumDescriptors
                                     UINT,                        // RegisterSpace
                                     D3D12_DESCRIPTOR_RANGE_TYPE, // RangeType
                                     UINT                         // OffsetInDescriptorsFromTableStart
        >;

        /**
         * \brief Add a set of heap range descriptors as a parameter of the root signature.
         * \param ranges The set of ranges to add.
         */
        void AddHeapRangesParameter(std::vector<HeapRange> const& ranges);

        /**
         * \brief Add a root parameter to the shader, defined by its type.
         * \param type The type of the parameter, e.g. constant buffer (CBV), shader resource (SRV), unordered access (UAV), or root constant (CBV, directly defined by its value instead of a buffer).
         * \param shaderRegister Indicates how to access the parameter in the HLSL code, e.g a SRV with shaderRegister==1 and registerSpace==0 is accessible via register(t1, space0).
         * \param registerSpace Indicates how to access the parameter in the HLSL code, e.g a SRV with shaderRegister==1 and registerSpace==0 is accessible via register(t1, space0).
         * \param numRootConstants In case of a root constant, this parameter indicates how many successive 32-bit constants will be bound.
         */
        void AddRootParameter(
            D3D12_ROOT_PARAMETER_TYPE type,
            UINT                      shaderRegister   = 0,
            UINT                      registerSpace    = 0,
            UINT                      numRootConstants = 1);

        /**
         * \brief Add a static sampler to the root signature. The sampler is defined by a D3D12_STATIC_SAMPLER_DESC.
         * \param sampler The sampler to add.
         */
        void AddStaticSampler(D3D12_STATIC_SAMPLER_DESC const* sampler);

        /**
         * \brief Set whether input assembler is allowed.
         * \param useInputAssembler True if input assembler is allowed, false otherwise.
         */
        void SetInputAssembler(bool const useInputAssembler) { m_allowInputAssembler = useInputAssembler; }

        /**
         * \brief Create the root signature from the set of parameters, in the order of the addition calls
         * \param device The device to create the root signature on.
         * \param isLocal Whether the root signature is local to the device or shared across multiple devices.
         * \return The root signature.
         */
        Microsoft::WRL::ComPtr<ID3D12RootSignature> Generate(
            Microsoft::WRL::ComPtr<ID3D12Device> const& device,
            bool                                        isLocal);

    private:
        std::vector<std::vector<D3D12_DESCRIPTOR_RANGE>> m_ranges         = {};
        std::vector<D3D12_ROOT_PARAMETER>                m_parameters     = {};
        std::vector<D3D12_STATIC_SAMPLER_DESC>           m_staticSamplers = {};

        /**
         * \brief For each entry of m_parameter, indicate the index of the range array in m_ranges, and ~0u if the parameter is not a heap range descriptor.
         */
        std::vector<UINT> m_rangeLocations = {};

        bool m_allowInputAssembler = false;

        enum
        {
            RSC_BASE_SHADER_REGISTER                   = 0,
            RSC_NUM_DESCRIPTORS                        = 1,
            RSC_REGISTER_SPACE                         = 2,
            RSC_RANGE_TYPE                             = 3,
            RSC_OFFSET_IN_DESCRIPTORS_FROM_TABLE_START = 4
        };
    };
}
