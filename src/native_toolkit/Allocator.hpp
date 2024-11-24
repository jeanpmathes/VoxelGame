// <copyright file="Allocator.hpp" company="VoxelGame">
// MIT License
// For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

class Allocator
{
public:
    Allocator();

    Allocator(Allocator const&)            = delete;
    Allocator& operator=(Allocator const&) = delete;

    Allocator(Allocator&&)            = delete;
    Allocator& operator=(Allocator&&) = delete;

    [[nodiscard]] std::byte* Allocate(UINT64 size) const;
    [[nodiscard]] HRESULT    Deallocate(std::byte* pointer) const;

    ~Allocator();

private:
    HANDLE m_heap;
};
