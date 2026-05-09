#include "stdafx.h"

InBufferAllocator::InBufferAllocator(NativeClient& client, D3D12_RESOURCE_STATES const state)
    : client(&client)
  , state(state)
  , pix(client.SupportPIX())
{
}

AddressableBuffer InBufferAllocator::Allocate(UINT64 const size)
{
    if (pix || size > MAX_SHARED_SIZE)
    {
        auto buffer = AllocateMemory(size);
        NAME_D3D12_OBJECT(buffer.resource);
        return AddressableBuffer(std::move(buffer));
    }

    return AllocateInternal(size);
}

void InBufferAllocator::CreateBarriers(ComPtr<ID3D12GraphicsCommandList> const& commandList, std::vector<ID3D12Resource*> const& resources)
{
    size_t const uavCount = resources.size() + blocks.size();

    barriers.clear();
    barriers.reserve(uavCount);

    for (ID3D12Resource* resource : resources)
    {
        D3D12_RESOURCE_BARRIER barrierDesc = {};
        barrierDesc.Type                   = D3D12_RESOURCE_BARRIER_TYPE_UAV;
        barrierDesc.UAV.pResource          = resource;
        barrierDesc.Flags                  = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barriers.push_back(barrierDesc);
    }

    for (auto const& block : blocks)
    {
        D3D12_RESOURCE_BARRIER barrierDesc = {};
        barrierDesc.Type                   = D3D12_RESOURCE_BARRIER_TYPE_UAV;
        barrierDesc.UAV.pResource          = block->GetResource();
        barrierDesc.Flags                  = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barriers.push_back(barrierDesc);
    }

    auto const barrierCount = static_cast<UINT>(uavCount);
    if (barrierCount == 0) return;

    commandList->ResourceBarrier(barrierCount, barriers.data());
}

AddressableBuffer InBufferAllocator::AllocateInternal(UINT64 const size)
{
    D3D12MA::VIRTUAL_ALLOCATION_DESC description = {};
    description.Size                             = size;
    description.Alignment                        = ALIGNMENT;

    std::optional<AddressableBuffer> allocation;

    for (; firstFreeBlock < blocks.size(); ++firstFreeBlock)
    {
        allocation = blocks[firstFreeBlock]->Allocate(&description);
        if (allocation.has_value()) return std::move(allocation.value());
    }

    Require(firstFreeBlock == blocks.size());
    blocks.emplace_back(Block::Create(*this, blocks.size()));

    allocation = blocks[firstFreeBlock]->Allocate(&description);
    Require(allocation.has_value());
    return std::move(allocation.value());
}

Allocation<ID3D12Resource> InBufferAllocator::AllocateMemory(UINT64 const size) const
{
    bool const committed = pix;
    return util::AllocateBuffer(*client, size, D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS, state, D3D12_HEAP_TYPE_DEFAULT, committed);
}

std::unique_ptr<InBufferAllocator::Block> InBufferAllocator::Block::Create(InBufferAllocator& allocator, size_t const index)
{
    D3D12MA::VirtualBlock* block;
    TryDo(CreateVirtualBlock(&allocator.blockDescription, &block));

    Allocation<ID3D12Resource> memory = allocator.AllocateMemory(BLOCK_SIZE);
    NAME_D3D12_OBJECT(memory.resource);

    return std::make_unique<Block>(block, std::move(memory), &allocator, index);
}

std::optional<AddressableBuffer> InBufferAllocator::Block::Allocate(D3D12MA::VIRTUAL_ALLOCATION_DESC const* description)
{
    if (description->Size >= limit) return std::nullopt;

    D3D12MA::VirtualAllocation allocation = {};
    UINT64                     offset;

    if (HRESULT const hr = block->Allocate(description, &allocation, &offset);
        SUCCEEDED(hr))
        return AddressableBuffer(memory.GetGPUVirtualAddress() + offset, allocation, this);

    limit = description->Size;

    return std::nullopt;
}

void InBufferAllocator::Block::FreeAllocation(D3D12MA::VirtualAllocation const allocation)
{
    block->FreeAllocation(allocation);

    limit                     = BLOCK_SIZE;
    allocator->firstFreeBlock = std::min(allocator->firstFreeBlock, index);
}

ID3D12Resource* InBufferAllocator::Block::GetResource() const { return memory.Get(); }

InBufferAllocator::Block::~Block() { if (block) block->Release(); }

InBufferAllocator::Block::Block(D3D12MA::VirtualBlock* block, Allocation<ID3D12Resource> memory, InBufferAllocator* allocator, size_t const index)
    : block(block)
  , memory(std::move(memory))
  , allocator(allocator)
  , index(index)
{
}

AddressableBuffer::AddressableBuffer(Allocation<ID3D12Resource> wrappedResource)
    : resource(wrappedResource)
  , address(resource.value().GetGPUVirtualAddress())
{
}

AddressableBuffer::AddressableBuffer(D3D12_GPU_VIRTUAL_ADDRESS const address, D3D12MA::VirtualAllocation const allocation, InBufferAllocator::Block* block)
    : address(address)
  , allocation(allocation)
  , block(block)
{
}

AddressableBuffer::AddressableBuffer(AddressableBuffer&& other) noexcept
{
    std::swap(resource, other.resource);
    std::swap(address, other.address);
    std::swap(allocation, other.allocation);
    std::swap(block, other.block);
}

AddressableBuffer& AddressableBuffer::operator=(AddressableBuffer&& other) noexcept
{
    std::swap(resource, other.resource);
    std::swap(address, other.address);
    std::swap(allocation, other.allocation);
    std::swap(block, other.block);

    return *this;
}

AddressableBuffer::~AddressableBuffer()
{
    if (resource.has_value() || block == nullptr) return;
    block->FreeAllocation(allocation);
}

D3D12_GPU_VIRTUAL_ADDRESS AddressableBuffer::GetAddress() const { return address; }

ID3D12Resource* AddressableBuffer::GetResource() const
{
    return resource.has_value() ? resource.value().Get() : nullptr;
}
