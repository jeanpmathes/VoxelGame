#include "stdafx.h"

ShaderResources::ConstantBufferViewDescriptor::ConstantBufferViewDescriptor(
    const D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, const UINT size) : gpuAddress(gpuAddress), size(size)
{
}

ShaderResources::ConstantBufferViewDescriptor::ConstantBufferViewDescriptor(
    const D3D12_CONSTANT_BUFFER_VIEW_DESC* description)
    : gpuAddress(description->BufferLocation), size(description->SizeInBytes)
{
}

void ShaderResources::ConstantBufferViewDescriptor::Create(
    ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    D3D12_CONSTANT_BUFFER_VIEW_DESC description;
    description.BufferLocation = gpuAddress;
    description.SizeInBytes = size;

    device->CreateConstantBufferView(&description, cpuHandle);
}

void ShaderResources::ShaderResourceViewDescriptor::Create(
    ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    device->CreateShaderResourceView(resource.Get(), description, cpuHandle);
}

void ShaderResources::UnorderedAccessViewDescriptor::Create(
    ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    device->CreateUnorderedAccessView(resource.Get(), nullptr, description, cpuHandle);
}

ShaderResources::Table::Entry::Entry(const UINT heapParameterIndex, const UINT inHeapIndex) :
    m_heapParameterIndex(heapParameterIndex), m_inHeapIndex(inHeapIndex)
{
}

bool ShaderResources::Table::Entry::IsValid() const
{
    return m_heapParameterIndex != UINT_MAX && m_inHeapIndex != UINT_MAX;
}

ShaderResources::Table::Entry ShaderResources::Table::Entry::invalid = Entry(UINT_MAX, UINT_MAX);

ShaderResources::Table::Entry ShaderResources::Table::AddConstantBufferView(
    const ShaderLocation location, const UINT count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddUnorderedAccessView(
    const ShaderLocation location, const UINT count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddShaderResourceView(
    const ShaderLocation location, const UINT count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddView(ShaderLocation location, UINT count,
                                                              D3D12_DESCRIPTOR_RANGE_TYPE type)
{
    const UINT offset = m_offsets.back();
    const UINT index = static_cast<UINT>(m_offsets.size()) - 1;

    m_offsets.push_back(offset + count);
    m_heapRanges.push_back({location.reg, count, location.space, type, offset});

    return Entry(m_heap, index);
}

ShaderResources::Table::Table(const UINT heap) : m_heap(heap)
{
}

void ShaderResources::Description::AddConstantBufferView(
    const D3D12_GPU_VIRTUAL_ADDRESS gpuAddress,
    const ShaderLocation location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_CBV, RootConstantBufferView{gpuAddress});
}

void ShaderResources::Description::AddShaderResourceView(
    const D3D12_GPU_VIRTUAL_ADDRESS gpuAddress,
    const ShaderLocation location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_SRV, RootShaderResourceView{gpuAddress});
}

void ShaderResources::Description::AddUnorderedAccessView(
    const D3D12_GPU_VIRTUAL_ADDRESS gpuAddress,
    const ShaderLocation location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_UAV, RootUnorderedAccessView{gpuAddress});
}

ShaderResources::TableHandle ShaderResources::Description::AddHeapDescriptorTable(
    const std::function<void(Table&)>& builder)
{
    const UINT handle = static_cast<UINT>(m_rootParameters.size()) + m_existingRootParameterCount;
    Table table(handle);

    builder(table);

    m_heapDescriptorTableCount += table.m_offsets.back();

    m_rootSignatureGenerator.AddHeapRangesParameter(table.m_heapRanges);
    m_rootParameters.push_back(RootHeapDescriptorTable{});
    m_heapDescriptorTableOffsets.push_back(std::move(table.m_offsets));

    return static_cast<TableHandle>(handle);
}

