// <copyright file="Object.h" company="VoxelGame">
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
    private:
// ReSharper disable once CppInconsistentNaming
#define NAME_D3D12_OBJECT_WITH_ID(object) \
    do \
    { \
        if (object != nullptr) \
        { \
            object->SetName((std::wstring(L#object) + L" " + std::to_wstring(GetID())).c_str()); \
        } \
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

    [[nodiscard]] NativeClient& GetClient() const;
    [[nodiscard]] uint64_t GetID() const;

private:
    NativeClient& m_client;

    static uint64_t m_nextID;
    uint64_t m_id = m_nextID++;
};
