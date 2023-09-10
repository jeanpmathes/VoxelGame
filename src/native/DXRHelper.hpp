// <copyright file="DXRHelper.hpp" company="NVIDIA Corp.">
//     Copyright 1998-2018 NVIDIA Corp. All Rights Reserved.
// </copyright>
// <author>NVIDIA Corp, jeanpmathes</author>

#pragma once

#include <fstream>
#include <sstream>
#include <string>
#include <vector>
#include <functional>

#include <d3d12.h>
#include <dxcapi.h>

#include "DXHelper.hpp"

#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment) (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

// Compile a HLSL file into a DXIL library
inline ComPtr<IDxcBlob> CompileShaderLibrary(LPCWSTR fileName, std::function<void(const char*)> errorCallback)
{
    static ComPtr<IDxcCompiler> pCompiler = nullptr;
    static ComPtr<IDxcUtils> pUtils = nullptr;
    static ComPtr<IDxcIncludeHandler> dxcIncludeHandler;

    HRESULT hr;

    // Initialize the DXC compiler and compiler helper
    if (!pCompiler)
    {
        TRY_DO(DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&pCompiler)));
        TRY_DO(DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&pUtils)));
        TRY_DO(pUtils->CreateDefaultIncludeHandler(&dxcIncludeHandler));
    }

    // Open and read the file
    std::ifstream shaderFile(fileName);
    if (not shaderFile.good())
    {
        std::string errorMsg = "Failed to open shader file";
        errorCallback(errorMsg.c_str());
        return nullptr;
    }

    std::stringstream strStream;
    strStream << shaderFile.rdbuf();
    std::string sShader = strStream.str();

    // Create blob from the string
    ComPtr<IDxcBlobEncoding> pTextBlob;
    TRY_DO(pUtils->CreateBlobFromPinned(sShader.c_str(), static_cast<UINT32>(sShader.size()), CP_UTF8, &pTextBlob));

    std::vector<LPCWSTR> args;
    std::vector<DxcDefine> defines;

#if defined(VG_DEBUG)
    args.push_back(DXC_ARG_WARNINGS_ARE_ERRORS);
    args.push_back(DXC_ARG_DEBUG);
#endif

    // Compile
    ComPtr<IDxcOperationResult> pResult;
    TRY_DO(pCompiler->Compile(pTextBlob.Get(), fileName, L"", L"lib_6_3",
        args.data(), static_cast<UINT32>(args.size()),
        defines.data(), static_cast<UINT32>(defines.size()),
        dxcIncludeHandler.Get(), &pResult));

    // Verify the result
    HRESULT resultCode;
    TRY_DO(pResult->GetStatus(&resultCode));
    if (FAILED(resultCode))
    {
        IDxcBlobEncoding* pError;
        hr = pResult->GetErrorBuffer(&pError);
        if (FAILED(hr))
        {
            throw NativeException("Failed to get shader compiler error.");
        }

        // Convert error blob to a string
        std::vector<char> infoLog(pError->GetBufferSize() + 1);
        memcpy(infoLog.data(), pError->GetBufferPointer(), pError->GetBufferSize());
        infoLog[pError->GetBufferSize()] = 0;

        std::string errorMsg = "Shader Compilation Error:\n";
        errorMsg.append(infoLog.data());

        errorCallback(errorMsg.c_str());
        return nullptr;
    }

    ComPtr<IDxcBlob> pBlob;
    TRY_DO(pResult->GetResult(&pBlob));
    return pBlob;
}