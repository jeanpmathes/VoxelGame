/******************************************************************************
 * Copyright 1998-2018 NVIDIA Corp. All Rights Reserved.
 *****************************************************************************/

#pragma once

#include <fstream>
#include <sstream>
#include <string>
#include <d3d12.h>
#include <dxcapi.h>

#include "DXHelper.h"

#include <vector>

namespace nv_helpers_dx12
{
    //--------------------------------------------------------------------------------------------------
    //
    //
    inline ComPtr<ID3D12Resource> CreateBuffer(ID3D12Device* m_device, uint64_t size,
                                               D3D12_RESOURCE_FLAGS flags, D3D12_RESOURCE_STATES initState,
                                               const D3D12_HEAP_PROPERTIES& heapProps)
    {
        D3D12_RESOURCE_DESC bufDesc = {};
        bufDesc.Alignment = 0;
        bufDesc.DepthOrArraySize = 1;
        bufDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        bufDesc.Flags = flags;
        bufDesc.Format = DXGI_FORMAT_UNKNOWN;
        bufDesc.Height = 1;
        bufDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        bufDesc.MipLevels = 1;
        bufDesc.SampleDesc.Count = 1;
        bufDesc.SampleDesc.Quality = 0;
        bufDesc.Width = size;

        ComPtr<ID3D12Resource> pBuffer;
        TRY_DO(m_device->CreateCommittedResource(&heapProps, D3D12_HEAP_FLAG_NONE, &bufDesc,
            initState, nullptr, IID_PPV_ARGS(&pBuffer)));
        return pBuffer;
    }

#ifndef ROUND_UP
#define ROUND_UP(v, powerOf2Alignment) (((v) + (powerOf2Alignment)-1) & ~((powerOf2Alignment)-1))
#endif

    // Specifies a heap used for uploading. This heap type has CPU access optimized
    // for uploading to the GPU.
    static const D3D12_HEAP_PROPERTIES kUploadHeapProps = {
        D3D12_HEAP_TYPE_UPLOAD, D3D12_CPU_PAGE_PROPERTY_UNKNOWN, D3D12_MEMORY_POOL_UNKNOWN, 0, 0
    };

    // Specifies the default heap. This heap type experiences the most bandwidth for
    // the GPU, but cannot provide CPU access.
    static const D3D12_HEAP_PROPERTIES kDefaultHeapProps = {
        D3D12_HEAP_TYPE_DEFAULT, D3D12_CPU_PAGE_PROPERTY_UNKNOWN, D3D12_MEMORY_POOL_UNKNOWN, 0, 0
    };

    //--------------------------------------------------------------------------------------------------
    // Compile a HLSL file into a DXIL library
    //
    inline ComPtr<IDxcBlob> CompileShaderLibrary(LPCWSTR fileName)
    {
        static ComPtr<IDxcCompiler> pCompiler = nullptr;
        static ComPtr<IDxcLibrary> pLibrary = nullptr;
        static ComPtr<IDxcIncludeHandler> dxcIncludeHandler;

        HRESULT hr;

        // Initialize the DXC compiler and compiler helper
        if (!pCompiler)
        {
            TRY_DO(DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&pCompiler)));
            TRY_DO(DxcCreateInstance(CLSID_DxcLibrary, IID_PPV_ARGS(&pLibrary)));
            TRY_DO(pLibrary->CreateIncludeHandler(&dxcIncludeHandler));
        }
        // Open and read the file
        std::ifstream shaderFile(fileName);
        if (shaderFile.good() == false)
        {
            throw std::logic_error("Cannot find shader file");
        }
        std::stringstream strStream;
        strStream << shaderFile.rdbuf();
        std::string sShader = strStream.str();

        // Create blob from the string
        ComPtr<IDxcBlobEncoding> pTextBlob;
        TRY_DO(pLibrary->CreateBlobWithEncodingFromPinned(
            sShader.c_str(), static_cast<uint32_t>(sShader.size()), 0, &pTextBlob));

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
                throw std::logic_error("Failed to get shader compiler error");
            }

            // Convert error blob to a string
            std::vector<char> infoLog(pError->GetBufferSize() + 1);
            memcpy(infoLog.data(), pError->GetBufferPointer(), pError->GetBufferSize());
            infoLog[pError->GetBufferSize()] = 0;

            std::string errorMsg = "Shader Compiler Error:\n";
            errorMsg.append(infoLog.data());

            MessageBoxA(nullptr, errorMsg.c_str(), "Error!", MB_OK);
            throw std::logic_error("Failed compile shader");
        }

        ComPtr<IDxcBlob> pBlob;
        TRY_DO(pResult->GetResult(&pBlob));
        return pBlob;
    }

    //--------------------------------------------------------------------------------------------------
    //
    //
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
} // namespace nv_helpers_dx12