#include "stdafx.h"

DescriptorHeap DescriptorHeap::CreateNew(
    const ComPtr<ID3D12Device5>& device,
    const UINT numDescriptors,
    const D3D12_DESCRIPTOR_HEAP_TYPE type,
    const bool shaderVisible)
{
    DescriptorHeap heap;
    heap.Create(device, numDescriptors, type, shaderVisible);
    return heap;
}

void DescriptorHeap::Create(
    const ComPtr<ID3D12Device5>& device,
    const UINT numDescriptors,
    const D3D12_DESCRIPTOR_HEAP_TYPE type,
    const bool shaderVisible,
    const bool copyExisting)
{
    const ComPtr<ID3D12DescriptorHeap> oldHeap = m_heap;
    const UINT oldNumDescriptors = m_numDescriptors;
    REQUIRE(IMPLIES(copyExisting, numDescriptors >= oldNumDescriptors));
    
    m_heap = nullptr;

    m_device = device;
    m_increment = device->GetDescriptorHandleIncrementSize(type);
    m_numDescriptors = numDescriptors;
    m_type = type;

    D3D12_DESCRIPTOR_HEAP_DESC description = {};
    description.NumDescriptors = numDescriptors;
    description.Type = type;
    description.Flags =
        shaderVisible ? D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE : D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

    TRY_DO(device->CreateDescriptorHeap(&description, IID_PPV_ARGS(&m_heap)));

    m_startCPU = m_heap->GetCPUDescriptorHandleForHeapStart();
    m_startGPU = shaderVisible ? m_heap->GetGPUDescriptorHandleForHeapStart() : D3D12_GPU_DESCRIPTOR_HANDLE{};

    if (copyExisting && oldHeap != nullptr && oldNumDescriptors > 0)
    {
        device->CopyDescriptorsSimple(
            oldNumDescriptors,
            m_startCPU,
            oldHeap->GetCPUDescriptorHandleForHeapStart(),
            type);
    }
}

D3D12_CPU_DESCRIPTOR_HANDLE DescriptorHeap::GetDescriptorHandleCPU(const UINT index) const
{
    REQUIRE(IsCreated());
    return Offset(m_startCPU, index);
}

D3D12_GPU_DESCRIPTOR_HANDLE DescriptorHeap::GetDescriptorHandleGPU(const UINT index) const
{
    REQUIRE(IsCreated());
    return Offset(m_startGPU, index);
}

ID3D12DescriptorHeap* DescriptorHeap::Get() const
{
    REQUIRE(IsCreated());
    return m_heap.Get();
}

bool DescriptorHeap::IsCreated() const
{
    return m_heap != nullptr;
}

UINT DescriptorHeap::GetDescriptorCount() const
{
    REQUIRE(IsCreated());
    return m_numDescriptors;
}

ID3D12DescriptorHeap** DescriptorHeap::GetAddressOf()
{
    return m_heap.GetAddressOf();
}

UINT DescriptorHeap::GetIncrement() const
{
    return m_increment;
}

D3D12_CPU_DESCRIPTOR_HANDLE DescriptorHeap::Offset(const D3D12_CPU_DESCRIPTOR_HANDLE handle, const UINT index) const
{
    return CD3DX12_CPU_DESCRIPTOR_HANDLE(handle, static_cast<INT>(index), m_increment);
}

D3D12_GPU_DESCRIPTOR_HANDLE DescriptorHeap::Offset(const D3D12_GPU_DESCRIPTOR_HANDLE handle, const UINT index) const
{
    return CD3DX12_GPU_DESCRIPTOR_HANDLE(handle, static_cast<INT>(index), m_increment);
}

void DescriptorHeap::CopyTo(const DescriptorHeap& other, const UINT offset) const
{
    REQUIRE(IsCreated());
    REQUIRE(other.IsCreated());

    REQUIRE(m_type == other.m_type);
    REQUIRE(other.GetDescriptorCount() >= GetDescriptorCount() + offset);

    m_device->CopyDescriptorsSimple(
        GetDescriptorCount(),
        other.GetDescriptorHandleCPU(offset),
        GetDescriptorHandleCPU(),
        m_type);
}
