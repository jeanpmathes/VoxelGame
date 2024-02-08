#include "stdafx.h"

InBufferAllocator::InBufferAllocator(NativeClient& client, D3D12_RESOURCE_STATES const state)
    : m_client(&client)
  , m_state(state)
  , m_pix(client.SupportPIX())
{
}

AddressableBuffer InBufferAllocator::Allocate(UINT64 const size)
{
    if (m_pix || size > MAX_SHARED_SIZE)
    {
        auto buffer = AllocateMemory(size);
        NAME_D3D12_OBJECT(buffer.resource);
        return AddressableBuffer(std::move(buffer));
    }

    return AllocateInternal(size);
}

void InBufferAllocator::CreateBarriers(
    ComPtr<ID3D12GraphicsCommandList> commandList, std::vector<ID3D12Resource*>&& resources) const
{
    size_t const uavCount = resources.size() + m_blocks.size();

    std::vector<D3D12_RESOURCE_BARRIER> barriers;
    barriers.reserve(uavCount);

    for (size_t resource = 0; resource < resources.size(); ++resource)
    {
        D3D12_RESOURCE_BARRIER barrierDesc = {};
        barrierDesc.Type                   = D3D12_RESOURCE_BARRIER_TYPE_UAV;
        barrierDesc.UAV.pResource          = resources[resource];
        barrierDesc.Flags                  = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barriers.push_back(barrierDesc);
    }

    for (size_t block = 0; block < m_blocks.size(); ++block)
    {
        D3D12_RESOURCE_BARRIER barrierDesc = {};
        barrierDesc.Type                   = D3D12_RESOURCE_BARRIER_TYPE_UAV;
        barrierDesc.UAV.pResource          = m_blocks[block]->memory.Get();
        barrierDesc.Flags                  = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barriers.push_back(barrierDesc);
    }

    UINT const barrierCount = static_cast<UINT>(uavCount);
    if (barrierCount == 0) return;

    commandList->ResourceBarrier(barrierCount, barriers.data());
}

AddressableBuffer InBufferAllocator::AllocateInternal(UINT64 const size)
{
    D3D12MA::VIRTUAL_ALLOCATION_DESC description = {};
    description.Size                             = size;
    description.Alignment                        = ALIGNMENT;

    std::optional<AddressableBuffer> allocation;

    for (; m_firstFreeBlock < m_blocks.size(); ++m_firstFreeBlock)
    {
        allocation = m_blocks[m_firstFreeBlock]->Allocate(&description);
        if (allocation.has_value()) return std::move(allocation.value());
    }

    REQUIRE(m_firstFreeBlock == m_blocks.size());
    m_blocks.emplace_back(Block::Create(*this, m_blocks.size()));

    allocation = m_blocks[m_firstFreeBlock]->Allocate(&description);
    REQUIRE(allocation.has_value());
    return std::move(allocation.value());
}

Allocation<ID3D12Resource> InBufferAllocator::AllocateMemory(UINT64 const size) const
{
    bool const committed = m_pix;
    return util::AllocateBuffer(
        *m_client,
        size,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
        m_state,
        D3D12_HEAP_TYPE_DEFAULT,
        committed);
}

std::unique_ptr<InBufferAllocator::Block> InBufferAllocator::Block::Create(
    InBufferAllocator& allocator, size_t const index)
{
    D3D12MA::VirtualBlock* block;
    TRY_DO(CreateVirtualBlock(&allocator.m_blockDescription, &block));

    Allocation<ID3D12Resource> memory = allocator.AllocateMemory(BLOCK_SIZE);
    NAME_D3D12_OBJECT(memory.resource);

    return std::make_unique<Block>(block, std::move(memory), &allocator, index);
}

std::optional<AddressableBuffer> InBufferAllocator::Block::Allocate(D3D12MA::VIRTUAL_ALLOCATION_DESC const* description)
{
    if (description->Size >= m_limit) return std::nullopt;

    D3D12MA::VirtualAllocation allocation = {};
    UINT64                     offset;

    if (HRESULT const hr = block->Allocate(description, &allocation, &offset);
        SUCCEEDED(hr))
        return AddressableBuffer(memory.GetGPUVirtualAddress() + offset, allocation, this);

    m_limit = description->Size;

    return std::nullopt;
}

void InBufferAllocator::Block::FreeAllocation(D3D12MA::VirtualAllocation const allocation)
{
    block->FreeAllocation(allocation);

    m_limit                       = BLOCK_SIZE;
    m_allocator->m_firstFreeBlock = std::min(m_allocator->m_firstFreeBlock, m_index);
}

InBufferAllocator::Block::~Block() { if (block) block->Release(); }

InBufferAllocator::Block::Block(
    D3D12MA::VirtualBlock* block, Allocation<ID3D12Resource>&& memory, InBufferAllocator* allocator, size_t const index)
    : block(block)
  , memory(std::move(memory))
  , m_allocator(allocator)
  , m_index(index)
{
}

AddressableBuffer::AddressableBuffer(Allocation<ID3D12Resource>&& resource)
{
    m_address  = resource.GetGPUVirtualAddress();
    m_resource = std::move(resource);
}

AddressableBuffer::AddressableBuffer(
    D3D12_GPU_VIRTUAL_ADDRESS const address, D3D12MA::VirtualAllocation const allocation,
    InBufferAllocator::Block*       block)
    : m_address(address)
  , m_allocation(allocation)
  , m_block(block)
{
}

AddressableBuffer::AddressableBuffer(AddressableBuffer&& other) noexcept
{
    std::swap(m_resource, other.m_resource);
    std::swap(m_address, other.m_address);
    std::swap(m_allocation, other.m_allocation);
    std::swap(m_block, other.m_block);
}

AddressableBuffer& AddressableBuffer::operator=(AddressableBuffer&& other) noexcept
{
    std::swap(m_resource, other.m_resource);
    std::swap(m_address, other.m_address);
    std::swap(m_allocation, other.m_allocation);
    std::swap(m_block, other.m_block);

    return *this;
}

AddressableBuffer::~AddressableBuffer()
{
    if (m_resource.has_value() || m_block == nullptr) return;
    m_block->FreeAllocation(m_allocation);
}

D3D12_GPU_VIRTUAL_ADDRESS AddressableBuffer::GetAddress() const { return m_address; }

ID3D12Resource* AddressableBuffer::GetResource() const
{
    return m_resource.has_value() ? m_resource.value().Get() : nullptr;
}
