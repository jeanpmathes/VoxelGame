// <copyright file="DescriptorHeap.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

/**
 * Contains a descriptor heap.
 */
class DescriptorHeap
{
public:
    static DescriptorHeap CreateNew(
        ComPtr<ID3D12Device5> const& device,
        UINT                         numDescriptors,
        D3D12_DESCRIPTOR_HEAP_TYPE   type,
        bool                         shaderVisible);

    /**
     * \brief Create a descriptor heap. If this class already contains a heap, it will be destroyed.
     * \param device The device to create the heap on.
     * \param numDescriptors The number of descriptors in the heap.
     * \param type The type of the heap.
     * \param shaderVisible Whether the heap should be shader visible.
     * \param copyExisting Whether the existing heap, if any, should be copied to the new heap.
     */
    void Create(
        ComPtr<ID3D12Device5> const& device,
        UINT                         numDescriptors,
        D3D12_DESCRIPTOR_HEAP_TYPE   type,
        bool                         shaderVisible,
        bool                         copyExisting = false);

    DescriptorHeap()                                 = default;
    DescriptorHeap(DescriptorHeap const&)            = delete;
    DescriptorHeap& operator=(DescriptorHeap const&) = delete;
    DescriptorHeap(DescriptorHeap&&)                 = default;
    DescriptorHeap& operator=(DescriptorHeap&&)      = default;
    ~DescriptorHeap()                                = default;

    [[nodiscard]] D3D12_CPU_DESCRIPTOR_HANDLE GetDescriptorHandleCPU(UINT index = 0) const;
    [[nodiscard]] D3D12_GPU_DESCRIPTOR_HANDLE GetDescriptorHandleGPU(UINT index = 0) const;
    [[nodiscard]] ID3D12DescriptorHeap*       Get() const;
    [[nodiscard]] bool                        IsCreated() const;
    [[nodiscard]] UINT                        GetDescriptorCount() const;
    [[nodiscard]] ID3D12DescriptorHeap**      GetAddressOf();

    [[nodiscard]] UINT GetIncrement() const;

    [[nodiscard]] D3D12_CPU_DESCRIPTOR_HANDLE Offset(D3D12_CPU_DESCRIPTOR_HANDLE handle, UINT index) const;
    [[nodiscard]] D3D12_GPU_DESCRIPTOR_HANDLE Offset(D3D12_GPU_DESCRIPTOR_HANDLE handle, UINT index) const;

    /**
     * Copy the descriptors from this heap to another heap.
     * They will be copied starting at the given offset.
     */
    void CopyTo(DescriptorHeap const& other, UINT offset) const;

    friend void SetName(DescriptorHeap const&, LPCWSTR);

private:
    ComPtr<ID3D12DescriptorHeap> m_heap;
    ComPtr<ID3D12Device5>        m_device;

    D3D12_CPU_DESCRIPTOR_HANDLE m_startCPU{};
    D3D12_GPU_DESCRIPTOR_HANDLE m_startGPU{};
    UINT                        m_increment{};
    UINT                        m_numDescriptors{};
    D3D12_DESCRIPTOR_HEAP_TYPE  m_type{};
};

inline void SetName(DescriptorHeap const& heap, LPCWSTR const name) { SetName(heap.m_heap, name); }
