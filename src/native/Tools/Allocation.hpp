// <copyright file="Util.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <optional>

#include "D3D12MemAlloc.hpp"
#include "DXHelper.hpp"

template <typename R>
struct Allocation;

/**
 * Represents the mapping of a resource R in memory.
 * The resource contains data formatted according to the struct S.
 * Allows writing to the resource.
 */
template <typename R, typename S>
class Mapping
{
public:
    Mapping()
        : m_resource({})
    {
    }

    /**
     * \brief Create a new mapping for a given resource.
     * \param resource The resource to map.
     * \param out A pointer to a HRESULT that will be set to the result of the mapping operation.
     * \param size The size of the resource in number of elements.
     */
    explicit Mapping(Allocation<R> const& resource, HRESULT* out, size_t const size)
        : m_resource(resource)
      , m_size(size)
    {
        Require(resource.resource != nullptr);
        Require(out != nullptr);
        Require(size > 0);

        constexpr D3D12_RANGE readRange = {0, 0}; // We do not intend to read from this resource on the CPU.
        *out                            = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&m_data));

        Require(m_data != nullptr);

        size_t const requiredSizeInBytes = m_size * sizeof(S);
        size_t const actualSizeInBytes   = m_resource.resource->GetDesc().Width;
        Require(requiredSizeInBytes <= actualSizeInBytes);
    }

    /**
     * \return The size of the mapped resource in number of elements.
     */
    [[nodiscard]] size_t GetSize() const { return m_size; }

    /**
     * \brief Write directly to the resource.
     * \return A pointer to the resource. Only writing to this pointer is allowed.
     */
    S* operator->()
    {
        Require(m_data != nullptr);
        return m_data;
    }

    /**
     * \brief Write data to the resource.
     * \param data The data to write.
     */
    void Write(S const& data)
    {
        Require(m_data != nullptr);

        *m_data = data;
    }

    /**
     * \brief Write data to the resource.
     * \param data Where to read the data from.
     * \param count How many elements to write.
     */
    void Write(S const* data, size_t const count)
    {
        Require(m_data != nullptr);
        Require(count <= m_size);

        std::memcpy(m_data, data, count * sizeof(S));
    }

    /**
     * \brief Fill the resource with zeros.
     */
    void Clear()
    {
        Require(m_data != nullptr);

        std::memset(m_data, 0, m_size * sizeof(S));
    }

    /**
     * \brief Write the data. If the data is null or the count is zero, clear the resource.
     * \param data The data to write.
     * \param count The number of elements to write.
     */
    void WriteOrClear(S const* data, size_t const count)
    {
        if (data == nullptr || count == 0) Clear();
        else Write(data, count);
    }

    void Unmap()
    {
        Require(m_data != nullptr);
        
        m_resource.resource->Unmap(0, nullptr);
        m_data = nullptr;
    }

    void UnmapSafe()
    {
        if (m_data == nullptr) return;

        try { Unmap(); }
        catch (...)
        {
            // Should not cause any problems for the rest of the program.
        }
    }

    ~Mapping() { UnmapSafe(); }

    Mapping(Mapping const&)            = delete;
    Mapping& operator=(Mapping const&) = delete;

    Mapping(Mapping&& other) noexcept
        : m_resource(other.m_resource)
      , m_data(other.m_data)
      , m_size(other.m_size) { other.m_data = nullptr; }

    Mapping& operator=(Mapping&& other) noexcept
    {
        UnmapSafe();

        m_resource = other.m_resource;
        m_data     = other.m_data;
        m_size     = other.m_size;

        other.m_data = nullptr;

        return *this;
    }

private:
    Allocation<R> m_resource;

    S*     m_data = nullptr;
    size_t m_size = 0;
};

/**
 * Contains a resource and its allocation.
 */
template <typename R>
struct Allocation
{
    ComPtr<D3D12MA::Allocation> allocation;
    ComPtr<R>                   resource;

    Allocation()
        : allocation(nullptr)
      , resource(nullptr)
    {
    }

    /**
     * \brief Wrap the pointers of an allocation. 
     * \param allocation The memory allocation.
     * \param resource The resource in the allocation.
     */
    Allocation(ComPtr<D3D12MA::Allocation> allocation, ComPtr<R> resource)
        : allocation(allocation)
      , resource(resource)
    {
    }

    [[nodiscard]] auto Get() const { return resource.Get(); }

    [[nodiscard]] bool IsSet() const { return resource != nullptr; }

    /**
     * \brief Map the resource to memory.
     * \tparam S The type of the data in the resource.
     * \param mapping The mapping to create.
     * \param size The size of the resource in number of elements.
     * \return The result of the mapping operation.
     */
    template <typename S>
        requires std::is_same_v<R, ID3D12Resource>
    [[nodiscard]] HRESULT Map(Mapping<R, S>* mapping, size_t size) const
    {
        auto result = S_OK;
        *mapping    = Mapping<R, S>(*this, &result, size);
        return result;
    }

    template <typename = std::nullopt_t>
        requires std::is_same_v<R, ID3D12Resource>
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress() const { return resource->GetGPUVirtualAddress(); }
};

template <typename T>
void SetName(Allocation<T> const& allocation, LPCWSTR const name)
{
    allocation.allocation->SetName(name);
    TryDo(allocation.resource->SetName(name));
}
