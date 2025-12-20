// <copyright file="Object.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

class NativeClient;

// ReSharper disable CppClangTidyBugproneMacroParentheses
#define DECLARE_OBJECT_SUBCLASS(name) \
    public: \
    ~name() override = default; \
    name(const name&) = delete; \
    name(name&&) = delete; \
    name& operator=(const name&) = delete; \
    name& operator=(name&&) = delete; \
    private: \
    constexpr static const wchar_t* const ClassName = L#name;
// ReSharper disable once CppInconsistentNaming
#define NAME_D3D12_OBJECT_WITH_ID(object) \
    do \
    { \
        if (!IS_DEBUG_BUILD) break; \
        SetName((object), \
            (std::wstring(L#object) + \
            L" in " + \
            std::wstring(ClassName) + \
            L" #" + \
            std::to_wstring(GetID())).c_str()); \
    } while (false)

// ReSharper disable once CppInconsistentNaming
#define NAME_D3D12_OBJECT_INDEXED_WITH_ID(objects, index) \
    do \
    { \
        if (!IS_DEBUG_BUILD) break; \
        SetName(objects[index], \
            (std::wstring(L#objects) + \
            L"[" + \
            std::to_wstring(index) + \
            L"] in " + \
            std::wstring(ClassName) + \
            L" #" + \
            std::to_wstring(GetID())).c_str()); \
    } while (false)

/**
 * A basic object base class, represents things that can be passed over the native-to-managed boundary.
 */
class Object
{
public:
    virtual ~Object()                = default;
    Object(Object const&)            = delete;
    Object(Object&&)                 = delete;
    Object& operator=(Object const&) = delete;
    Object& operator=(Object&&)      = delete;

protected:
    explicit Object(NativeClient& client);

public:
    [[nodiscard]] NativeClient& GetClient() const;
    [[nodiscard]] UINT64        GetID() const;

private:
    NativeClient* m_client;

    static UINT64 m_nextID;
    UINT64        m_id = m_nextID++;
};
