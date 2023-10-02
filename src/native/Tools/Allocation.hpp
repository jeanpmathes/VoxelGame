// <copyright file="Util.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "D3D12MemAlloc.hpp"

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
    Mapping() : m_resource({})
    {
    }

    explicit Mapping(const Allocation<R>& resource, HRESULT* out) : m_resource(resource)
    {
        constexpr D3D12_RANGE readRange = {0, 0}; // We do not intend to read from this resource on the CPU.
        *out = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&m_data));
    }

    void Write(const S& data)
    {
        REQUIRE(m_data != nullptr);
        *m_data = data;
    }

    ~Mapping()
    {
        if (m_data != nullptr)
            m_resource.resource->Unmap(0, nullptr);
    }

    Mapping(const Mapping&) = delete;
    Mapping& operator=(const Mapping&) = delete;

    Mapping(Mapping&& other) noexcept
    {
        m_resource = other.m_resource;
        m_data = other.m_data;

        other.m_data = nullptr;
    }

    Mapping& operator=(Mapping&& other) noexcept
    {
        m_resource = other.m_resource;
        m_data = other.m_data;

        other.m_data = nullptr;

        return *this;
    }

private:
    Allocation<R> m_resource;
    S* m_data = nullptr;
};

/**
 * Contains a resource and its allocation.
 */
template <typename R>
struct Allocation
{
    ComPtr<D3D12MA::Allocation> allocation;
    ComPtr<R> resource;

    Allocation() : allocation(nullptr), resource(nullptr)
    {
    }

    Allocation(ComPtr<D3D12MA::Allocation> allocation, ComPtr<R> resource)
        : allocation(allocation), resource(resource)
    {
    }

    auto Get() const
    {
        return resource.Get();
    }

    template <typename S>
        requires std::is_same_v<R, ID3D12Resource>
    [[nodiscard]] HRESULT Map(Mapping<R, S>* mapping) const
    {
        HRESULT result = S_OK;
        *mapping = Mapping<R, S>(*this, &result);
        return result;
    }

    template <typename = std::nullopt_t>
        requires std::is_same_v<R, ID3D12Resource>
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress() const
    {
        return resource->GetGPUVirtualAddress();
    }
};

template <typename T>
void SetName(const Allocation<T>& allocation, const LPCWSTR name)
{
    allocation.allocation->SetName(name);
    TRY_DO(allocation.resource->SetName(name));
}