void ShaderResources::Description::AddStaticSampler(const ShaderLocation location)
{
    D3D12_STATIC_SAMPLER_DESC sampler;
    sampler.Filter = D3D12_FILTER_MIN_MAG_MIP_LINEAR;
    sampler.AddressU = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.AddressV = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.AddressW = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.MipLODBias = 0;
    sampler.MaxAnisotropy = 1;
    sampler.ComparisonFunc = D3D12_COMPARISON_FUNC_NEVER;
    sampler.BorderColor = D3D12_STATIC_BORDER_COLOR_TRANSPARENT_BLACK;
    sampler.MinLOD = 0.0f;
    sampler.MaxLOD = D3D12_FLOAT32_MAX;
    sampler.ShaderRegister = location.reg;
    sampler.RegisterSpace = location.space;
    sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;

    m_rootSignatureGenerator.AddStaticSampler(&sampler);
}

void ShaderResources::Description::EnableInputAssembler()
{
    m_rootSignatureGenerator.SetInputAssembler(true);
}

ShaderResources::ListHandle ShaderResources::Description::AddConstantBufferViewDescriptorList(
    const ShaderLocation location,
    SizeGetter count,
    const DescriptorGetter<ConstantBufferViewDescriptor>& descriptor,
    ListBuilder builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), false);
}

ShaderResources::ListHandle ShaderResources::Description::AddShaderResourceViewDescriptorList(
    const ShaderLocation location,
    SizeGetter count,
    const DescriptorGetter<ShaderResourceViewDescriptor>& descriptor,
    ListBuilder builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), false);
}

ShaderResources::ListHandle ShaderResources::Description::AddUnorderedAccessViewDescriptorList(
    const ShaderLocation location,
    SizeGetter count,
    const DescriptorGetter<UnorderedAccessViewDescriptor>& descriptor,
    ListBuilder builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), false);
}

ShaderResources::SelectionList<ShaderResources::ConstantBufferViewDescriptor>
ShaderResources::Description::AddConstantBufferViewDescriptorSelectionList(const ShaderLocation location)
{
    return AddSelectionList<ConstantBufferViewDescriptor>(location);
}

ShaderResources::SelectionList<ShaderResources::ShaderResourceViewDescriptor>
ShaderResources::Description::AddShaderResourceViewDescriptorSelectionList(const ShaderLocation location)
{
    return AddSelectionList<ShaderResourceViewDescriptor>(location);
}

ShaderResources::SelectionList<ShaderResources::UnorderedAccessViewDescriptor>
ShaderResources::Description::AddUnorderedAccessViewDescriptorSelectionList(const ShaderLocation location)
{
    return AddSelectionList<UnorderedAccessViewDescriptor>(location);
}

void ShaderResources::Description::AddRootParameter(
    const ShaderLocation location,
    const D3D12_ROOT_PARAMETER_TYPE type,
    RootParameter&& parameter)
{
    m_rootSignatureGenerator.AddRootParameter(type, location.reg, location.space);
    m_rootParameters.emplace_back(std::move(parameter));
}

ComPtr<ID3D12RootSignature> ShaderResources::Description::GenerateRootSignature(ComPtr<ID3D12Device> device)
{
    return m_rootSignatureGenerator.Generate(device, false);
}

ShaderResources::Description::Description(const UINT existingRootParameterCount) : m_existingRootParameterCount(
    existingRootParameterCount)
{
}

