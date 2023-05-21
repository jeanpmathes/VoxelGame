// <copyright file="Util.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "D3D12MemAlloc.h"

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

    Allocation(const ComPtr<D3D12MA::Allocation> allocation, ComPtr<T> resource)
        : allocation(allocation), resource(resource)
    {
    }

    auto Get() const
    {
        return resource.Get();
    }
};
