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

#define REQUIRE(expression) \
    do { \
        if (!(expression)) \
        { \
            std::string message; \
            if (IsDebugBuild) \
                message = "failed requirement '" #expression "' at " __FILE__ ":" + std::to_string(__LINE__); \
            else \
                message = "failed requirement '" #expression "'"; \
            throw NativeException(message); \
        } \
    } while (false)

#define TRY_DO(expression) \
    do { \
        auto result = (expression); \
        std::string errorMessage; \
        if (IsDebugBuild) \
            errorMessage = "throwing from " #expression " at " __FILE__ ":" + std::to_string(__LINE__); \
        else \
            errorMessage = "throwing from " #expression; \
        ThrowIfFailed(result, errorMessage); \
    } while (false)

#define CHECK_RETURN(value) \
    do { \
        std::string errorMessage; \
        if (IsDebugBuild) \
            errorMessage = "error with " #value " at " __FILE__ ":" + std::to_string(__LINE__); \
        else \
            errorMessage = "error with " #value; \
        BOOL ok = (value) != NULL; \
        ThrowIfFailed(ok, errorMessage); \
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
