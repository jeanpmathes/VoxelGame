// <copyright file="InBufferAllocator.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

class NativeClient;

struct AddressableBuffer;

/**
 * Helps allocating memory for BLAS by allocating on buffers, thus allowing to use the small alignment requirements of BLAS.
 */
class InBufferAllocator
{
public:
    static constexpr UINT BLOCK_SIZE = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;
    static constexpr UINT ALIGNMENT = D3D12_RAYTRACING_ACCELERATION_STRUCTURE_BYTE_ALIGNMENT;
    static constexpr UINT MAX_SHARED_SIZE = BLOCK_SIZE / 4;

    /**
     * Creates a new allocator that allocates using a buffer with the given state.
     */
    InBufferAllocator(NativeClient& client, D3D12_RESOURCE_STATES state);

    /**
     * Allocates memory for a buffer of the given size.
     */
    AddressableBuffer Allocate(UINT64 size);

    /**
     * Create barriers for all resources that are used by this allocator.
     * Additionally, a vector of further resources can be passed to create barriers for them as well.
     */
    void CreateBarriers(ComPtr<ID3D12GraphicsCommandList> commandList, std::vector<ID3D12Resource*>&& resources) const;

private:
    AddressableBuffer AllocateInternal(UINT64 size);
    [[nodiscard]] Allocation<ID3D12Resource> AllocateMemory(UINT64 size) const;

    NativeClient* m_client;
    D3D12_RESOURCE_STATES m_state;
    bool m_pix;

    D3D12MA::VIRTUAL_BLOCK_DESC m_blockDescription =
    {
        .Size = BLOCK_SIZE,
    };

    friend struct AddressableBuffer;

    struct Block
    {
        D3D12MA::VirtualBlock* block = nullptr;
        Allocation<ID3D12Resource> memory = {};

        static std::unique_ptr<Block> Create(InBufferAllocator& allocator, size_t index);
        std::optional<AddressableBuffer> Allocate(const D3D12MA::VIRTUAL_ALLOCATION_DESC* description);
        void FreeAllocation(D3D12MA::VirtualAllocation allocation);

        Block(const Block&) = delete;
        Block& operator=(const Block&) = delete;

        Block(Block&&) = delete;
        Block& operator=(Block&&) = delete;

        ~Block();

        Block(D3D12MA::VirtualBlock* block, Allocation<ID3D12Resource>&& memory, InBufferAllocator* allocator,
              size_t index);

    private:
        InBufferAllocator* m_allocator = nullptr;
        size_t m_index = 0;
        UINT64 m_limit = BLOCK_SIZE;
    };

    std::vector<std::unique_ptr<Block>> m_blocks = {};
    size_t m_firstFreeBlock = 0;
};

struct AddressableBuffer
{
    AddressableBuffer() = default;

    explicit AddressableBuffer(Allocation<ID3D12Resource>&& resource);
    explicit AddressableBuffer(
        D3D12_GPU_VIRTUAL_ADDRESS address, D3D12MA::VirtualAllocation allocation,
        InBufferAllocator::Block* block);

    AddressableBuffer(const AddressableBuffer&) = delete;
    AddressableBuffer& operator=(const AddressableBuffer&) = delete;

    AddressableBuffer(AddressableBuffer&&) noexcept;
    AddressableBuffer& operator=(AddressableBuffer&&) noexcept;

    ~AddressableBuffer();

    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetAddress() const;
    [[nodiscard]] ID3D12Resource* GetResource() const;

    friend void SetName(const AddressableBuffer&, LPCWSTR);

private:
    std::optional<Allocation<ID3D12Resource>> m_resource = std::nullopt;

    D3D12_GPU_VIRTUAL_ADDRESS m_address = 0;
    D3D12MA::VirtualAllocation m_allocation = {};
    InBufferAllocator::Block* m_block = nullptr;
};

struct BLAS
{
    AddressableBuffer result;
    AddressableBuffer scratch;
};

inline void SetName(const AddressableBuffer& object, const LPCWSTR name)
{
    if (object.m_resource.has_value()) SetName(object.m_resource.value(), name);
}