void ShaderResources::Initialize(const Builder& graphics, const Builder& compute, ComPtr<ID3D12Device5> device)
{
    m_device = device;

    Description graphicsDescription(0);
    graphics(graphicsDescription);

    Description computeDescription(static_cast<UINT>(graphicsDescription.m_rootParameters.size()));
    compute(computeDescription);

    m_graphicsRootSignature = graphicsDescription.GenerateRootSignature(device);
    m_graphicsRootParameters = std::move(graphicsDescription.m_rootParameters);
    NAME_D3D12_OBJECT(m_graphicsRootSignature);

    m_computeRootSignature = computeDescription.GenerateRootSignature(device);
    m_computeRootParameters = std::move(computeDescription.m_rootParameters);
    NAME_D3D12_OBJECT(m_computeRootSignature);

    m_totalTableDescriptorCount = graphicsDescription.m_heapDescriptorTableCount + computeDescription.
        m_heapDescriptorTableCount;

    auto initializeDescriptorTables = [&](
        std::vector<RootParameter>& rootParameters,
        const std::vector<std::vector<UINT>>& internalOffsets,
        UINT* externalOffset)
    {
        UINT tableIndex = 0;

        for (auto& parameter : rootParameters)
        {
            if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
            {
                const UINT size = internalOffsets[tableIndex].back();

                auto& tableParameter = std::get<RootHeapDescriptorTable>(parameter);
                tableParameter.index = static_cast<UINT>(m_descriptorTables.size());

                auto& tableData = m_descriptorTables.emplace_back();
                tableData.heap.Create(device, size, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, false);
                tableData.parameter = &tableParameter;
                tableData.internalOffsets = std::move(internalOffsets[tableIndex]);
                tableData.externalOffset = *externalOffset;

                NAME_D3D12_OBJECT(tableData.heap);

                *externalOffset += size;
                tableIndex++;
            }
        }
    };

    m_totalTableOffset = 0;

    initializeDescriptorTables(m_graphicsRootParameters, graphicsDescription.m_heapDescriptorTableOffsets,
                               &m_totalTableOffset);
    initializeDescriptorTables(m_computeRootParameters, computeDescription.m_heapDescriptorTableOffsets,
                               &m_totalTableOffset);

    auto initializeDescriptorLists = [&](
        std::vector<RootParameter>& rootParameters,
        const auto& descriptions)
    {
        UINT listIndex = 0;

        for (auto& parameter : rootParameters)
        {
            if (std::holds_alternative<RootHeapDescriptorList>(parameter))
            {
                auto& listParameter = std::get<RootHeapDescriptorList>(parameter);
                listParameter.index = static_cast<UINT>(m_descriptorLists.size());

                auto& description = descriptions[listIndex];
                
                auto& listData = m_descriptorLists.emplace_back();
                listData.sizeGetter = description.sizeGetter;
                listData.descriptorAssigner = description.descriptorAssigner;
                listData.listBuilder = description.listBuilder;
                listData.parameter = &listParameter;

                listParameter.isSelectionList = description.isSelectionList;

                listIndex++;
            }
        }
    };

    initializeDescriptorLists(m_graphicsRootParameters, graphicsDescription.m_descriptorListDescriptions);
    initializeDescriptorLists(m_computeRootParameters, computeDescription.m_descriptorListDescriptions);

    Update();
}

bool ShaderResources::IsInitialized() const
{
    return m_device != nullptr;
}

ComPtr<ID3D12RootSignature> ShaderResources::GetGraphicsRootSignature() const
{
    return m_graphicsRootSignature;
}

ComPtr<ID3D12RootSignature> ShaderResources::GetComputeRootSignature() const
{
    return m_computeRootSignature;
}

void ShaderResources::RequestListRefresh(ListHandle listHandle, const IntegerSet<>& indices)
{
    REQUIRE(listHandle != ListHandle::INVALID);

    const UINT parameterIndex = static_cast<UINT>(listHandle);
    const RootParameter& parameter = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorList>(parameter))
    {
        auto& list = m_descriptorLists[std::get<RootHeapDescriptorList>(parameter).index];
        list.dirtyIndices = indices;
    }
    else
    {
        REQUIRE(FALSE);
    }
}

