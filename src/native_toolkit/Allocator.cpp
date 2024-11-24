#include "stdafx.h"

Allocator::Allocator() { m_heap = HeapCreate(HEAP_NO_SERIALIZE, 0, 0); }

std::byte* Allocator::Allocate(UINT64 const size) const { return static_cast<std::byte*>(HeapAlloc(m_heap, 0, size)); }

HRESULT Allocator::Deallocate(std::byte* pointer) const { return HeapFree(m_heap, 0, pointer); }

Allocator::~Allocator()
{
    HeapDestroy(m_heap);
    m_heap = nullptr;
}
