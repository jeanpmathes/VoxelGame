#include "stdafx.h"

ShaderResources::ConstantBufferViewDescriptor::ConstantBufferViewDescriptor(D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, UINT const size)
    : gpuAddress(gpuAddress)
  , size(size)
{
}

ShaderResources::ConstantBufferViewDescriptor::ConstantBufferViewDescriptor(D3D12_CONSTANT_BUFFER_VIEW_DESC const* description)
    : gpuAddress(description->BufferLocation)
  , size(description->SizeInBytes)
{
}

void ShaderResources::ConstantBufferViewDescriptor::Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    D3D12_CONSTANT_BUFFER_VIEW_DESC description;
    description.BufferLocation = gpuAddress;
    description.SizeInBytes    = size;

    device->CreateConstantBufferView(&description, cpuHandle);
}

void ShaderResources::ShaderResourceViewDescriptor::Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    device->CreateShaderResourceView(resource.Get(), description, cpuHandle);
}

void ShaderResources::UnorderedAccessViewDescriptor::Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    device->CreateUnorderedAccessView(resource.Get(), nullptr, description, cpuHandle);
}

ShaderResources::Table::Entry::Entry(UINT const heapParameterIndex, UINT const inHeapIndex)
    : heapParameterIndex(heapParameterIndex)
  , inHeapIndex(inHeapIndex)
{
}

bool ShaderResources::Table::Entry::IsValid() const
{
    return heapParameterIndex != UINT_MAX && inHeapIndex != UINT_MAX;
}

ShaderResources::Table::Entry ShaderResources::Table::Entry::invalid = Entry(UINT_MAX, UINT_MAX);

ShaderResources::Table::Entry ShaderResources::Table::AddConstantBufferView(ShaderLocation const location, UINT const count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddUnorderedAccessView(ShaderLocation const location, UINT const count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddShaderResourceView(ShaderLocation const location, UINT const count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddView(ShaderLocation location, UINT count, D3D12_DESCRIPTOR_RANGE_TYPE type)
{
    UINT const offset = offsets.back();
    auto const index  = static_cast<UINT>(offsets.size()) - 1;

    offsets.push_back(offset + count);
    heapRanges.push_back({location.reg, count, location.space, type, offset});

    return Entry(heap, index);
}

ShaderResources::Table::Table(UINT const heap)
    : heap(heap)
{
}

ShaderResources::ConstantHandle ShaderResources::Description::AddRootConstant(std::function<Value32()> const& getter, ShaderLocation const location)
{
    auto const handle = static_cast<UINT>(rootParameters.size()) + existingRootParameterCount;

    rootSignatureGenerator.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_32BIT_CONSTANTS, location.reg, location.space, 1);
    rootParameters.emplace_back(RootConstant{});
    rootConstants.push_back(getter);

    return static_cast<ConstantHandle>(handle);
}

void ShaderResources::Description::AddConstantBufferView(D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, ShaderLocation const location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_CBV, RootConstantBufferView{gpuAddress});
}

void ShaderResources::Description::AddShaderResourceView(D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, ShaderLocation const location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_SRV, RootShaderResourceView{gpuAddress});
}

void ShaderResources::Description::AddUnorderedAccessView(D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, ShaderLocation const location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_UAV, RootUnorderedAccessView{gpuAddress});
}

void ShaderResources::Description::AddStaticSampler(ShaderLocation const location, D3D12_FILTER const filter, D3D12_TEXTURE_ADDRESS_MODE const mode, UINT const maxAnisotropy)
{
    D3D12_STATIC_SAMPLER_DESC sampler;
    sampler.Filter           = filter;
    sampler.AddressU         = mode;
    sampler.AddressV         = mode;
    sampler.AddressW         = mode;
    sampler.MipLODBias       = 0;
    sampler.MaxAnisotropy    = maxAnisotropy;
    sampler.ComparisonFunc   = D3D12_COMPARISON_FUNC_NEVER;
    sampler.BorderColor      = D3D12_STATIC_BORDER_COLOR_TRANSPARENT_BLACK;
    sampler.MinLOD           = 0.0f;
    sampler.MaxLOD           = D3D12_FLOAT32_MAX;
    sampler.ShaderRegister   = location.reg;
    sampler.RegisterSpace    = location.space;
    sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;

    rootSignatureGenerator.AddStaticSampler(&sampler);
}