void ShaderResources::Bind(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    if (m_cpuDescriptorHeapDirty)
    {
        m_cpuDescriptorHeap.CopyTo(m_gpuDescriptorHeap, 0);
        m_cpuDescriptorHeapDirty = false;
    }
    
    commandList->SetGraphicsRootSignature(m_graphicsRootSignature.Get());
    commandList->SetComputeRootSignature(m_computeRootSignature.Get());
    commandList->SetDescriptorHeaps(1, m_gpuDescriptorHeap.GetAddressOf());

    for (size_t parameterIndex = 0; parameterIndex < m_graphicsRootParameters.size(); ++parameterIndex)
    {
        auto& parameter = m_graphicsRootParameters[parameterIndex];

        std::visit([&]<typename Arg>(Arg& arg)
        {
            using T = std::decay_t<Arg>;

            if constexpr (std::is_same_v<T, RootConstantBufferView>)
            {
                const auto& [gpuAddress] = arg;
                commandList->SetGraphicsRootConstantBufferView(
                    static_cast<UINT>(parameterIndex),
                    gpuAddress);
            }
            else if constexpr (std::is_same_v<T, RootShaderResourceView>)
            {
                const auto& [gpuAddress] = arg;
                commandList->SetGraphicsRootShaderResourceView(
                    static_cast<UINT>(parameterIndex),
                    gpuAddress);
            }
            else if constexpr (std::is_same_v<T, RootUnorderedAccessView>)
            {
                const auto& [gpuAddress] = arg;
                commandList->SetGraphicsRootUnorderedAccessView(
                    static_cast<UINT>(parameterIndex),
                    gpuAddress);
            }
            else if constexpr (std::is_same_v<T, RootHeapDescriptorTable>)
            {
                const auto& [gpuHandle, index] = arg;
                commandList->SetGraphicsRootDescriptorTable(
                    static_cast<UINT>(parameterIndex),
                    gpuHandle);
            }
            else if constexpr (std::is_same_v<T, RootHeapDescriptorList>)
            {
                const auto& [gpuHandle, index, isSelectionList] = arg;

                if (isSelectionList)
                {
                    auto& list = m_descriptorLists[index];

                    list.bind = [parameterIndex, gpuHandle, ptr = &list, increment = m_gpuDescriptorHeap.GetIncrement()
                        ](auto command)
                        {
                            command->SetGraphicsRootDescriptorTable(static_cast<UINT>(parameterIndex),
                                                                    CD3DX12_GPU_DESCRIPTOR_HANDLE(
                                                                        gpuHandle, static_cast<INT>(ptr->selection),
                                                                        increment));
                        };

                    // Intentionally do not bind yet, as last value might not be safe anymore.
                }
                else
                {
                    commandList->SetGraphicsRootDescriptorTable(
                        static_cast<UINT>(parameterIndex),
                        gpuHandle);
                }
            }
            else
            {
                REQUIRE(FALSE);
            }
        }, parameter);
    }

    for (size_t parameterIndex = 0; parameterIndex < m_computeRootParameters.size(); ++parameterIndex)
    {
        auto& parameter = m_computeRootParameters[parameterIndex];

        std::visit([&]<typename Arg>(Arg& arg)
        {
            using T = std::decay_t<Arg>;

            if constexpr (std::is_same_v<T, RootConstantBufferView>)
            {
                const auto& [gpuAddress] = arg;
                commandList->SetComputeRootConstantBufferView(
                    static_cast<UINT>(parameterIndex),
                    gpuAddress);
            }
            else if constexpr (std::is_same_v<T, RootShaderResourceView>)
            {
                const auto& [gpuAddress] = arg;
                commandList->SetComputeRootShaderResourceView(
                    static_cast<UINT>(parameterIndex),
                    gpuAddress);
            }
            else if constexpr (std::is_same_v<T, RootUnorderedAccessView>)
            {
                const auto& [gpuAddress] = arg;
                commandList->SetComputeRootUnorderedAccessView(
                    static_cast<UINT>(parameterIndex),
                    gpuAddress);
            }
            else if constexpr (std::is_same_v<T, RootHeapDescriptorTable>)
            {
                const auto& [gpuHandle, index] = arg;
                commandList->SetComputeRootDescriptorTable(
                    static_cast<UINT>(parameterIndex),
                    gpuHandle);
            }
            else if constexpr (std::is_same_v<T, RootHeapDescriptorList>)
            {
                const auto& [gpuHandle, index, isSelectionList] = arg;

                if (isSelectionList)
                {
                    auto& list = m_descriptorLists[index];

                    list.bind = [parameterIndex, gpuHandle, ptr = &list, increment = m_gpuDescriptorHeap.GetIncrement()
                        ](auto command)
                        {
                            command->SetComputeRootDescriptorTable(static_cast<UINT>(parameterIndex),
                                                                   CD3DX12_GPU_DESCRIPTOR_HANDLE(
                                                                       gpuHandle, static_cast<INT>(ptr->selection),
                                                                       increment));
                        };

                    // Intentionally do not bind yet, as last value might not be safe anymore.
                }
                else
                {
                    commandList->SetComputeRootDescriptorTable(
                        static_cast<UINT>(parameterIndex),
                        gpuHandle);
                }
            }
            else
            {
                REQUIRE(FALSE);
            }
        }, parameter);
    }
}

