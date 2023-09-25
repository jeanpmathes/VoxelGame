// <copyright file="DescriptorHeap.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

/**
 * Contains a descriptor heap.
 */
class DescriptorHeap
{
public:
    static DescriptorHeap CreateNew(const ComPtr<ID3D12Device5>& device, UINT numDescriptors,
                                    D3D12_DESCRIPTOR_HEAP_TYPE type, bool shaderVisible);
    void Create(const ComPtr<ID3D12Device5>& device, UINT numDescriptors, D3D12_DESCRIPTOR_HEAP_TYPE type,
                bool shaderVisible);

    DescriptorHeap() = default;
    DescriptorHeap(const DescriptorHeap&) = delete;
    DescriptorHeap(DescriptorHeap&&) = default;
    DescriptorHeap& operator=(const DescriptorHeap&) = delete;
    DescriptorHeap& operator=(DescriptorHeap&&) = default;
    ~DescriptorHeap() = default;

    [[nodiscard]] D3D12_CPU_DESCRIPTOR_HANDLE GetDescriptorHandleCPU(UINT index = 0) const;
    [[nodiscard]] D3D12_GPU_DESCRIPTOR_HANDLE GetDescriptorHandleGPU(UINT index = 0) const;
    [[nodiscard]] ID3D12DescriptorHeap* Get() const;
    [[nodiscard]] bool IsCreated() const;
    [[nodiscard]] UINT GetDescriptorCount() const;

    [[nodiscard]] ID3D12DescriptorHeap** GetAddressOf();
    
    friend void SetName(const DescriptorHeap&, LPCWSTR);

private:
    ComPtr<ID3D12DescriptorHeap> m_heap;
    ComPtr<ID3D12Device5> m_device;

    D3D12_CPU_DESCRIPTOR_HANDLE m_startCPU{};
    D3D12_GPU_DESCRIPTOR_HANDLE m_startGPU{};
    UINT m_increment{};
    UINT m_numDescriptors{};
};

inline void SetName(const DescriptorHeap& heap, const LPCWSTR name)
{
    SetName(heap.m_heap, name);
}
