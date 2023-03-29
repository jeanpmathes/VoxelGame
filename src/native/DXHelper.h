//  <copyright file="DXHelper.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include <stdexcept>
#include <iomanip>
#include <sstream>

#include "comdef.h"

using Microsoft::WRL::ComPtr;

inline std::string HResultToString(const HRESULT hr)
{
    std::stringstream code;
    code << std::hex << std::showbase << hr;

    return "Error: (HRESULT) " + code.str();
}

class HResultException final : public std::runtime_error
{
public:
    explicit HResultException(const HRESULT hr, const std::string info) : std::runtime_error(
                                                                              HResultToString(hr) + "\nInfo: " + info),
                                                                          m_hr(hr), m_info(info)
    {
    }

    [[nodiscard]] HRESULT Error() const { return m_hr; }
    [[nodiscard]] const char* Info() const { return m_info.c_str(); }

private:
    const HRESULT m_hr;
    const std::string m_info;
};

class NativeException final : public std::runtime_error
{
public:
    explicit NativeException(const std::string msg) : std::runtime_error(msg)
    {
    }
};

#define SAFE_RELEASE(p) if (p) (p)->Release()

#ifdef _DEBUG
constexpr bool IsDebugBuild = true;
#else
constexpr bool IsDebugBuild = false;
#endif

#define TRY_DO(expression) \
    do { \
        auto result = (expression); \
        std::string message; \
        if (IsDebugBuild) \
            message = "throwing from " #expression " at " __FILE__ ":" + std::to_string(__LINE__); \
        else \
            message = "throwing from " #expression; \
        ThrowIfFailed(result, message); \
    } while (false)

inline void ThrowIfFailed(const BOOL b, const std::string message)
{
    if (!b)
    {
        throw HResultException(HRESULT_FROM_WIN32(GetLastError()), message);
    }
}

inline void ThrowIfFailed(const HRESULT hr, const std::string message)
{
    if (FAILED(hr))
    {
        throw HResultException(hr, message);
    }
}

inline HRESULT ReadDataFromFile(const LPCWSTR filename, byte** data, UINT* size)
{
    using namespace Microsoft::WRL;

    CREATEFILE2_EXTENDED_PARAMETERS extendedParams;
    extendedParams.dwSize = sizeof(CREATEFILE2_EXTENDED_PARAMETERS);
    extendedParams.dwFileAttributes = FILE_ATTRIBUTE_NORMAL;
    extendedParams.dwFileFlags = FILE_FLAG_SEQUENTIAL_SCAN;
    extendedParams.dwSecurityQosFlags = SECURITY_ANONYMOUS;
    extendedParams.lpSecurityAttributes = nullptr;
    extendedParams.hTemplateFile = nullptr;

    const Wrappers::FileHandle file(
        CreateFile2(filename, GENERIC_READ, FILE_SHARE_READ, OPEN_EXISTING, &extendedParams));

    if (file.Get() == INVALID_HANDLE_VALUE)
    {
        throw std::exception();
    }

    FILE_STANDARD_INFO fileInfo = {};
    if (!GetFileInformationByHandleEx(file.Get(), FileStandardInfo, &fileInfo, sizeof(fileInfo)))
    {
        throw std::exception();
    }

    if (fileInfo.EndOfFile.HighPart != 0)
    {
        throw std::exception();
    }

    *data = static_cast<byte*>(malloc(fileInfo.EndOfFile.LowPart));
    *size = fileInfo.EndOfFile.LowPart;

    if (!ReadFile(file.Get(), *data, fileInfo.EndOfFile.LowPart, nullptr, nullptr))
    {
        throw std::exception();
    }

    return S_OK;
}