void ShaderResources::Description::EnableInputAssembler() { rootSignatureGenerator.SetInputAssembler(true); }

ShaderResources::ListHandle ShaderResources::Description::AddConstantBufferViewDescriptorList(
    ShaderLocation const                                  location,
    SizeGetter                                            count,
    DescriptorGetter<ConstantBufferViewDescriptor> const& descriptor,
    ListBuilder                                           builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), std::nullopt);
}

ShaderResources::ListHandle ShaderResources::Description::AddShaderResourceViewDescriptorList(
    ShaderLocation const                                  location,
    SizeGetter                                            count,
    DescriptorGetter<ShaderResourceViewDescriptor> const& descriptor,
    ListBuilder                                           builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), std::nullopt);
}

ShaderResources::ListHandle ShaderResources::Description::AddUnorderedAccessViewDescriptorList(
    ShaderLocation const                                   location,
    SizeGetter                                             count,
    DescriptorGetter<UnorderedAccessViewDescriptor> const& descriptor,
    ListBuilder                                            builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), std::nullopt);
}

ShaderResources::SelectionList<ShaderResources::ConstantBufferViewDescriptor>
ShaderResources::Description::AddConstantBufferViewDescriptorSelectionList(ShaderLocation const location, UINT const window)
{
    return AddSelectionList<ConstantBufferViewDescriptor>(location, window);
}

ShaderResources::SelectionList<ShaderResources::ShaderResourceViewDescriptor>
ShaderResources::Description::AddShaderResourceViewDescriptorSelectionList(ShaderLocation const location, UINT const window)
{
    return AddSelectionList<ShaderResourceViewDescriptor>(location, window);
}

ShaderResources::SelectionList<ShaderResources::UnorderedAccessViewDescriptor>
ShaderResources::Description::AddUnorderedAccessViewDescriptorSelectionList(ShaderLocation const location, UINT const window)
{
    return AddSelectionList<UnorderedAccessViewDescriptor>(location, window);
}

void ShaderResources::Description::AddRootParameter(ShaderLocation const location, D3D12_ROOT_PARAMETER_TYPE const type, RootParameter parameter)
{
    rootSignatureGenerator.AddRootParameter(type, location.reg, location.space);
    rootParameters.emplace_back(std::move(parameter));
}

ComPtr<ID3D12RootSignature> ShaderResources::Description::GenerateRootSignature(ComPtr<ID3D12Device> const& device)
{
    return rootSignatureGenerator.Generate(device, false);
}

ShaderResources::Description::Description(UINT const existingRootParameterCount)
    : existingRootParameterCount(existingRootParameterCount)
{
}

bool ShaderResources::IsInitialized() const { return device != nullptr; }

ComPtr<ID3D12RootSignature> ShaderResources::GetGraphicsRootSignature() const { return graphicsRootSignature; }

ComPtr<ID3D12RootSignature> ShaderResources::GetComputeRootSignature() const { return computeRootSignature; }

void ShaderResources::RequestListRefresh(ListHandle listHandle, IntegerSet<> const& indices)
{
    Require(listHandle != ListHandle::INVALID);

    auto const           parameterIndex = static_cast<UINT>(listHandle);
    RootParameter const& parameter      = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorList>(parameter))
    {
        auto& list        = descriptorLists[std::get<RootHeapDescriptorList>(parameter).index];
        list.dirtyIndices = indices;
    }
    else Require(FALSE);
}

