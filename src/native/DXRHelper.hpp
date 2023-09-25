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

// Compile a HLSL file into a DXIL library.
inline ComPtr<IDxcBlob> CompileShaderLibrary(LPCWSTR fileName, std::function<void(const char*)> errorCallback)
{
    static ComPtr<IDxcCompiler> compiler = nullptr;
    static ComPtr<IDxcUtils> utils = nullptr;
    static ComPtr<IDxcIncludeHandler> dxcIncludeHandler;

    // Initialize the DXC compiler and compiler helper.
    if (!compiler)
    {
        TRY_DO(DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&compiler)));
        TRY_DO(DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&utils)));
        TRY_DO(utils->CreateDefaultIncludeHandler(&dxcIncludeHandler));
    }

    // Open and read the file.
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
    ComPtr<IDxcBlobEncoding> textBlob;
    TRY_DO(utils->CreateBlobFromPinned(sShader.c_str(), static_cast<UINT32>(sShader.size()), CP_UTF8, &textBlob));

    std::vector<LPCWSTR> args;
    std::vector<DxcDefine> defines;

#if defined(VG_DEBUG)
    args.push_back(DXC_ARG_WARNINGS_ARE_ERRORS);
    args.push_back(DXC_ARG_DEBUG);
    args.push_back(L"-Qembed_debug");
#endif
    // todo: try passing optimization 3 as argument when not in debug mode

    // Compile.
    ComPtr<IDxcOperationResult> result;
    TRY_DO(compiler->Compile(textBlob.Get(), fileName, L"", L"lib_6_7",
        args.data(), static_cast<UINT32>(args.size()),
        defines.data(), static_cast<UINT32>(defines.size()),
        dxcIncludeHandler.Get(), &result));

    // Verify the result.
    HRESULT resultCode;
    TRY_DO(result->GetStatus(&resultCode));
    if (FAILED(resultCode))
    {
        IDxcBlobEncoding* error;
        TRY_DO(result->GetErrorBuffer(&error));

        // Convert error blob to a string.
        std::vector<char> infoLog(error->GetBufferSize() + 1);
        memcpy(infoLog.data(), error->GetBufferPointer(), error->GetBufferSize());
        infoLog[error->GetBufferSize()] = 0;

        std::string errorMsg = "Shader Compilation Error:\n";
        errorMsg.append(infoLog.data());

        errorCallback(errorMsg.c_str());
        return nullptr;
    }

    ComPtr<IDxcBlob> blob;
    TRY_DO(result->GetResult(&blob));

    return blob;
}
