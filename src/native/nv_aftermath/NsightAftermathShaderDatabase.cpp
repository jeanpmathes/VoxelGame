//*********************************************************
//
// Copyright (c) 2019-2022, NVIDIA CORPORATION. All rights reserved.
// 
//  Permission is hereby granted, free of charge, to any person obtaining a
//  copy of this software and associated documentation files (the "Software"),
//  to deal in the Software without restriction, including without limitation
//  the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
//  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//  DEALINGS IN THE SOFTWARE.
//
//*********************************************************

#include "stdafx.h"

bool ShaderDatabase::FindShaderBinary(const GFSDK_Aftermath_ShaderBinaryHash& shaderHash,
                                      std::vector<uint8_t>& shader) const
{
    const auto iterator = m_shaderBinaries.find(shaderHash);
    if (iterator == m_shaderBinaries.end()) return false;

    shader = iterator->second;
    return true;
}

bool ShaderDatabase::FindSourceShaderDebugData(const GFSDK_Aftermath_ShaderDebugName& shaderDebugName,
                                               std::vector<uint8_t>& debugData) const
{
    // Find shader debug data for the shader debug name.
    const auto iterator = m_sourceShaderDebugData.find(shaderDebugName);
    if (iterator == m_sourceShaderDebugData.end()) return false;

    debugData = iterator->second;
    return true;
}

void ShaderDatabase::AddShader(std::vector<uint8_t>&& binary, std::vector<uint8_t>&& pdb)
{
    const D3D12_SHADER_BYTECODE shader{binary.data(), binary.size()};

    GFSDK_Aftermath_ShaderBinaryHash shaderHash;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderHash(
        GFSDK_Aftermath_Version_API,
        &shader,
        &shaderHash));

    GFSDK_Aftermath_ShaderDebugName debugName;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderDebugName(
        GFSDK_Aftermath_Version_API,
        &shader,
        &debugName));

    const std::string string = debugName.name;
    const std::string name = std::filesystem::path(string).stem().string();

    GpuCrashTracker::WriteToAftermathFile(name + ".cso", reinterpret_cast<const std::byte*>(binary.data()),
                                          binary.size());
    GpuCrashTracker::WriteToAftermathFile(name + ".pdb", reinterpret_cast<const std::byte*>(pdb.data()), pdb.size());

    m_shaderBinaries[shaderHash].swap(binary);
    m_sourceShaderDebugData[debugName].swap(pdb);
}