void ShaderResources::Bind(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    if (cpuDescriptorHeapDirty)
    {
        cpuDescriptorHeap.CopyTo(gpuDescriptorHeap, 0);
        cpuDescriptorHeapDirty = false;
    }

    commandList->SetGraphicsRootSignature(graphicsRootSignature.Get());
    commandList->SetComputeRootSignature(computeRootSignature.Get());
    commandList->SetDescriptorHeaps(1, gpuDescriptorHeap.GetAddressOf());

    for (size_t parameterIndex = 0; parameterIndex < graphicsRootParameters.size(); ++parameterIndex)
    {
        auto& parameter = graphicsRootParameters[parameterIndex];

        std::visit(
            [this, commandList, parameterIndex]<typename Arg>(Arg& arg)
            {
                using T = std::decay_t<Arg>;

                if constexpr (std::is_same_v<T, RootConstant>)
                {
                    auto const& [index, queue] = arg;
                    auto&       constant       = constants[index];

                    commandList->SetGraphicsRoot32BitConstant(static_cast<UINT>(parameterIndex), constant.getter().uInteger, 0);
                }
                else if constexpr (std::is_same_v<T, RootConstantBufferView>)
                {
                    auto const& [gpuAddress] = arg;
                    commandList->SetGraphicsRootConstantBufferView(static_cast<UINT>(parameterIndex), gpuAddress);
                }
                else if constexpr (std::is_same_v<T, RootShaderResourceView>)
                {
                    auto const& [gpuAddress] = arg;
                    commandList->SetGraphicsRootShaderResourceView(static_cast<UINT>(parameterIndex), gpuAddress);
                }
                else if constexpr (std::is_same_v<T, RootUnorderedAccessView>)
                {
                    auto const& [gpuAddress] = arg;
                    commandList->SetGraphicsRootUnorderedAccessView(static_cast<UINT>(parameterIndex), gpuAddress);
                }
                else if constexpr (std::is_same_v<T, RootHeapDescriptorTable>)
                {
                    auto const& [gpuHandle, index] = arg;
                    commandList->SetGraphicsRootDescriptorTable(static_cast<UINT>(parameterIndex), gpuHandle);
                }
                else if constexpr (std::is_same_v<T, RootHeapDescriptorList>)
                {
                    auto const& [gpuHandle, index, isSelectionList] = arg;

                    if (isSelectionList)
                    {
                        auto& list = descriptorLists[index];

                        list.bind = [parameterIndex, gpuHandle, ptr = &list, increment = gpuDescriptorHeap.GetIncrement() ](auto command)
                        {
                            command->SetGraphicsRootDescriptorTable(
                                static_cast<UINT>(parameterIndex),
                                CD3DX12_GPU_DESCRIPTOR_HANDLE(gpuHandle, static_cast<INT>(ptr->selection), increment));
                        };

                        // Intentionally do not bind yet, as last value might not be safe anymore.
                    }
                    else commandList->SetGraphicsRootDescriptorTable(static_cast<UINT>(parameterIndex), gpuHandle);
                }
                else Require(FALSE);
            },
            parameter);
    }

    for (size_t parameterIndex = 0; parameterIndex < computeRootParameters.size(); ++parameterIndex)
    {
        auto& parameter = computeRootParameters[parameterIndex];

        std::visit(
            [this, commandList, parameterIndex]<typename Arg>(Arg& arg)
            {
                using T = std::decay_t<Arg>;

                if constexpr (std::is_same_v<T, RootConstant>)
                {
                    auto const& [index, queue] = arg;
                    auto&       constant       = constants[index];

                    commandList->SetComputeRoot32BitConstant(static_cast<UINT>(parameterIndex), constant.getter().uInteger, 0);
                }
                else if constexpr (std::is_same_v<T, RootConstantBufferView>)
                {
                    auto const& [gpuAddress] = arg;
                    commandList->SetComputeRootConstantBufferView(static_cast<UINT>(parameterIndex), gpuAddress);
                }
                else if constexpr (std::is_same_v<T, RootShaderResourceView>)
                {
                    auto const& [gpuAddress] = arg;
                    commandList->SetComputeRootShaderResourceView(static_cast<UINT>(parameterIndex), gpuAddress);
                }
                else if constexpr (std::is_same_v<T, RootUnorderedAccessView>)
                {
                    auto const& [gpuAddress] = arg;
                    commandList->SetComputeRootUnorderedAccessView(static_cast<UINT>(parameterIndex), gpuAddress);
                }
                else if constexpr (std::is_same_v<T, RootHeapDescriptorTable>)
                {
                    auto const& [gpuHandle, index] = arg;
                    commandList->SetComputeRootDescriptorTable(static_cast<UINT>(parameterIndex), gpuHandle);
                }
                else if constexpr (std::is_same_v<T, RootHeapDescriptorList>)
                {
                    auto const& [gpuHandle, index, isSelectionList] = arg;

                    if (isSelectionList)
                    {
                        auto& list = descriptorLists[index];

                        list.bind = [parameterIndex, gpuHandle, ptr = &list, increment = gpuDescriptorHeap.GetIncrement() ](auto command)
                        {
                            command->SetComputeRootDescriptorTable(
                                static_cast<UINT>(parameterIndex),
                                CD3DX12_GPU_DESCRIPTOR_HANDLE(gpuHandle, static_cast<INT>(ptr->selection), increment));
                        };

                        // Intentionally do not bind yet, as last value might not be safe anymore.
                    }
                    else commandList->SetComputeRootDescriptorTable(static_cast<UINT>(parameterIndex), gpuHandle);
                }
                else Require(FALSE);
            },
            parameter);
    }
}