void ShaderResources::Update()
{
    UINT indexOfFirstResizedList, totalListDescriptorCount;
    const bool resized = CheckListSizeUpdate(&indexOfFirstResizedList, &totalListDescriptorCount);

    if (resized || !m_cpuDescriptorHeap.IsCreated() || !m_gpuDescriptorHeap.IsCreated())
    {
        PerformSizeUpdate(indexOfFirstResizedList, totalListDescriptorCount);

        for (auto& table : m_descriptorTables)
        {
            table.heap.CopyTo(m_cpuDescriptorHeap, table.externalOffset);
        }

        m_cpuDescriptorHeapDirty = true;
    }
    
    const UINT maxIndexOfListsToUpdate =
        resized ? indexOfFirstResizedList : static_cast<UINT>(m_descriptorLists.size());
    for (UINT listIndex = 0; listIndex < maxIndexOfListsToUpdate; ++listIndex)
    {
        auto& list = m_descriptorLists[listIndex];

        if (!list.dirtyIndices.IsEmpty())
        {
            for (const size_t index : list.dirtyIndices)
            {
                const UINT offset = list.externalOffset + static_cast<UINT>(index);
                list.descriptorAssigner(m_device.Get(), static_cast<UINT>(index),
                                        m_cpuDescriptorHeap.GetDescriptorHandleCPU(offset));
            }

            m_cpuDescriptorHeapDirty = true;
        }

        list.dirtyIndices.Clear();
    }
}

void ShaderResources::CreateConstantBufferView(
    Table::Entry entry,
    const UINT offset,
    const ConstantBufferViewDescriptor& descriptor) const
{
    REQUIRE(entry.IsValid());

    auto& [parameterIndex, inHeapIndex] = entry;
    const auto& parameter = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        D3D12_CONSTANT_BUFFER_VIEW_DESC description;
        description.BufferLocation = descriptor.gpuAddress;
        description.SizeInBytes = descriptor.size;

        const auto handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles)
        {
            m_device->CreateConstantBufferView(&description, handle);
        }
    }
    else
    {
        REQUIRE(FALSE);
    }
}

void ShaderResources::CreateShaderResourceView(
    Table::Entry entry,
    const UINT offset,
    const ShaderResourceViewDescriptor& descriptor) const
{
    REQUIRE(entry.IsValid());

    auto& [parameterIndex, inHeapIndex] = entry;
    const auto& parameter = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        const auto handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles)
        {
            m_device->CreateShaderResourceView(descriptor.resource.Get(), descriptor.description, handle);
        }
    }
    else
    {
        REQUIRE(FALSE);
    }
}

