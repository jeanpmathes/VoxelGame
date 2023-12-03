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

#pragma once

#include <map>
#include <mutex>

#include "NsightAftermathHelpers.hpp"
#include "NsightAftermathShaderDatabase.hpp"

/**
 * \brief Implements GPU crash dump tracking using the Nsight Aftermath API.
 */
class GpuCrashTracker
{
public:
    constexpr static UINT MARKER_FRAME_HISTORY = 4;
    using MarkerMap = std::array<std::map<uint64_t, std::string>, MARKER_FRAME_HISTORY>;

    GpuCrashTracker(const MarkerMap& markerMap, const ShaderDatabase& shaderDatabase);
    ~GpuCrashTracker();

    GpuCrashTracker(const GpuCrashTracker&) = delete;
    GpuCrashTracker& operator=(const GpuCrashTracker&) = delete;

    GpuCrashTracker(GpuCrashTracker&&) = delete;
    GpuCrashTracker& operator=(GpuCrashTracker&&) = delete;

    /**
     * \brief Initialize the GPU crash dump tracker.
     */
    void Initialize();

    /**
     * \brief Write binary data to a file in the aftermath directory.
     * \param name The name of the file to write.
     * \param data The data to write.
     * \param size The size of the data to write, in bytes.
     */
    static void WriteToAftermathFile(const std::string& name, const std::byte* data, size_t size);

private:
    void OnCrashDump(const void* pGpuCrashDump, uint32_t gpuCrashDumpSize);
    void OnShaderDebugInfo(const void* pShaderDebugInfo, uint32_t shaderDebugInfoSize);
    void OnDescription(PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription);
    void OnResolveMarker(const void* pMarkerData, uint32_t markerDataSize, void** ppResolvedMarkerData,
                         uint32_t* pResolvedMarkerDataSize) const;

    void WriteGpuCrashDumpToFile(const void* pGpuCrashDump, uint32_t gpuCrashDumpSize);
    void WriteShaderDebugInformationToFile(
        GFSDK_Aftermath_ShaderDebugInfoIdentifier identifier,
        const void* pShaderDebugInfo,
        uint32_t shaderDebugInfoSize) const;

    void OnShaderDebugInfoLookup(
        const GFSDK_Aftermath_ShaderDebugInfoIdentifier& identifier,
        PFN_GFSDK_Aftermath_SetData setShaderDebugInfo) const;
    void OnShaderLookup(
        const GFSDK_Aftermath_ShaderBinaryHash& shaderHash,
        PFN_GFSDK_Aftermath_SetData setShaderBinary) const;
    void OnShaderSourceDebugInfoLookup(
        const GFSDK_Aftermath_ShaderDebugName& shaderDebugName,
        PFN_GFSDK_Aftermath_SetData setShaderBinary) const;

    static void GpuCrashDumpCallback(
        const void* pGpuCrashDump,
        uint32_t gpuCrashDumpSize,
        void* pUserData);
    static void ShaderDebugInfoCallback(
        const void* pShaderDebugInfo,
        uint32_t shaderDebugInfoSize,
        void* pUserData);
    static void CrashDumpDescriptionCallback(
        PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription,
        void* pUserData);
    static void ResolveMarkerCallback(
        const void* pMarkerData,
        uint32_t markerDataSize,
        void* pUserData,
        void** ppResolvedMarkerData,
        uint32_t* pResolvedMarkerDataSize
    );

    static void ShaderDebugInfoLookupCallback(
        const GFSDK_Aftermath_ShaderDebugInfoIdentifier* pIdentifier,
        PFN_GFSDK_Aftermath_SetData setShaderDebugInfo,
        void* pUserData);
    static void ShaderLookupCallback(
        const GFSDK_Aftermath_ShaderBinaryHash* pShaderHash,
        PFN_GFSDK_Aftermath_SetData setShaderBinary,
        void* pUserData);
    static void ShaderSourceDebugInfoLookupCallback(
        const GFSDK_Aftermath_ShaderDebugName* pShaderDebugName,
        PFN_GFSDK_Aftermath_SetData setShaderBinary,
        void* pUserData);

    bool m_initialized;
    mutable std::mutex m_mutex;
    std::map<GFSDK_Aftermath_ShaderDebugInfoIdentifier, std::vector<uint8_t>> m_shaderDebugInfo = {};
    const MarkerMap& m_markerMap;
    const ShaderDatabase& m_shaderDatabase;
};