void ShaderResources::UpdateConstant(ConstantHandle handle, ComPtr<ID3D12GraphicsCommandList> const& commandList) const
{
    auto const& parameterIndex = static_cast<UINT>(handle);
    auto const& parameter      = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootConstant>(parameter))
    {
        auto const& [index, queue]               = std::get<RootConstant>(parameter);
        auto const& [getter, rootParameterIndex] = constants[index];

        Value32 const value = getter();

        if (queue == QueueType::GRAPHICS) commandList->SetGraphicsRoot32BitConstant(rootParameterIndex, value.uInteger, 0);
        else if (queue == QueueType::COMPUTE) commandList->SetComputeRoot32BitConstant(rootParameterIndex, value.uInteger, 0);
        else Require(FALSE);
    }
    else Require(FALSE);
}

void ShaderResources::Update()
{
    UINT indexOfFirstResizedList;
    UINT totalListDescriptorCount;

    bool const resized = CheckListSizeUpdate(&indexOfFirstResizedList, &totalListDescriptorCount);

    if (resized || !cpuDescriptorHeap.IsCreated() || !gpuDescriptorHeap.IsCreated())
    {
        PerformSizeUpdate(indexOfFirstResizedList, totalListDescriptorCount);

        for (auto const& table : descriptorTables) table.heap.CopyTo(cpuDescriptorHeap, table.externalOffset);

        cpuDescriptorHeapDirty = true;
    }

    UINT const maxIndexOfListsToUpdate = resized ? indexOfFirstResizedList : static_cast<UINT>(descriptorLists.size());
    for (UINT listIndex = 0; listIndex < maxIndexOfListsToUpdate; ++listIndex)
    {
        auto& list = descriptorLists[listIndex];

        if (!list.dirtyIndices.IsEmpty())
        {
            for (size_t const index : list.dirtyIndices)
            {
                UINT const offset = list.externalOffset + static_cast<UINT>(index);
                list.descriptorAssigner(device.Get(), static_cast<UINT>(index), cpuDescriptorHeap.GetDescriptorHandleCPU(offset));
            }

            cpuDescriptorHeapDirty = true;
        }

        list.dirtyIndices.Clear();
    }
}

void ShaderResources::CreateConstantBufferView(Table::Entry entry, UINT const offset, ConstantBufferViewDescriptor const& descriptor) const
{
    Require(entry.IsValid());

    auto const& [parameterIndex, inHeapIndex] = entry;
    auto const& parameter                     = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        D3D12_CONSTANT_BUFFER_VIEW_DESC description;
        description.BufferLocation = descriptor.gpuAddress;
        description.SizeInBytes    = descriptor.size;

        auto const handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles) device->CreateConstantBufferView(&description, handle);
    }
    else Require(FALSE);
}

void ShaderResources::CreateShaderResourceView(Table::Entry entry, UINT const offset, ShaderResourceViewDescriptor const& descriptor) const
{
    Require(entry.IsValid());

    auto const& [parameterIndex, inHeapIndex] = entry;
    auto const& parameter                     = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        auto const handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles) device->CreateShaderResourceView(descriptor.resource.Get(), descriptor.description, handle);
    }
    else Require(FALSE);
}

