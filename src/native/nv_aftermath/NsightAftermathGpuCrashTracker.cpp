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

#include <fstream>
#include <string>
#include <array>

#include "NsightAftermathGpuCrashTracker.hpp"

GpuCrashTracker::GpuCrashTracker(const MarkerMap& markerMap, const ShaderDatabase& shaderDatabase)
    : m_initialized(false)
      , m_markerMap(markerMap)
      , m_shaderDatabase(shaderDatabase)
{
}

GpuCrashTracker::~GpuCrashTracker()
{
    if (m_initialized)
    {
        GFSDK_Aftermath_DisableGpuCrashDumps();
    }
}

void GpuCrashTracker::Initialize()
{
    // Enable GPU crash dumps and set up the callbacks for crash dump notifications,
    // shader debug information notifications, and providing additional crash
    // dump description data. Only the crash dump callback is mandatory. The other two
    // callbacks are optional and can be omitted, by passing nullptr, if the corresponding
    // functionality is not used.
    // The DeferDebugInfoCallbacks flag enables caching of shader debug information data
    // in memory. If the flag is set, ShaderDebugInfoCallback will be called only
    // in the event of a crash, right before GpuCrashDumpCallback. If the flag is not set,
    // ShaderDebugInfoCallback will be called for every shader that is compiled.
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_EnableGpuCrashDumps(
        GFSDK_Aftermath_Version_API,
        GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_DX,
        GFSDK_Aftermath_GpuCrashDumpFeatureFlags_DeferDebugInfoCallbacks,
        // Let the Nsight Aftermath library cache shader debug information.
        GpuCrashDumpCallback, // Register callback for GPU crash dumps.
        ShaderDebugInfoCallback, // Register callback for shader debug information.
        CrashDumpDescriptionCallback, // Register callback for GPU crash dump description.
        ResolveMarkerCallback, // Register callback for resolving application-managed markers.
        this)); // Set the GpuCrashTracker object as user data for the above callbacks.

    m_initialized = true;
}

void GpuCrashTracker::WriteToAftermathFile(const std::string& name, const std::byte* data, const size_t size)
{
    const std::filesystem::path aftermath = "aftermath";
    create_directory(aftermath);

    if (std::ofstream file(aftermath / name, std::ios::out | std::ios::binary); file)
    {
        file.write(reinterpret_cast<const char*>(data), size);
        file.close();
    }
}

void GpuCrashTracker::OnCrashDump(const void* pGpuCrashDump, const uint32_t gpuCrashDumpSize)
{
    std::lock_guard lock(m_mutex);

    WriteGpuCrashDumpToFile(pGpuCrashDump, gpuCrashDumpSize);
}

void GpuCrashTracker::OnShaderDebugInfo(const void* pShaderDebugInfo, const uint32_t shaderDebugInfoSize)
{
    std::lock_guard lock(m_mutex);

    GFSDK_Aftermath_ShaderDebugInfoIdentifier identifier = {};
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetShaderDebugInfoIdentifier(
        GFSDK_Aftermath_Version_API,
        pShaderDebugInfo,
        shaderDebugInfoSize,
        &identifier));

    std::vector data(static_cast<const uint8_t*>(pShaderDebugInfo),
                     static_cast<const uint8_t*>(pShaderDebugInfo) + shaderDebugInfoSize);
    m_shaderDebugInfo[identifier].swap(data);

    WriteShaderDebugInformationToFile(identifier, pShaderDebugInfo, shaderDebugInfoSize);
}

void GpuCrashTracker::OnDescription(PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription)
{
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationName, "SomeApp"); // todo: pass from c# side
    addDescription(GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationVersion, "v1.0"); // todo: pass from c# side
}

void GpuCrashTracker::OnResolveMarker(const void* pMarkerData, const uint32_t, void** ppResolvedMarkerData,
                                      uint32_t* pResolvedMarkerDataSize) const
{
    for (auto& map : m_markerMap)
    {
        const auto& foundMarker = map.find(reinterpret_cast<uint64_t>(pMarkerData));
        if (foundMarker != map.end())
        {
            const std::string& foundMarkerData = foundMarker->second;
            *ppResolvedMarkerData = const_cast<void*>(reinterpret_cast<const void*>(foundMarkerData.data()));
            *pResolvedMarkerDataSize = static_cast<uint32_t>(foundMarkerData.length());
            return;
        }
    }
}

