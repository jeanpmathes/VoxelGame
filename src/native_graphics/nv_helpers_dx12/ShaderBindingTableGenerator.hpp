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

#include <string>
#include <vector>

namespace nv_helpers_dx12
{
    /**
     * \brief Helper class to create and maintain a Shader Binding Table (SBT).
     */
    class ShaderBindingTableGenerator
    {
    public:
        /**
         * \brief Add a ray generation program by name, with its list of data pointers or values according to the layout of its root signature.
         * \param entryPoint The name of the program.
         * \param inputData The list of data pointers or values according to the layout of its root signature.
         */
        void AddRayGenerationProgram(std::wstring const& entryPoint, std::vector<void*> const& inputData);

        /**
         * \brief Add a miss program by name, with its list of data pointers or values according to the layout of its root signature.
         * \param entryPoint The name of the program.
         * \param inputData The list of data pointers or values according to the layout of its root signature.
         */
        void AddMissProgram(std::wstring const& entryPoint, std::vector<void*> const& inputData);

        /**
         * \brief Add a hit group by name, with its list of data pointers or values according to the layout of its root signature.
         * \param entryPoint The name of the program.
         * \param inputData The list of data pointers or values according to the layout of its root signature.
         */
        void AddHitGroup(std::wstring const& entryPoint, std::vector<void*> const& inputData);

        /**
         * \brief Compute the size of the SBT based on the set of programs and hit groups it contains.
         * \return The size in bytes of the SBT.
         */
        uint32_t ComputeSBTSize();

        /**
         * \brief Build the SBT and store it into sbtBuffer, which has to be pre-allocated on the upload heap.
         * \param sbtBuffer The pre-allocated buffer on the upload heap.
         * \param raytracingPipeline The raytracing pipeline object.
         */
        void Generate(ID3D12Resource* sbtBuffer, ID3D12StateObjectProperties* raytracingPipeline);

        /**
         * \brief Reset the sets of programs and hit groups
         */
        void Reset();

        [[nodiscard]] UINT GetRayGenSectionSize() const;
        [[nodiscard]] UINT GetRayGenEntrySize() const;
        [[nodiscard]] UINT GetRayGenSectionOffset() const;

        [[nodiscard]] UINT GetMissSectionSize() const;
        [[nodiscard]] UINT GetMissEntrySize() const;
        [[nodiscard]] UINT GetMissSectionOffset() const;

        [[nodiscard]] UINT GetHitGroupSectionSize() const;
        [[nodiscard]] UINT GetHitGroupEntrySize() const;
        [[nodiscard]] UINT GetHitGroupSectionOffset() const;

    private:
        struct SBTEntry
        {
            SBTEntry(std::wstring entryPoint, std::vector<void*> inputData);

            std::wstring       entryPoint;
            std::vector<void*> inputData;
        };

        uint32_t CopyShaderData(ID3D12StateObjectProperties* raytracingPipeline, uint8_t* outputData, std::vector<SBTEntry> const& shaders, uint32_t entrySize) const;

        [[nodiscard]] uint32_t GetEntrySize(std::vector<SBTEntry> const& entries) const;

        std::vector<SBTEntry> m_rayGen;
        std::vector<SBTEntry> m_miss;
        std::vector<SBTEntry> m_hitGroup;

        uint32_t m_rayGenEntrySize   = 0;
        uint32_t m_missEntrySize     = 0;
        uint32_t m_hitGroupEntrySize = 0;

        uint32_t m_rayGenStart   = 0;
        uint32_t m_missStart     = 0;
        uint32_t m_hitGroupStart = 0;

        UINT m_programIdSize = 0;
    };
}
