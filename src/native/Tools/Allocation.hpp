// <copyright file="Util.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "D3D12MemAlloc.hpp"

/**
 * Contains a resource and its allocation.
 */
template <typename T>
struct Allocation
{
    ComPtr<D3D12MA::Allocation> allocation;
    ComPtr<T> resource;

    Allocation() : allocation(nullptr), resource(nullptr)
    {
    }

    Allocation(ComPtr<D3D12MA::Allocation> allocation, ComPtr<T> resource)
        : allocation(allocation), resource(resource)
    {
    }

    auto Get() const
    {
        return resource.Get();
    }
};

template <typename T>
void SetName(const Allocation<T>& allocation, const LPCWSTR name)
{
    allocation.allocation->SetName(name);
    TRY_DO(allocation.resource->SetName(name));
}