void ShaderResources::CreateUnorderedAccessView(Table::Entry entry, UINT const offset, UnorderedAccessViewDescriptor const& descriptor) const
{
    Require(entry.IsValid());

    auto const& [parameterIndex, inHeapIndex] = entry;
    auto const& parameter                     = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        auto const handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto const& handle : handles) device->CreateUnorderedAccessView(descriptor.resource.Get(), nullptr, descriptor.description, handle);
    }
    else Require(FALSE);
}

ShaderResources::RootParameter const& ShaderResources::GetRootParameter(UINT const index) const
{
    Require(index < graphicsRootParameters.size() + computeRootParameters.size());

    return graphicsRootParameters.size() > index ? graphicsRootParameters[index] : computeRootParameters[index - graphicsRootParameters.size()];
}

std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> ShaderResources::GetDescriptorHandlesForWrite(RootParameter const& parameter, UINT const inHeapIndex, UINT const offset) const
{
    UINT const  descriptorTableIndex = std::get<RootHeapDescriptorTable>(parameter).index;
    auto const& table                = descriptorTables[descriptorTableIndex];

    UINT const baseOffsetInSecondaryHeap  = table.internalOffsets[inHeapIndex];
    UINT const totalOffsetInSecondaryHeap = baseOffsetInSecondaryHeap + offset;

    UINT const totalOffsetInPrimaryHeap = table.externalOffset + totalOffsetInSecondaryHeap;

    std::vector handles = {
        cpuDescriptorHeap.GetDescriptorHandleCPU(totalOffsetInPrimaryHeap),
        gpuDescriptorHeap.GetDescriptorHandleCPU(totalOffsetInPrimaryHeap),
        table.heap.GetDescriptorHandleCPU(totalOffsetInSecondaryHeap)
    };

    return handles;
}

bool ShaderResources::CheckListSizeUpdate(UINT* firstResizedList, UINT* totalListDescriptorCount)
{
    *firstResizedList         = UINT_MAX;
    *totalListDescriptorCount = 0;

    for (UINT index = 0; index < descriptorLists.size(); ++index)
    {
        auto& list = descriptorLists[index];

        if (UINT const requiredSize = list.sizeGetter();
            list.size < requiredSize || list.size == 0)
        {
            do { list.size = std::max(4u, list.size * 2u); }
            while (list.size < requiredSize);

            *firstResizedList = std::min(*firstResizedList, index);
        }

        *totalListDescriptorCount += list.size;
    }

    return *firstResizedList != UINT_MAX;
}

void ShaderResources::PerformSizeUpdate(UINT const firstResizedListIndex, UINT const totalListDescriptorCount)
{
    UINT const totalDescriptorCount = totalTableDescriptorCount + totalListDescriptorCount;

    cpuDescriptorHeap.Create(device, totalDescriptorCount, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, false, true);
    NAME_D3D12_OBJECT(cpuDescriptorHeap);

    gpuDescriptorHeap.Create(device, totalDescriptorCount, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true, false);
    NAME_D3D12_OBJECT(gpuDescriptorHeap);

    for (auto const& table : descriptorTables) table.parameter->gpuHandle = gpuDescriptorHeap.GetDescriptorHandleGPU(table.externalOffset);

    UINT externalOffset = totalTableOffset;
    for (auto& list : descriptorLists)
    {
        list.externalOffset       = externalOffset;
        list.parameter->gpuHandle = gpuDescriptorHeap.GetDescriptorHandleGPU(list.externalOffset);

        if (list.parameter->index >= firstResizedListIndex)
        {
            auto const& assigner = list.descriptorAssigner;
            auto        builder  = [this, externalOffset, assigner](UINT const index)
            {
                UINT const internalOffset = externalOffset + index;
                assigner(device.Get(), index, cpuDescriptorHeap.GetDescriptorHandleCPU(internalOffset));
            };

            list.listBuilder(builder);
        }

        externalOffset += list.size;
    }
}
