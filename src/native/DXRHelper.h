// <copyright file="DXRHelper.h" company="NVIDIA Corp.">
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

#include "DXHelper.h"

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

    // Compile
    ComPtr<IDxcOperationResult> pResult;
    TRY_DO(pCompiler->Compile(pTextBlob.Get(), fileName, L"", L"lib_6_3", nullptr, 0, nullptr, 0,
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

inline ComPtr<ID3D12DescriptorHeap> CreateDescriptorHeap(ID3D12Device* device, uint32_t count,
                                                         D3D12_DESCRIPTOR_HEAP_TYPE type, bool shaderVisible)
{
    D3D12_DESCRIPTOR_HEAP_DESC desc = {};
    desc.NumDescriptors = count;
    desc.Type = type;
    desc.Flags =
        shaderVisible ? D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE : D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

    ComPtr<ID3D12DescriptorHeap> pHeap;
    TRY_DO(device->CreateDescriptorHeap(&desc, IID_PPV_ARGS(&pHeap)));
    return pHeap;
}
