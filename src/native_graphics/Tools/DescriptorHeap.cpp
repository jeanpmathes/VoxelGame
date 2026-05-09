#include "stdafx.h"

DescriptorHeap DescriptorHeap::CreateNew(ComPtr<ID3D12Device5> const& device, UINT const numDescriptors, D3D12_DESCRIPTOR_HEAP_TYPE const type, bool const shaderVisible)
{
    DescriptorHeap heap;
    heap.Create(device, numDescriptors, type, shaderVisible);
    return heap;
}

void DescriptorHeap::Create(
    ComPtr<ID3D12Device5> const&     targetDevice,
    UINT const                       descriptorCount,
    D3D12_DESCRIPTOR_HEAP_TYPE const heapType,
    bool const                       shaderVisible,
    bool const                       copyExisting)
{
    ComPtr<ID3D12DescriptorHeap> const oldHeap           = heap;
    UINT const                         oldNumDescriptors = numDescriptors;
    Require(Implies(copyExisting, descriptorCount >= oldNumDescriptors));

    heap = nullptr;

    device         = targetDevice;
    increment      = device->GetDescriptorHandleIncrementSize(heapType);
    numDescriptors = descriptorCount;
    type           = heapType;

    D3D12_DESCRIPTOR_HEAP_DESC description = {};
    description.NumDescriptors             = descriptorCount;
    description.Type                       = heapType;
    description.Flags                      = shaderVisible ? D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE : D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

    TryDo(device->CreateDescriptorHeap(&description, IID_PPV_ARGS(&heap)));

    startCPU = heap->GetCPUDescriptorHandleForHeapStart();
    startGPU = shaderVisible ? heap->GetGPUDescriptorHandleForHeapStart() : D3D12_GPU_DESCRIPTOR_HANDLE{};

    if (copyExisting && oldHeap != nullptr && oldNumDescriptors > 0) device->CopyDescriptorsSimple(
        oldNumDescriptors,
        startCPU,
        oldHeap->GetCPUDescriptorHandleForHeapStart(),
        heapType);
}

D3D12_CPU_DESCRIPTOR_HANDLE DescriptorHeap::GetDescriptorHandleCPU(UINT const index) const
{
    Require(IsCreated());
    return Offset(startCPU, index);
}

D3D12_GPU_DESCRIPTOR_HANDLE DescriptorHeap::GetDescriptorHandleGPU(UINT const index) const
{
    Require(IsCreated());
    return Offset(startGPU, index);
}

ID3D12DescriptorHeap* DescriptorHeap::Get() const
{
    Require(IsCreated());
    return heap.Get();
}

bool DescriptorHeap::IsCreated() const { return heap != nullptr; }

UINT DescriptorHeap::GetDescriptorCount() const
{
    Require(IsCreated());
    return numDescriptors;
}

ID3D12DescriptorHeap** DescriptorHeap::GetAddressOf() { return heap.GetAddressOf(); }

UINT DescriptorHeap::GetIncrement() const { return increment; }

D3D12_CPU_DESCRIPTOR_HANDLE DescriptorHeap::Offset(D3D12_CPU_DESCRIPTOR_HANDLE const handle, UINT const index) const
{
    return CD3DX12_CPU_DESCRIPTOR_HANDLE(handle, static_cast<INT>(index), increment);
}

D3D12_GPU_DESCRIPTOR_HANDLE DescriptorHeap::Offset(D3D12_GPU_DESCRIPTOR_HANDLE const handle, UINT const index) const
{
    return CD3DX12_GPU_DESCRIPTOR_HANDLE(handle, static_cast<INT>(index), increment);
}

void DescriptorHeap::CopyTo(DescriptorHeap const& other, UINT const offset) const
{
    Require(IsCreated());
    Require(other.IsCreated());

    Require(type == other.type);
    Require(other.GetDescriptorCount() >= GetDescriptorCount() + offset);

    device->CopyDescriptorsSimple(GetDescriptorCount(), other.GetDescriptorHandleCPU(offset), GetDescriptorHandleCPU(), type);
}