void ShaderResources::CreateUnorderedAccessView(
    Table::Entry entry,
    const UINT offset,
    const UnorderedAccessViewDescriptor& descriptor) const
{
    REQUIRE(entry.IsValid());

    auto& [parameterIndex, inHeapIndex] = entry;
    const auto& parameter = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        const auto handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles)
        {
            m_device->CreateUnorderedAccessView(descriptor.resource.Get(), nullptr, descriptor.description, handle);
        }
    }
    else
    {
        REQUIRE(FALSE);
    }
}

const ShaderResources::RootParameter& ShaderResources::GetRootParameter(const UINT index) const
{
    REQUIRE(index < m_graphicsRootParameters.size() + m_computeRootParameters.size());

    return m_graphicsRootParameters.size() > index
               ? m_graphicsRootParameters[index]
               : m_computeRootParameters[index - m_graphicsRootParameters.size()];
}

std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> ShaderResources::GetDescriptorHandlesForWrite(
    const RootParameter& parameter, const UINT inHeapIndex, const UINT offset) const
{
    const UINT descriptorTableIndex = std::get<RootHeapDescriptorTable>(parameter).index;
    const auto& table = m_descriptorTables[descriptorTableIndex];

    const UINT baseOffsetInSecondaryHeap = table.internalOffsets[inHeapIndex];
    const UINT totalOffsetInSecondaryHeap = baseOffsetInSecondaryHeap + offset;

    const UINT totalOffsetInPrimaryHeap = table.externalOffset + totalOffsetInSecondaryHeap;

    std::vector handles = {
        m_cpuDescriptorHeap.GetDescriptorHandleCPU(totalOffsetInPrimaryHeap),
        m_gpuDescriptorHeap.GetDescriptorHandleCPU(totalOffsetInPrimaryHeap),
        table.heap.GetDescriptorHandleCPU(totalOffsetInSecondaryHeap)
    };

    return handles;
}

bool ShaderResources::CheckListSizeUpdate(UINT* firstResizedList, UINT* totalListDescriptorCount)
{
    *firstResizedList = UINT_MAX;
    *totalListDescriptorCount = 0;

    for (UINT index = 0; index < m_descriptorLists.size(); ++index)
    {
        auto& list = m_descriptorLists[index];
        const UINT requiredSize = list.sizeGetter();

        if (list.size < requiredSize || list.size == 0)
        {
            do
            {
                list.size = std::max(4u, list.size * 2u);
            }
            while (list.size < requiredSize);

            *firstResizedList = std::min(*firstResizedList, index);
        }

        *totalListDescriptorCount += list.size;
    }

    return *firstResizedList != UINT_MAX;
}

void ShaderResources::PerformSizeUpdate(const UINT firstResizedListIndex, const UINT totalListDescriptorCount)
{
    const UINT totalDescriptorCount = m_totalTableDescriptorCount + totalListDescriptorCount;

    m_cpuDescriptorHeap.Create(
        m_device, totalDescriptorCount,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
        false, true);
    NAME_D3D12_OBJECT(m_cpuDescriptorHeap);

    m_gpuDescriptorHeap.Create(
        m_device, totalDescriptorCount,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
        true, false);
    NAME_D3D12_OBJECT(m_gpuDescriptorHeap);

    for (const auto& table : m_descriptorTables)
    {
        table.parameter->gpuHandle = m_gpuDescriptorHeap.GetDescriptorHandleGPU(table.externalOffset);
    }

    UINT externalOffset = m_totalTableOffset;
    for (auto& list : m_descriptorLists)
    {
        list.externalOffset = externalOffset;
        list.parameter->gpuHandle = m_gpuDescriptorHeap.GetDescriptorHandleGPU(list.externalOffset);
        
        if (list.parameter->index >= firstResizedListIndex)
        {
            auto assigner = list.descriptorAssigner;
            auto builder = [this, externalOffset, assigner](const UINT index)
            {
                const UINT internalOffset = externalOffset + index;
                assigner(m_device.Get(), index, m_cpuDescriptorHeap.GetDescriptorHandleCPU(internalOffset));
            };

            list.listBuilder(builder);
        }

        externalOffset += list.size;
    }
}
