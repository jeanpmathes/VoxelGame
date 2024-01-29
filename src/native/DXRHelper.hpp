// <copyright file="DXRHelper.hpp" company="NVIDIA Corp.">
//     Copyright 1998-2018 NVIDIA Corp. All Rights Reserved.
// </copyright>
// <author>NVIDIA Corp, jeanpmathes</author>

#pragma once

#include <fstream>
#include <sstream>
#include <string>
#include <vector>

#include <d3d12.h>
#include <dxcapi.h>

#include "DXHelper.hpp"
#include "native.hpp"

#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment) (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

// Compile a HLSL file into a DXIL library.
inline ComPtr<IDxcBlob> CompileShader(
    LPCWSTR fileName,
    const std::wstring& entry, const std::wstring& target,
    std::function<void(ComPtr<IDxcResult>)> registry,
    NativeErrorFunc errorCallback)
{
    static ComPtr<IDxcCompiler3> compiler = nullptr;
    static ComPtr<IDxcUtils> utils = nullptr;
    static ComPtr<IDxcIncludeHandler> dxcIncludeHandler;
    
    if (!compiler)
    {
        TRY_DO(DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&compiler)));
        TRY_DO(DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&utils)));
        TRY_DO(utils->CreateDefaultIncludeHandler(&dxcIncludeHandler));
    }
    
    std::ifstream shaderFile(fileName);
    if (not shaderFile.good())
    {
        std::string errorMsg = "Failed to open shader file";
        errorCallback(E_FAIL, errorMsg.c_str());
        return nullptr;
    }

    std::stringstream shaderStream;
    shaderStream << shaderFile.rdbuf();
    std::string shader = shaderStream.str();

    ComPtr<IDxcBlobEncoding> shaderSourceBlob;
    TRY_DO(utils->CreateBlobFromPinned(shader.c_str(), static_cast<UINT32>(shader.size()), CP_UTF8, &shaderSourceBlob));

    DxcBuffer sourceBuffer = {
        .Ptr = shaderSourceBlob->GetBufferPointer(),
        .Size = shaderSourceBlob->GetBufferSize(),
        .Encoding = DXC_CP_UTF8
    };

    std::vector<LPCWSTR> args;
    std::vector<DxcDefine> defines;

#if defined(NATIVE_DEBUG) || defined(USE_NSIGHT_AFTERMATH)
    args.push_back(DXC_ARG_WARNINGS_ARE_ERRORS);
    args.push_back(DXC_ARG_DEBUG);
    args.push_back(L"-Qembed_debug");
#else
    args.push_back(DXC_ARG_OPTIMIZATION_LEVEL3);
#endif

    ComPtr<IDxcCompilerArgs> compilerArgs;
    TRY_DO(utils->BuildArguments(
        fileName, entry.c_str(), target.c_str(),
        args.data(), static_cast<UINT32>(args.size()),
        defines.data(), static_cast<UINT32>(defines.size()),
        &compilerArgs));

    ComPtr<IDxcResult> result;
    TRY_DO(compiler->Compile(
        &sourceBuffer,
        compilerArgs->GetArguments(), compilerArgs->GetCount(),
        dxcIncludeHandler.Get(),
        IID_PPV_ARGS(&result)));

    HRESULT resultCode;
    TRY_DO(result->GetStatus(&resultCode));
    if (FAILED(resultCode))
    {
        ComPtr<IDxcBlobUtf8> error;
        TRY_DO(result->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&error), nullptr));
        std::vector<char> infoLog(error->GetBufferSize());
        memcpy(infoLog.data(), error->GetBufferPointer(), error->GetBufferSize());

        std::string errorMsg = "Shader Compilation Error:\n";
        errorMsg.append(infoLog.data());

        errorCallback(resultCode, errorMsg.c_str());
        return nullptr;
    }

    registry(result);

    ComPtr<IDxcBlob> blob;
    TRY_DO(result->GetResult(&blob));

    return blob;
}
