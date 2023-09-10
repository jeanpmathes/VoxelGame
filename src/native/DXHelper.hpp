//  <copyright file="DXHelper.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include <stdexcept>
#include <iomanip>
#include <sstream>

// ReSharper disable once CppUnusedIncludeDirective
#include <wrl.h>

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

#if defined(VG_DEBUG)
constexpr bool IS_DEBUG_BUILD = true;
#else
constexpr bool IS_DEBUG_BUILD = false;
#endif

#define REQUIRE(expression) \
    do { \
        if (!(expression)) \
        { \
            if (!IS_DEBUG_BUILD) break; \
            std::string message = "failed requirement '" #expression "' at " __FILE__ ":" + std::to_string(__LINE__); \
            throw NativeException(message); \
        } \
    } while (false)

#define TRY_DO(expression) \
    do { \
        auto result = (expression); \
        std::string errorMessage; \
        if (IS_DEBUG_BUILD) \
            errorMessage = "throwing from " #expression " at " __FILE__ ":" + std::to_string(__LINE__); \
        else \
            errorMessage = "throwing from " #expression; \
        ThrowIfFailed(result, errorMessage); \
    } while (false)

#define CHECK_RETURN(value) \
    do { \
        std::string errorMessage; \
        if (IS_DEBUG_BUILD) \
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

inline std::wstring GetNameIndexed(const LPCWSTR name, const UINT index)
{
    std::wstringstream ss;

    ss << name;
    ss << "[";
    ss << std::to_wstring(index);
    ss << "]";

    return ss.str();
}

inline void SetName(const ComPtr<ID3D12Object>& object, const LPCWSTR name)
{
    TRY_DO(object->SetName(name));
}

// Naming helper for ComPtr<T>.
// Assigns the name of the variable as the name of the object.
// The indexed variant will include the index in the name of the object.

// ReSharper disable CppInconsistentNaming
#define NAME_D3D12_OBJECT(object) \
    do { \
        if (!IS_DEBUG_BUILD) break; \
        SetName((object), L#object); \
    } while (false)

#define NAME_D3D12_OBJECT_INDEXED(object, index) \
    do { \
        if (!IS_DEBUG_BUILD) break; \
        SetName((object)[index], GetNameIndexed(L#object, index).c_str()); \
    } while (false)
// ReSharper restore CppInconsistentNaming

inline UINT CalculateConstantBufferByteSize(UINT byteSize)
{
    // Constant buffer size is required to be aligned.
    return (byteSize + (D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1)) & ~(
        D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1);
}

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