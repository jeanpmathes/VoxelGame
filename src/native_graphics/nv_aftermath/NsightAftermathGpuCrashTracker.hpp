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
    using MarkerMap                            = std::array<std::map<uint64_t, std::string>, MARKER_FRAME_HISTORY>;

    struct Description
    {
        std::string applicationName;
        std::string applicationVersion;

        static Description Create(LPWSTR applicationName, LPWSTR applicationVersion);
    };

    GpuCrashTracker(MarkerMap const& markerMap, ShaderDatabase const& shaderDatabase, Description description);
    ~GpuCrashTracker();

    GpuCrashTracker(GpuCrashTracker const&)            = delete;
    GpuCrashTracker& operator=(GpuCrashTracker const&) = delete;

    GpuCrashTracker(GpuCrashTracker&&)            = delete;
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
    static void WriteToAftermathFile(std::string const& name, std::byte const* data, size_t size);

private:
    void OnCrashDump(void const* pGpuCrashDump, uint32_t gpuCrashDumpSize);
    void OnShaderDebugInfo(void const* pShaderDebugInfo, uint32_t shaderDebugInfoSize);
    void OnDescription(PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription) const;
    void OnResolveMarker(
        void const* pMarkerData,
        uint32_t    markerDataSize,
        void**      ppResolvedMarkerData,
        uint32_t*   pResolvedMarkerDataSize) const;

    void WriteGpuCrashDumpToFile(void const* pGpuCrashDump, uint32_t gpuCrashDumpSize);
    void WriteShaderDebugInformationToFile(
        GFSDK_Aftermath_ShaderDebugInfoIdentifier identifier,
        void const*                               pShaderDebugInfo,
        uint32_t                                  shaderDebugInfoSize) const;

    void OnShaderDebugInfoLookup(
        GFSDK_Aftermath_ShaderDebugInfoIdentifier const& identifier,
        PFN_GFSDK_Aftermath_SetData                      setShaderDebugInfo) const;
    void OnShaderLookup(
        GFSDK_Aftermath_ShaderBinaryHash const& shaderHash,
        PFN_GFSDK_Aftermath_SetData             setShaderBinary) const;
    void OnShaderSourceDebugInfoLookup(
        GFSDK_Aftermath_ShaderDebugName const& shaderDebugName,
        PFN_GFSDK_Aftermath_SetData            setShaderBinary) const;

    static void GpuCrashDumpCallback(void const* pGpuCrashDump, uint32_t gpuCrashDumpSize, void* pUserData);
    static void ShaderDebugInfoCallback(void const* pShaderDebugInfo, uint32_t shaderDebugInfoSize, void* pUserData);
    static void CrashDumpDescriptionCallback(
        PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addDescription,
        void*                                          pUserData);
    static void ResolveMarkerCallback(
        void const* pMarkerData,
        uint32_t    markerDataSize,
        void*       pUserData,
        void**      ppResolvedMarkerData,
        uint32_t*   pResolvedMarkerDataSize);

    static void ShaderDebugInfoLookupCallback(
        GFSDK_Aftermath_ShaderDebugInfoIdentifier const* pIdentifier,
        PFN_GFSDK_Aftermath_SetData                      setShaderDebugInfo,
        void*                                            pUserData);
    static void ShaderLookupCallback(
        GFSDK_Aftermath_ShaderBinaryHash const* pShaderHash,
        PFN_GFSDK_Aftermath_SetData             setShaderBinary,
        void*                                   pUserData);
    static void ShaderSourceDebugInfoLookupCallback(
        GFSDK_Aftermath_ShaderDebugName const* pShaderDebugName,
        PFN_GFSDK_Aftermath_SetData            setShaderBinary,
        void*                                  pUserData);

    bool                                                                      m_initialized;
    mutable std::mutex                                                        m_mutex;
    std::map<GFSDK_Aftermath_ShaderDebugInfoIdentifier, std::vector<uint8_t>> m_shaderDebugInfo = {};
    MarkerMap const&                                                          m_markerMap;
    ShaderDatabase const&                                                     m_shaderDatabase;

    Description const m_description;
};
