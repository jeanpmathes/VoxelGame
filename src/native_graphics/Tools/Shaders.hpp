// <copyright file="Shaders.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <fstream>
#include <sstream>
#include <string>
#include <vector>

#include <dxcapi.h>

#include "Basic.hpp"

#include "native.hpp"

template <typename T>
std::vector<T> ReadBlob(ComPtr<ID3DBlob> const& blob)
{
    return std::vector<T>(static_cast<T*>(blob->GetBufferPointer()), static_cast<T*>(blob->GetBufferPointer()) + blob->GetBufferSize() / sizeof(T));
}

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
ComPtr<IDxcBlob> CompileShader(LPCWSTR fileName, std::wstring const& entry, std::wstring const& target, Registry registry, NativeErrorFunction errorCallback)
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
        std::string errorMessage = "Failed to open shader file";
        errorCallback(E_FAIL, errorMessage.c_str());
        return nullptr;
    }

    std::stringstream shaderStream;
    shaderStream << shaderFile.rdbuf();
    std::string shader = shaderStream.str();

    ComPtr<IDxcBlobEncoding> shaderSourceBlob;
    TryDo(utils->CreateBlobFromPinned(shader.c_str(), static_cast<UINT32>(shader.size()), CP_UTF8, &shaderSourceBlob));

    DxcBuffer sourceBuffer = {.Ptr = shaderSourceBlob->GetBufferPointer(), .Size = shaderSourceBlob->GetBufferSize(), .Encoding = DXC_CP_UTF8};

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
    TryDo(compiler->Compile(&sourceBuffer, compilerArgs->GetArguments(), compilerArgs->GetCount(), dxcIncludeHandler.Get(), IID_PPV_ARGS(&result)));

    HRESULT resultCode;
    TryDo(result->GetStatus(&resultCode));

    if (FAILED(resultCode))
    {
        ComPtr<IDxcBlobUtf8> error;
        TryDo(result->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&error), nullptr));
        std::vector<char> infoLog(error->GetBufferSize());
        memcpy(infoLog.data(), error->GetBufferPointer(), error->GetBufferSize());

        std::string errorMessage = "Shader Compilation Error:\n";
        errorMessage.append(infoLog.data());

        errorCallback(resultCode, errorMessage.c_str());
        return nullptr;
    }

    registry(result);

    ComPtr<IDxcBlob> blob;
    TryDo(result->GetResult(&blob));

    return blob;
}
