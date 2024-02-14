//  <copyright file="DXHelper.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include <format>
#include <source_location>
#include <sstream>
#include <stdexcept>
#include <vector>

// ReSharper disable once CppUnusedIncludeDirective
#include <wrl.h>

using Microsoft::WRL::ComPtr;

inline std::string HResultToString(HRESULT const hr)
{
    std::stringstream code;
    code << std::hex << std::showbase << hr;

    return "Error: (HRESULT) " + code.str();
}

class HResultException final : public std::runtime_error
{
public:
    explicit HResultException(HRESULT const hr, std::string const& info)
        : std::runtime_error(HResultToString(hr) + "\nInfo: " + info)
      , m_hr(hr)
      , m_info(info)
    {
    }

    [[nodiscard]] HRESULT     Error() const { return m_hr; }
    [[nodiscard]] char const* Info() const { return m_info.c_str(); }

private:
    HRESULT     m_hr;
    std::string m_info;
};

class NativeException final : public std::runtime_error
{
public:
    using std::runtime_error::runtime_error;
};

#if defined(NATIVE_DEBUG)
constexpr bool IS_DEBUG_BUILD = true;
#else
constexpr bool IS_DEBUG_BUILD = false;
#endif

constexpr bool Implies(bool const a, bool const b) { return !a || b; }

/**
 * \brief Assert that a condition is true. 
 */
constexpr void Require(bool const condition, std::source_location const& location = std::source_location::current())
{
    if constexpr (!IS_DEBUG_BUILD) return;

    if (!condition)
    {
        std::string const message = std::format(
            "failed requirement in function {} at {}:{}:{}",
            location.function_name(),
            location.file_name(),
            location.line(),
            location.column());

        if (IsDebuggerPresent()) DebugBreak();
        throw NativeException(message);
    }
}

inline std::string GetTryDoMessage(std::source_location const& location)
{
    if constexpr (IS_DEBUG_BUILD)
        return std::format(
            "throwing from function {} at {}:{}:{}",
            location.function_name(),
            location.file_name(),
            location.line(),
            location.column());
    else return std::format("throwing from function {}", location.function_name());
}

/**
 * \brief Try to do something, e.g. a Win32 API call, and throw an exception if it fails.
 */
inline void TryDo(BOOL const b, std::source_location const& location = std::source_location::current())
{
    if (b) return;

    std::string const message = GetTryDoMessage(location);

    if (IsDebuggerPresent()) DebugBreak();
    throw HResultException(HRESULT_FROM_WIN32(GetLastError()), message);
}

/**
 * \brief Try to do something, e.g. a DirectX API call, and throw an exception if it fails.
 */
inline void TryDo(HRESULT const hr, std::source_location const& location = std::source_location::current())
{
    if (SUCCEEDED(hr)) return;

    std::string const message = GetTryDoMessage(location);

    if (IsDebuggerPresent()) DebugBreak();
    throw HResultException(hr, message);
}

/**
 * \brief Check that the return value of a function is not NULL, and throw an exception based on GetLastError if it is.
 */
template <typename T>
constexpr T const& CheckReturn(T const& value, std::source_location const& location = std::source_location::current())
{
    if (value != NULL) return value;

    std::string const message = std::format(
        "error with value of type '{}' in function {} at {}:{}:{}",
        typeid(T).name(),
        location.function_name(),
        location.file_name(),
        location.line(),
        location.column());

    if (IsDebuggerPresent()) DebugBreak();
    throw HResultException(HRESULT_FROM_WIN32(GetLastError()), message);
}

inline std::wstring GetNameIndexed(LPCWSTR const name, UINT const index)
{
    std::wstringstream ss;

    ss << name;
    ss << "[";
    ss << std::to_wstring(index);
    ss << "]";

    return ss.str();
}

inline void SetName(ComPtr<ID3D12Object> const& object, LPCWSTR const name) { TryDo(object->SetName(name)); }

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
void ResetComPtrArray(T* comPtrArray) { for (auto& i : *comPtrArray) i.Reset(); }


// Resets all elements in a unique_ptr array.
template <class T>
void ResetUniquePtrArray(T* uniquePtrArray) { for (auto& i : *uniquePtrArray) i.reset(); }

template <typename T>
std::vector<T> ReadBlob(ComPtr<ID3DBlob> const& blob)
{
    return std::vector<T>(
        static_cast<T*>(blob->GetBufferPointer()),
        static_cast<T*>(blob->GetBufferPointer()) + blob->GetBufferSize() / sizeof(T));
}
