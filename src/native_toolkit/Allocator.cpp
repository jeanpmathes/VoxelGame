#include "stdafx.h"

Allocator::Allocator() { heap = HeapCreate(HEAP_NO_SERIALIZE, 0, 0); }

std::byte* Allocator::Allocate(UINT64 const size) const { return static_cast<std::byte*>(HeapAlloc(heap, 0, size)); }

HRESULT Allocator::Deallocate(std::byte* pointer) const { return HeapFree(heap, 0, pointer); }

Allocator::~Allocator()
{
    HeapDestroy(heap);
    heap = nullptr;
}