inline HRESULT ReadDataFromDDSFile(const LPCWSTR filename, byte** data, UINT* offset, UINT* size)
{
    if (FAILED(ReadDataFromFile(filename, data, size)))
    {
        return E_FAIL;
    }

    // DDS files always start with the same magic number.
    static constexpr UINT DDS_MAGIC = 0x20534444;
    const UINT magicNumber = *reinterpret_cast<const UINT*>(*data);
    if (magicNumber != DDS_MAGIC)
    {
        return E_FAIL;
    }

    struct DDSPixelformat
    {
        UINT size;
        UINT flags;
        UINT fourCC;
        UINT rgbBitCount;
        UINT rBitMask;
        UINT gBitMask;
        UINT bBitMask;
        UINT aBitMask;
    };

    struct DDSHeader
    {
        UINT size;
        UINT flags;
        UINT height;
        UINT width;
        UINT pitchOrLinearSize;
        UINT depth;
        UINT mipMapCount;
        UINT reserved1[11];
        DDSPixelformat ddsPixelFormat;
        UINT caps;
        UINT caps2;
        UINT caps3;
        UINT caps4;
        UINT reserved2;
    };

    const auto ddsHeader = reinterpret_cast<const DDSHeader*>(*data + sizeof(UINT));
    if (ddsHeader->size != sizeof(DDSHeader) || ddsHeader->ddsPixelFormat.size != sizeof(DDSPixelformat))
    {
        return E_FAIL;
    }

    constexpr ptrdiff_t ddsDataOffset = sizeof(UINT) + sizeof(DDSHeader);
    *offset = ddsDataOffset;
    *size = *size - ddsDataOffset;

    return S_OK;
}

// Assign a name to the object to aid with debugging.
#if defined(_DEBUG) || defined(DBG)
inline void SetName(ID3D12Object* pObject, const LPCWSTR name)
{
    pObject->SetName(name);
}

inline void SetNameIndexed(ID3D12Object* pObject, const LPCWSTR name, const UINT index)
{
    WCHAR fullName[50];
    if (swprintf_s(fullName, L"%s[%u]", name, index) > 0)
    {
        pObject->SetName(fullName);
    }
}
#else
inline void SetName(ID3D12Object*, LPCWSTR)
{
}
inline void SetNameIndexed(ID3D12Object*, LPCWSTR, UINT)
{
}
#endif

// Naming helper for ComPtr<T>.
// Assigns the name of the variable as the name of the object.
// The indexed variant will include the index in the name of the object.

// ReSharper disable CppInconsistentNaming
#define NAME_D3D12_OBJECT(x) SetName((x).Get(), L#x)
#define NAME_D3D12_OBJECT_INDEXED(x, n) SetNameIndexed((x)[n].Get(), L#x, n)
// ReSharper restore CppInconsistentNaming

inline UINT CalculateConstantBufferByteSize(UINT byteSize)
{
    // Constant buffer size is required to be aligned.
    return (byteSize + (D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1)) & ~(
        D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1);
}

#ifdef D3D_COMPILE_STANDARD_FILE_INCLUDE
inline ComPtr<ID3DBlob> CompileShader(
    const std::wstring& filename,
    const D3D_SHADER_MACRO* defines,
    const std::string& entrypoint,
    const std::string& target)
{
    UINT compileFlags;
#if defined(_DEBUG) || defined(DBG)
    compileFlags = D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION;
#else
    compileFlags = 0;
#endif

    ComPtr<ID3DBlob> byteCode = nullptr;
    ComPtr<ID3DBlob> errors;

    const HRESULT hr = D3DCompileFromFile(filename.c_str(), defines, D3D_COMPILE_STANDARD_FILE_INCLUDE,
                                          entrypoint.c_str(), target.c_str(), compileFlags, 0, &byteCode, &errors);

    if (errors != nullptr)
    {
        OutputDebugStringA(static_cast<char*>(errors->GetBufferPointer()));
    }
    TRY_DO(hr);

    return byteCode;
}
#endif

// Resets all elements in a ComPtr array.
template <class T>
void ResetComPtrArray(T* comPtrArray)
{
    for (auto& i : *comPtrArray)
    {
        i.Reset();
    }
}


// Resets all elements in a unique_ptr array.
template <class T>
void ResetUniquePtrArray(T* uniquePtrArray)
{
    for (auto& i : *uniquePtrArray)
    {
        i.reset();
    }
}
