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

The ShaderBindingTable is a helper to construct the SBT. It helps to maintain the
proper offsets of each element, required when constructing the SBT, but also when filling the
dispatch rays description.

*/

#include "ShaderBindingTableGenerator.hpp"

#include <algorithm>
#include <iterator>
#include <stdexcept>
#include <string>

#include "DXRHelper.hpp"

namespace nv_helpers_dx12
{
    void ShaderBindingTableGenerator::AddRayGenerationProgram(
        std::wstring const&       entryPoint,
        std::vector<void*> const& inputData) { m_rayGen.emplace_back(entryPoint, inputData); }

    void ShaderBindingTableGenerator::AddMissProgram(
        std::wstring const&       entryPoint,
        std::vector<void*> const& inputData) { m_miss.emplace_back(entryPoint, inputData); }

    void ShaderBindingTableGenerator::AddHitGroup(std::wstring const& entryPoint, std::vector<void*> const& inputData)
    {
        m_hitGroup.emplace_back(entryPoint, inputData);
    }

    uint32_t ShaderBindingTableGenerator::ComputeSBTSize()
    {
        m_programIdSize     = D3D12_RAYTRACING_SHADER_RECORD_BYTE_ALIGNMENT;
        m_rayGenEntrySize   = GetEntrySize(m_rayGen);
        m_missEntrySize     = GetEntrySize(m_miss);
        m_hitGroupEntrySize = GetEntrySize(m_hitGroup);

        uint32_t const rayGenSize   = static_cast<uint32_t>(m_rayGen.size()) * m_rayGenEntrySize;
        uint32_t const missSize     = static_cast<uint32_t>(m_miss.size()) * m_missEntrySize;
        uint32_t const hitGroupSize = static_cast<uint32_t>(m_hitGroup.size()) * m_hitGroupEntrySize;

        uint32_t const totalSize = RoundUp(rayGenSize, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT) + RoundUp(
            missSize,
            D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT) + RoundUp(
            hitGroupSize,
            D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT);

        uint32_t const sbtSize = RoundUp(totalSize, 256);

        return sbtSize;
    }

    void ShaderBindingTableGenerator::Generate(
        ID3D12Resource*              sbtBuffer,
        ID3D12StateObjectProperties* raytracingPipeline)
    {
        uint8_t* pData;

        if (HRESULT const hr = sbtBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData));
            FAILED(hr))
            throw std::logic_error("Could not map the shader binding table.");

        uint32_t offset      = 0;
        uint32_t totalOffset = 0;
        m_rayGenStart        = offset;

        offset = CopyShaderData(raytracingPipeline, pData, m_rayGen, m_rayGenEntrySize);
        offset = RoundUp(offset, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT);

        totalOffset += offset;
        pData       += offset;
        m_missStart = totalOffset;

        offset = CopyShaderData(raytracingPipeline, pData, m_miss, m_missEntrySize);
        offset = RoundUp(offset, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT);

        totalOffset     += offset;
        pData           += offset;
        m_hitGroupStart = totalOffset;

        CopyShaderData(raytracingPipeline, pData, m_hitGroup, m_hitGroupEntrySize);

        sbtBuffer->Unmap(0, nullptr);
    }

    void ShaderBindingTableGenerator::Reset()
    {
        m_rayGen.clear();
        m_miss.clear();
        m_hitGroup.clear();

        m_rayGenEntrySize   = 0;
        m_missEntrySize     = 0;
        m_hitGroupEntrySize = 0;
        m_programIdSize     = 0;
    }

    UINT ShaderBindingTableGenerator::GetRayGenSectionSize() const
    {
        return m_rayGenEntrySize * static_cast<UINT>(m_rayGen.size());
    }

    UINT ShaderBindingTableGenerator::GetRayGenEntrySize() const { return m_rayGenEntrySize; }

    UINT ShaderBindingTableGenerator::GetRayGenSectionOffset() const { return m_rayGenStart; }

    UINT ShaderBindingTableGenerator::GetMissSectionSize() const
    {
        return m_missEntrySize * static_cast<UINT>(m_miss.size());
    }

    UINT ShaderBindingTableGenerator::GetMissEntrySize() const { return m_missEntrySize; }

    UINT ShaderBindingTableGenerator::GetMissSectionOffset() const { return m_missStart; }

    UINT ShaderBindingTableGenerator::GetHitGroupSectionSize() const
    {
        return m_hitGroupEntrySize * static_cast<UINT>(m_hitGroup.size());
    }

    UINT ShaderBindingTableGenerator::GetHitGroupEntrySize() const { return m_hitGroupEntrySize; }

    UINT ShaderBindingTableGenerator::GetHitGroupSectionOffset() const { return m_hitGroupStart; }

    uint32_t ShaderBindingTableGenerator::CopyShaderData(
        ID3D12StateObjectProperties* raytracingPipeline,
        uint8_t*                     outputData,
        std::vector<SBTEntry> const& shaders,
        uint32_t const               entrySize) const
    {
        uint8_t* pData = outputData;
        for (auto const& shader : shaders)
        {
            void const* id = raytracingPipeline->GetShaderIdentifier(shader.entryPoint.c_str());
            if (!id)
            {
                std::string transformedIdentifier;
                std::ranges::transform(
                    shader.entryPoint,
                    std::back_inserter(transformedIdentifier),
                    [](wchar_t const c) { return static_cast<char>(c); });

                throw std::logic_error("Unknown shader identifier used in the SBT: " + transformedIdentifier);
            }

            static_assert(sizeof(void*) == 8);

            memcpy(pData, id, m_programIdSize);
            memcpy(pData + m_programIdSize, shader.inputData.data(), shader.inputData.size() * sizeof(void*));

            pData += entrySize;
        }

        return static_cast<uint32_t>(shaders.size()) * entrySize;
    }

    uint32_t ShaderBindingTableGenerator::GetEntrySize(std::vector<SBTEntry> const& entries) const
    {
        size_t maxArgs = 0;
        for (auto const& shader : entries) maxArgs = max(maxArgs, shader.inputData.size());

        uint32_t entrySize = m_programIdSize + 8 * static_cast<uint32_t>(maxArgs);
        entrySize          = RoundUp(entrySize, D3D12_RAYTRACING_SHADER_RECORD_BYTE_ALIGNMENT);

        return entrySize;
    }

    ShaderBindingTableGenerator::SBTEntry::SBTEntry(std::wstring entryPoint, std::vector<void*> inputData)
        : entryPoint(std::move(entryPoint))
      , inputData(std::move(inputData))
    {
    }
}
