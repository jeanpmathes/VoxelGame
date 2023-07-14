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
#include <string>
#include <stdexcept>

// Helper to compute aligned buffer sizes
#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment) (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

namespace nv_helpers_dx12
{
    void ShaderBindingTableGenerator::AddRayGenerationProgram(const std::wstring& entryPoint,
                                                              const std::vector<void*>& inputData)
    {
        m_rayGen.emplace_back(SBTEntry(entryPoint, inputData));
    }

    void ShaderBindingTableGenerator::AddMissProgram(const std::wstring& entryPoint,
                                                     const std::vector<void*>& inputData)
    {
        m_miss.emplace_back(entryPoint, inputData);
    }

    void ShaderBindingTableGenerator::AddHitGroup(const std::wstring& entryPoint,
                                                  const std::vector<void*>& inputData)
    {
        m_hitGroup.emplace_back(entryPoint, inputData);
    }

    uint32_t ShaderBindingTableGenerator::ComputeSBTSize()
    {
        // Size of a program identifier
        m_progIdSize = D3D12_RAYTRACING_SHADER_RECORD_BYTE_ALIGNMENT;
        // Compute the entry size of each program type depending on the maximum number of parameters in
        // each category
        m_rayGenEntrySize = GetEntrySize(m_rayGen);
        m_missEntrySize = GetEntrySize(m_miss);
        m_hitGroupEntrySize = GetEntrySize(m_hitGroup);

        const uint32_t rayGenSize = static_cast<uint32_t>(m_rayGen.size()) * m_rayGenEntrySize;
        const uint32_t missSize = static_cast<uint32_t>(m_miss.size()) * m_missEntrySize;
        const uint32_t hitGroupSize = static_cast<uint32_t>(m_hitGroup.size()) * m_hitGroupEntrySize;

        const uint32_t totalSize = ROUND_UP(rayGenSize, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT) +
            ROUND_UP(missSize, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT) +
            ROUND_UP(hitGroupSize, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT);

        // The total SBT size is the sum of the entries for ray generation, miss and hit groups, aligned
        // on 256 bytes
        const uint32_t sbtSize = ROUND_UP(totalSize, 256);

        return sbtSize;
    }

    void ShaderBindingTableGenerator::Generate(ID3D12Resource* sbtBuffer,
                                               ID3D12StateObjectProperties* raytracingPipeline)
    {
        // Map the SBT
        uint8_t* pData;
        HRESULT hr = sbtBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData));
        if (FAILED(hr))
        {
            throw std::logic_error("Could not map the shader binding table");
        }

        // Copy the shader identifiers followed by their resource pointers or root constants: first the
        // ray generation, then the miss shaders, and finally the set of hit groups
        uint32_t offset = 0;
        uint32_t totalOffset = 0;
        m_rayGenStart = offset;

        offset = CopyShaderData(raytracingPipeline, pData, m_rayGen, m_rayGenEntrySize);
        offset = ROUND_UP(offset, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT);

        totalOffset += offset;
        pData += offset;
        m_missStart = totalOffset;

        offset = CopyShaderData(raytracingPipeline, pData, m_miss, m_missEntrySize);
        offset = ROUND_UP(offset, D3D12_RAYTRACING_SHADER_TABLE_BYTE_ALIGNMENT);

        totalOffset += offset;
        pData += offset;
        m_hitGroupStart = totalOffset;

        CopyShaderData(raytracingPipeline, pData, m_hitGroup, m_hitGroupEntrySize);

        sbtBuffer->Unmap(0, nullptr);
    }

    void ShaderBindingTableGenerator::Reset()
    {
        m_rayGen.clear();
        m_miss.clear();
        m_hitGroup.clear();

        m_rayGenEntrySize = 0;
        m_missEntrySize = 0;
        m_hitGroupEntrySize = 0;
        m_progIdSize = 0;
    }

    UINT ShaderBindingTableGenerator::GetRayGenSectionSize() const
    {
        return m_rayGenEntrySize * static_cast<UINT>(m_rayGen.size());
    }

    UINT ShaderBindingTableGenerator::GetRayGenEntrySize() const
    {
        return m_rayGenEntrySize;
    }

    UINT ShaderBindingTableGenerator::GetRayGenSectionOffset() const
    {
        return m_rayGenStart;
    }

    UINT ShaderBindingTableGenerator::GetMissSectionSize() const
    {
        return m_missEntrySize * static_cast<UINT>(m_miss.size());
    }

    UINT ShaderBindingTableGenerator::GetMissEntrySize() const
    {
        return m_missEntrySize;
    }

    UINT ShaderBindingTableGenerator::GetMissSectionOffset() const
    {
        return m_missStart;
    }

    UINT ShaderBindingTableGenerator::GetHitGroupSectionSize() const
    {
        return m_hitGroupEntrySize * static_cast<UINT>(m_hitGroup.size());
    }

    UINT ShaderBindingTableGenerator::GetHitGroupEntrySize() const
    {
        return m_hitGroupEntrySize;
    }

    UINT ShaderBindingTableGenerator::GetHitGroupSectionOffset() const
    {
        return m_hitGroupStart;
    }

    uint32_t ShaderBindingTableGenerator::CopyShaderData(
        ID3D12StateObjectProperties* raytracingPipeline, uint8_t* outputData,
        const std::vector<SBTEntry>& shaders, uint32_t entrySize) const
    {
        uint8_t* pData = outputData;
        for (const auto& shader : shaders)
        {
            // Get the shader identifier, and check whether that identifier is known
            void* id = raytracingPipeline->GetShaderIdentifier(shader.m_entryPoint.c_str());
            if (!id)
            {
                std::wstring errMsg(std::wstring(L"Unknown shader identifier used in the SBT: ") +
                    shader.m_entryPoint);

                std::string transformedErrMsg;
                std::ranges::transform(errMsg, std::back_inserter(transformedErrMsg),
                                       [](const wchar_t c) { return static_cast<char>(c); });

                throw std::logic_error(transformedErrMsg);
            }
            // Copy the shader identifier
            memcpy(pData, id, m_progIdSize);
            // Copy all its resources pointers or values in bulk
            memcpy(pData + m_progIdSize, shader.m_inputData.data(), shader.m_inputData.size() * 8);

            pData += entrySize;
        }
        // Return the number of bytes actually written to the output buffer
        return static_cast<uint32_t>(shaders.size()) * entrySize;
    }

    uint32_t ShaderBindingTableGenerator::GetEntrySize(const std::vector<SBTEntry>& entries) const
    {
        // Find the maximum number of parameters used by a single entry
        size_t maxArgs = 0;
        for (const auto& shader : entries)
        {
            maxArgs = max(maxArgs, shader.m_inputData.size());
        }
        // A SBT entry is made of a program ID and a set of parameters, taking 8 bytes each. Those
        // parameters can either be 8-bytes pointers, or 4-bytes constants
        uint32_t entrySize = m_progIdSize + 8 * static_cast<uint32_t>(maxArgs);

        // The entries of the shader binding table must be 32-bytes-aligned
        entrySize = ROUND_UP(entrySize, D3D12_RAYTRACING_SHADER_RECORD_BYTE_ALIGNMENT);

        return entrySize;
    }

    ShaderBindingTableGenerator::SBTEntry::SBTEntry(std::wstring entryPoint,
                                                    std::vector<void*> inputData)
        : m_entryPoint(std::move(entryPoint)), m_inputData(std::move(inputData))
    {
    }
} // namespace nv_helpers_dx12
