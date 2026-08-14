// <copyright file="Basic.hpp" company="VoxelGame">
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

#include <format>
#include <source_location>
#include <sstream>
#include <stdexcept>

// ReSharper disable once CppUnusedIncludeDirective
#include <wrl.h>

#include <d2d1_3.h>
#include <d3d11.h>

using Microsoft::WRL::ComPtr;

inline std::string HResultToString(HRESULT const hr) { return std::format("Error: (HRESULT) {:#x}", hr); }

class HResultException final : public std::runtime_error
{
public:
    explicit HResultException(HRESULT const hr, std::string const& info)
        : std::runtime_error(HResultToString(hr) + "\nInfo: " + info)
      , hr(hr)
      , info(info)
    {
    }

    [[nodiscard]] HRESULT     Error() const { return hr; }
    [[nodiscard]] char const* Info() const { return info.c_str(); }

private:
    HRESULT     hr;
    std::string info;
};

class NativeException final : public std::runtime_error
{
public:
    using std::runtime_error::runtime_error;
};

#ifdef NATIVE_DEBUG
constexpr bool IS_DEBUG_BUILD = true;
#else
constexpr bool IS_DEBUG_BUILD = false;
#endif

inline bool TryBreak()
{
#ifdef NATIVE_DEBUG
    __try
    {
        DebugBreak();
    }
    __except (GetExceptionCode() == EXCEPTION_BREAKPOINT ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
    {
        return false;
    }

    return true;
#else
    return false;
#endif
}

constexpr bool Implies(bool const a, bool const b) { return !a || b; }

/**
 * \brief Assert that a condition is true. 
 */
constexpr void Require(bool const condition, std::source_location const& location = std::source_location::current())
{
    if constexpr (!IS_DEBUG_BUILD) return;

    if (!condition)
    {
        std::string const message = std::format("failed requirement in function {} at {}:{}:{}", location.function_name(), location.file_name(), location.line(), location.column());

        TryBreak();

        throw NativeException(message);
    }
}

inline std::string GetTryDoMessage(std::source_location const& location)
{
    if constexpr (IS_DEBUG_BUILD) return std::format("throwing from function {} at {}:{}:{}", location.function_name(), location.file_name(), location.line(), location.column());
    else return std::format("throwing from function {}", location.function_name());
}

/**
 * \brief Try to do something, e.g. a Win32 API call, and throw an exception if it fails.
 */
inline void TryDo(BOOL const b, bool const breakpoint = true, std::source_location const& location = std::source_location::current())
{
    if (b) return;

    std::string const message = GetTryDoMessage(location);

    if (breakpoint) TryBreak();

    throw HResultException(HRESULT_FROM_WIN32(GetLastError()), message);
}

/**
 * \brief Try to do something, e.g. a DirectX API call, and throw an exception if it fails.
 */
inline void TryDo(HRESULT const hr, bool const breakpoint = true, std::source_location const& location = std::source_location::current())
{
    if (SUCCEEDED(hr)) return;

    std::string const message = GetTryDoMessage(location);

    if (breakpoint) TryBreak();

    throw HResultException(hr, message);
}

/**
 * \brief Check that the return value of a function is not NULL, and throw an exception based on GetLastError if it is.
 */
template <typename T>
constexpr T const& CheckReturn(T const& value, bool const breakpoint = true, std::source_location const& location = std::source_location::current())
{
    if (value != NULL) return value;

    std::string const message = std::format(
                                            "error with value of type '{}' in function {} at {}:{}:{}",
                                            typeid(T).name(),
                                            location.function_name(),
                                            location.file_name(),
                                            location.line(),
                                            location.column());

    if (breakpoint) TryBreak();

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

inline void SetName(ComPtr<ID3D11DeviceChild> const& object, LPCWSTR const name)
{
    TryDo(object->SetPrivateData(WKPDID_D3DDebugObjectName, static_cast<UINT>(wcslen(name)) * sizeof(WCHAR), name));
}

inline void SetName(ComPtr<IDXGIObject> const& object, LPCWSTR const name)
{
    TryDo(object->SetPrivateData(WKPDID_D3DDebugObjectName, static_cast<UINT>(wcslen(name)) * sizeof(WCHAR), name));
}

// Naming helper for ComPtr<T>.
// Assigns the name of the variable as the name of the object.
// The indexed variant will include the index in the name of the object.

// ReSharper disable CppInconsistentNaming
#define NAME_DIRECT_OBJECT(object) \
    do { \
        if (!IS_DEBUG_BUILD) break; \
        SetName((object), L#object); \
    } while (false)

#define NAME_DIRECT_OBJECT_INDEXED(object, index) \
    do { \
        if (!IS_DEBUG_BUILD) break; \
        SetName((object)[index], GetNameIndexed(L#object, index).c_str()); \
    } while (false)
// ReSharper restore CppInconsistentNaming

inline UINT CalculateConstantBufferByteSize(UINT byteSize)
{
    // Constant buffer size is required to be aligned.
    return (byteSize + (D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1)) & ~(D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT - 1);
}

// Resets all elements in a ComPtr array.
template <class T>
void ResetComPtrArray(T* comPtrArray) { for (auto& i : *comPtrArray) i.Reset(); }


// Resets all elements in a unique_ptr array.
template <class T>
void ResetUniquePtrArray(T* uniquePtrArray) { for (auto& i : *uniquePtrArray) i.reset(); }

/**
* \brief Round a value up to the nearest multiple of an alignment.
 * \param value The value to round up.
 * \param alignment The alignment to round up to.
 * \return The rounded up value.
 */
template <typename T, typename V>
constexpr T RoundUp(T const value, V const alignment) { return (value + alignment - 1) & ~(alignment - 1); }
