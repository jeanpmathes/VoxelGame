// <copyright file="DXRHelper.hpp" company="NVIDIA Corp.">
//     Copyright 1998-2018 NVIDIA Corp. All Rights Reserved.
// </copyright>
// <author>NVIDIA Corp, jeanpmathes</author>

#pragma once

#include <fstream>
#include <sstream>
#include <string>
#include <vector>

#include <dxcapi.h>
#include <iostream>

#include "DXHelper.hpp"
#include "native.hpp"

/**
 * \brief Round a value up to the nearest multiple of an alignment.
 * \param value The value to round up.
 * \param alignment The alignment to round up to.
 * \return The rounded up value.
 */
template <typename T, typename V>
constexpr T RoundUp(T const value, V const alignment) { return (value + alignment - 1) & ~(alignment - 1); }

/**
 * \brief Compile a shader to a DXIL blob.
 * \param fileName The file name of the shader to compile.
 * \param entry The entry point of the shader.
 * \param target The target profile of the shader, e.g. "lib_6_3".
 * \param registry A function to register the shader with the application for debugging purposes.
 * \param errorCallback A function to call if the shader compilation fails.
 * \return The compiled shader blob.
 */
template <typename Registry>
ComPtr<IDxcBlob> CompileShader(
    LPCWSTR             fileName,
    std::wstring const& entry,
    std::wstring const& target,
    Registry            registry,
    NativeErrorFunc     errorCallback)
{
    static ComPtr<IDxcCompiler3>      compiler = nullptr;
    static ComPtr<IDxcUtils>          utils    = nullptr;
    static ComPtr<IDxcIncludeHandler> dxcIncludeHandler;

    if (!compiler)
    {
        TryDo(DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&compiler)));
        TryDo(DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&utils)));
        TryDo(utils->CreateDefaultIncludeHandler(&dxcIncludeHandler));
    }

    std::ifstream shaderFile(fileName);
    if (!shaderFile.good())
    {
        std::string errorMsg = "Failed to open shader file";
        errorCallback(E_FAIL, errorMsg.c_str());
        return nullptr;
    }

    std::stringstream shaderStream;
    shaderStream << shaderFile.rdbuf();
    std::string shader = shaderStream.str();

    ComPtr<IDxcBlobEncoding> shaderSourceBlob;
    TryDo(utils->CreateBlobFromPinned(shader.c_str(), static_cast<UINT32>(shader.size()), CP_UTF8, &shaderSourceBlob));

    DxcBuffer sourceBuffer = {
        .Ptr = shaderSourceBlob->GetBufferPointer(),
        .Size = shaderSourceBlob->GetBufferSize(),
        .Encoding = DXC_CP_UTF8
    };

    std::vector<LPCWSTR>   args;
    std::vector<DxcDefine> defines;

#if defined(NATIVE_DEBUG) || defined(USE_NSIGHT_AFTERMATH)
    args.push_back(DXC_ARG_WARNINGS_ARE_ERRORS);
    args.push_back(DXC_ARG_DEBUG);
    args.push_back(L"-Qembed_debug");
#else
    args.push_back(DXC_ARG_OPTIMIZATION_LEVEL3);
#endif

    ComPtr<IDxcCompilerArgs> compilerArgs;
    TryDo(
        utils->BuildArguments(
            fileName,
            entry.c_str(),
            target.c_str(),
            args.data(),
            static_cast<UINT32>(args.size()),
            defines.data(),
            static_cast<UINT32>(defines.size()),
            &compilerArgs));

    ComPtr<IDxcResult> result;
    TryDo(
        compiler->Compile(
            &sourceBuffer,
            compilerArgs->GetArguments(),
            compilerArgs->GetCount(),
            dxcIncludeHandler.Get(),
            IID_PPV_ARGS(&result)));

    HRESULT resultCode;
    TryDo(result->GetStatus(&resultCode));

    if (FAILED(resultCode))
    {
        ComPtr<IDxcBlobUtf8> error;
        TryDo(result->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&error), nullptr));
        std::vector<char> infoLog(error->GetBufferSize());
        memcpy(infoLog.data(), error->GetBufferPointer(), error->GetBufferSize());

        std::string errorMsg = "Shader Compilation Error:\n";
        errorMsg.append(infoLog.data());

        errorCallback(resultCode, errorMsg.c_str());
        return nullptr;
    }

    registry(result);

    ComPtr<IDxcBlob> blob;
    TryDo(result->GetResult(&blob));

    return blob;
}
