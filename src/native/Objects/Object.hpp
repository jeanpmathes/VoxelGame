// <copyright file="Object.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
 * \brief A basic object base class, represents things that can be passed over the native-to-managed boundary.
 */
class Object
{
public:
    virtual ~Object() = default;
    Object(const Object&) = delete;
    Object(Object&&) = delete;
    Object& operator=(const Object&) = delete;
    Object& operator=(Object&&) = delete;

protected:
    explicit Object(NativeClient& client);

public:
    [[nodiscard]] NativeClient& GetClient() const;
    [[nodiscard]] UINT64 GetID() const;

private:
    NativeClient& m_client;

    static UINT64 m_nextID;
    UINT64 m_id = m_nextID++;
};