void GpuCrashTracker::WriteGpuCrashDumpToFile(const void* pGpuCrashDump, const uint32_t gpuCrashDumpSize)
{
    GFSDK_Aftermath_GpuCrashDump_Decoder decoder = {};
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_CreateDecoder(
        GFSDK_Aftermath_Version_API,
        pGpuCrashDump,
        gpuCrashDumpSize,
        &decoder));

    GFSDK_Aftermath_GpuCrashDump_BaseInfo baseInfo = {};
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetBaseInfo(decoder, &baseInfo));

    uint32_t applicationNameLength = 0;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetDescriptionSize(
        decoder,
        GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationName,
        &applicationNameLength));

    std::vector<char> applicationName(applicationNameLength, '\0');

    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetDescription(
        decoder,
        GFSDK_Aftermath_GpuCrashDumpDescriptionKey_ApplicationName,
        static_cast<uint32_t>(applicationName.size()),
        applicationName.data()));

    static int count = 0;
    const std::string baseFileName =
        std::string(applicationName.data())
        + "-"
        + std::to_string(baseInfo.pid)
        + "-"
        + std::to_string(++count);

    const std::string crashDumpFileName = baseFileName + ".nv-gpudmp";
    WriteToAftermathFile(crashDumpFileName, static_cast<const std::byte*>(pGpuCrashDump), gpuCrashDumpSize);

    uint32_t jsonSize = 0;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GenerateJSON(
        decoder,
        GFSDK_Aftermath_GpuCrashDumpDecoderFlags_ALL_INFO,
        GFSDK_Aftermath_GpuCrashDumpFormatterFlags_NONE,
        ShaderDebugInfoLookupCallback,
        ShaderLookupCallback,
        ShaderSourceDebugInfoLookupCallback,
        this,
        &jsonSize));

    std::vector<char> json(jsonSize);
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_GetJSON(
        decoder,
        static_cast<uint32_t>(json.size()),
        json.data()));

    const std::string jsonFileName = crashDumpFileName + ".json";
    WriteToAftermathFile(jsonFileName, reinterpret_cast<const std::byte*>(json.data()), json.size() - 1);

    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GpuCrashDump_DestroyDecoder(decoder));
}

void GpuCrashTracker::WriteShaderDebugInformationToFile(
    GFSDK_Aftermath_ShaderDebugInfoIdentifier identifier,
    const void* pShaderDebugInfo,
    const uint32_t shaderDebugInfoSize) const
{
    const std::string name = "shader-" + std::to_string(identifier) + ".nvdbg";
    WriteToAftermathFile(name, static_cast<const std::byte*>(pShaderDebugInfo), shaderDebugInfoSize);
}

void GpuCrashTracker::OnShaderDebugInfoLookup(
    const GFSDK_Aftermath_ShaderDebugInfoIdentifier& identifier,
    PFN_GFSDK_Aftermath_SetData setShaderDebugInfo) const
{
    const auto iterator = m_shaderDebugInfo.find(identifier);
    if (iterator == m_shaderDebugInfo.end()) return;

    setShaderDebugInfo(iterator->second.data(), static_cast<uint32_t>(iterator->second.size()));
}

void GpuCrashTracker::OnShaderLookup(
    const GFSDK_Aftermath_ShaderBinaryHash& shaderHash,
    PFN_GFSDK_Aftermath_SetData setShaderBinary) const
{
    std::vector<uint8_t> shaderBinary;
    if (!m_shaderDatabase.FindShaderBinary(shaderHash, shaderBinary)) return;

    setShaderBinary(shaderBinary.data(), static_cast<uint32_t>(shaderBinary.size()));
}

void GpuCrashTracker::OnShaderSourceDebugInfoLookup(
    const GFSDK_Aftermath_ShaderDebugName& shaderDebugName,
    PFN_GFSDK_Aftermath_SetData setShaderBinary) const
{
    std::vector<uint8_t> sourceDebugInfo;
    if (!m_shaderDatabase.FindSourceShaderDebugData(shaderDebugName, sourceDebugInfo)) return;

    setShaderBinary(sourceDebugInfo.data(), static_cast<uint32_t>(sourceDebugInfo.size()));
}

void GpuCrashTracker::GpuCrashDumpCallback(
    const void* pGpuCrashDump,
    const uint32_t gpuCrashDumpSize,
    void* pUserData)
{
    auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnCrashDump(pGpuCrashDump, gpuCrashDumpSize);
}

void GpuCrashTracker::ShaderDebugInfoCallback(
    const void* pShaderDebugInfo,
    const uint32_t shaderDebugInfoSize,
    void* pUserData)
{
    auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderDebugInfo(pShaderDebugInfo, shaderDebugInfoSize);
}

void GpuCrashTracker::CrashDumpDescriptionCallback(
    PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription,
    void* pUserData)
{
    auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnDescription(addDescription);
}

void GpuCrashTracker::ResolveMarkerCallback(
    const void* pMarkerData,
    const uint32_t markerDataSize,
    void* pUserData,
    void** ppResolvedMarkerData,
    uint32_t* pResolvedMarkerDataSize)
{
    const auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnResolveMarker(pMarkerData, markerDataSize, ppResolvedMarkerData, pResolvedMarkerDataSize);
}

void GpuCrashTracker::ShaderDebugInfoLookupCallback(
    const GFSDK_Aftermath_ShaderDebugInfoIdentifier* pIdentifier,
    PFN_GFSDK_Aftermath_SetData setShaderDebugInfo,
    void* pUserData)
{
    const auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderDebugInfoLookup(*pIdentifier, setShaderDebugInfo);
}

void GpuCrashTracker::ShaderLookupCallback(
    const GFSDK_Aftermath_ShaderBinaryHash* pShaderHash,
    PFN_GFSDK_Aftermath_SetData setShaderBinary,
    void* pUserData)
{
    const auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderLookup(*pShaderHash, setShaderBinary);
}

void GpuCrashTracker::ShaderSourceDebugInfoLookupCallback(
    const GFSDK_Aftermath_ShaderDebugName* pShaderDebugName,
    PFN_GFSDK_Aftermath_SetData setShaderBinary,
    void* pUserData)
{
    const auto* pGpuCrashTracker = static_cast<GpuCrashTracker*>(pUserData);
    pGpuCrashTracker->OnShaderSourceDebugInfoLookup(*pShaderDebugName, setShaderBinary);
}
