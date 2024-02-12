#include "stdafx.h"

ShaderResources::ConstantBufferViewDescriptor::ConstantBufferViewDescriptor(
    D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, UINT const size)
    : gpuAddress(gpuAddress)
  , size(size)
{
}

ShaderResources::ConstantBufferViewDescriptor::ConstantBufferViewDescriptor(
    D3D12_CONSTANT_BUFFER_VIEW_DESC const* description)
    : gpuAddress(description->BufferLocation)
  , size(description->SizeInBytes)
{
}

void ShaderResources::ConstantBufferViewDescriptor::Create(
    ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const
{
    D3D12_CONSTANT_BUFFER_VIEW_DESC description;
    description.BufferLocation = gpuAddress;
    description.SizeInBytes    = size;

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

ShaderResources::Table::Entry::Entry(UINT const heapParameterIndex, UINT const inHeapIndex)
    : m_heapParameterIndex(heapParameterIndex)
  , m_inHeapIndex(inHeapIndex)
{
}

bool ShaderResources::Table::Entry::IsValid() const
{
    return m_heapParameterIndex != UINT_MAX && m_inHeapIndex != UINT_MAX;
}

ShaderResources::Table::Entry ShaderResources::Table::Entry::invalid = Entry(UINT_MAX, UINT_MAX);

ShaderResources::Table::Entry ShaderResources::Table::AddConstantBufferView(
    ShaderLocation const location, UINT const count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddUnorderedAccessView(
    ShaderLocation const location, UINT const count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddShaderResourceView(
    ShaderLocation const location, UINT const count)
{
    return AddView(location, count, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
}

ShaderResources::Table::Entry ShaderResources::Table::AddView(
    ShaderLocation location, UINT count, D3D12_DESCRIPTOR_RANGE_TYPE type)
{
    UINT const offset = m_offsets.back();
    UINT const index  = static_cast<UINT>(m_offsets.size()) - 1;

    m_offsets.push_back(offset + count);
    m_heapRanges.push_back({location.reg, count, location.space, type, offset});

    return Entry(m_heap, index);
}

ShaderResources::Table::Table(UINT const heap)
    : m_heap(heap)
{
}

ShaderResources::ConstantHandle ShaderResources::Description::AddRootConstant(
    std::function<Value32()> const& getter, ShaderLocation const location)
{
    UINT const handle = static_cast<UINT>(m_rootParameters.size()) + m_existingRootParameterCount;

    m_rootSignatureGenerator.AddRootParameter(
        D3D12_ROOT_PARAMETER_TYPE_32BIT_CONSTANTS,
        location.reg,
        location.space,
        1);
    m_rootParameters.push_back(RootConstant{});
    m_rootConstants.push_back(getter);

    return static_cast<ConstantHandle>(handle);
}

void ShaderResources::Description::AddConstantBufferView(
    D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, ShaderLocation const location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_CBV, RootConstantBufferView{gpuAddress});
}

void ShaderResources::Description::AddShaderResourceView(
    D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, ShaderLocation const location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_SRV, RootShaderResourceView{gpuAddress});
}

void ShaderResources::Description::AddUnorderedAccessView(
    D3D12_GPU_VIRTUAL_ADDRESS const gpuAddress, ShaderLocation const location)
{
    AddRootParameter(location, D3D12_ROOT_PARAMETER_TYPE_UAV, RootUnorderedAccessView{gpuAddress});
}

ShaderResources::TableHandle ShaderResources::Description::AddHeapDescriptorTable(
    std::function<void(Table&)> const& builder)
{
    UINT const handle = static_cast<UINT>(m_rootParameters.size()) + m_existingRootParameterCount;
    Table      table(handle);

    builder(table);

    m_heapDescriptorTableCount += table.m_offsets.back();

    m_rootSignatureGenerator.AddHeapRangesParameter(table.m_heapRanges);
    m_rootParameters.push_back(RootHeapDescriptorTable{});
    m_heapDescriptorTableOffsets.push_back(std::move(table.m_offsets));

    return static_cast<TableHandle>(handle);
}

void ShaderResources::Description::AddStaticSampler(ShaderLocation const location, D3D12_FILTER const filter)
{
    D3D12_STATIC_SAMPLER_DESC sampler;
    sampler.Filter           = filter;
    sampler.AddressU         = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.AddressV         = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.AddressW         = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
    sampler.MipLODBias       = 0;
    sampler.MaxAnisotropy    = 1;
    sampler.ComparisonFunc   = D3D12_COMPARISON_FUNC_NEVER;
    sampler.BorderColor      = D3D12_STATIC_BORDER_COLOR_TRANSPARENT_BLACK;
    sampler.MinLOD           = 0.0f;
    sampler.MaxLOD           = D3D12_FLOAT32_MAX;
    sampler.ShaderRegister   = location.reg;
    sampler.RegisterSpace    = location.space;
    sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;

    m_rootSignatureGenerator.AddStaticSampler(&sampler);
}

void ShaderResources::Description::EnableInputAssembler() { m_rootSignatureGenerator.SetInputAssembler(true); }

ShaderResources::ListHandle ShaderResources::Description::AddConstantBufferViewDescriptorList(
    ShaderLocation const location, SizeGetter count, DescriptorGetter<ConstantBufferViewDescriptor> const& descriptor,
    ListBuilder          builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), std::nullopt);
}

ShaderResources::ListHandle ShaderResources::Description::AddShaderResourceViewDescriptorList(
    ShaderLocation const location, SizeGetter count, DescriptorGetter<ShaderResourceViewDescriptor> const& descriptor,
    ListBuilder          builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), std::nullopt);
}

ShaderResources::ListHandle ShaderResources::Description::AddUnorderedAccessViewDescriptorList(
    ShaderLocation const location, SizeGetter count, DescriptorGetter<UnorderedAccessViewDescriptor> const& descriptor,
    ListBuilder          builder)
{
    return AddDescriptorList(location, std::move(count), descriptor, std::move(builder), std::nullopt);
}

ShaderResources::SelectionList<ShaderResources::ConstantBufferViewDescriptor>
ShaderResources::Description::AddConstantBufferViewDescriptorSelectionList(
    ShaderLocation const location, UINT const window)
{
    return AddSelectionList<ConstantBufferViewDescriptor>(location, window);
}

ShaderResources::SelectionList<ShaderResources::ShaderResourceViewDescriptor>
ShaderResources::Description::AddShaderResourceViewDescriptorSelectionList(
    ShaderLocation const location, UINT const window)
{
    return AddSelectionList<ShaderResourceViewDescriptor>(location, window);
}

ShaderResources::SelectionList<ShaderResources::UnorderedAccessViewDescriptor>
ShaderResources::Description::AddUnorderedAccessViewDescriptorSelectionList(
    ShaderLocation const location, UINT const window)
{
    return AddSelectionList<UnorderedAccessViewDescriptor>(location, window);
}

void ShaderResources::Description::AddRootParameter(
    ShaderLocation const location, D3D12_ROOT_PARAMETER_TYPE const type, RootParameter&& parameter)
{
    m_rootSignatureGenerator.AddRootParameter(type, location.reg, location.space);
    m_rootParameters.emplace_back(std::move(parameter));
}

ComPtr<ID3D12RootSignature> ShaderResources::Description::GenerateRootSignature(ComPtr<ID3D12Device> device)
{
    return m_rootSignatureGenerator.Generate(device, false);
}

ShaderResources::Description::Description(UINT const existingRootParameterCount)
    : m_existingRootParameterCount(existingRootParameterCount)
{
}

void ShaderResources::Initialize(Builder const& graphics, Builder const& compute, ComPtr<ID3D12Device5> device)
{
    m_device = device;

    UINT        rootParameterCount = 0;
    Description graphicsDescription(rootParameterCount);
    graphics(graphicsDescription);

    rootParameterCount = static_cast<UINT>(graphicsDescription.m_rootParameters.size());
    Description computeDescription(rootParameterCount);
    compute(computeDescription);

    m_graphicsRootSignature  = graphicsDescription.GenerateRootSignature(device);
    m_graphicsRootParameters = std::move(graphicsDescription.m_rootParameters);
    NAME_D3D12_OBJECT(m_graphicsRootSignature);

    m_computeRootSignature  = computeDescription.GenerateRootSignature(device);
    m_computeRootParameters = std::move(computeDescription.m_rootParameters);
    NAME_D3D12_OBJECT(m_computeRootSignature);

    auto                            initializeConstants = [&](
        std::vector<RootParameter>& rootParameters, std::vector<std::function<Value32()>>&& getters)
    {
        UINT constantIndex = 0;

        for (auto& parameter : rootParameters)
            if (std::holds_alternative<RootConstant>(parameter))
            {
                auto& [index] = std::get<RootConstant>(parameter);
                index         = constantIndex;

                auto& [getter] = m_constants.emplace_back();
                getter         = std::move(getters[constantIndex]);

                constantIndex++;
            }
    };

    initializeConstants(m_graphicsRootParameters, std::move(graphicsDescription.m_rootConstants));
    initializeConstants(m_computeRootParameters, std::move(computeDescription.m_rootConstants));

    m_totalTableDescriptorCount = graphicsDescription.m_heapDescriptorTableCount + computeDescription.
        m_heapDescriptorTableCount;

    auto                            initializeDescriptorTables = [&](
        std::vector<RootParameter>& rootParameters, std::vector<std::vector<UINT>> const& internalOffsets,
        UINT*                       externalOffset)
    {
        UINT tableIndex = 0;

        for (auto& parameter : rootParameters)
            if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
            {
                UINT const size = internalOffsets[tableIndex].back();

                auto& tableParameter = std::get<RootHeapDescriptorTable>(parameter);
                tableParameter.index = static_cast<UINT>(m_descriptorTables.size());

                auto& tableData = m_descriptorTables.emplace_back();
                tableData.heap.Create(device, size, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, false);
                tableData.parameter       = &tableParameter;
                tableData.internalOffsets = std::move(internalOffsets[tableIndex]);
                tableData.externalOffset  = *externalOffset;

                NAME_D3D12_OBJECT(tableData.heap);

                *externalOffset += size;
                tableIndex++;
            }
    };

    m_totalTableOffset = 0;

    initializeDescriptorTables(
        m_graphicsRootParameters,
        graphicsDescription.m_heapDescriptorTableOffsets,
        &m_totalTableOffset);
    initializeDescriptorTables(
        m_computeRootParameters,
        computeDescription.m_heapDescriptorTableOffsets,
        &m_totalTableOffset);

    auto initializeDescriptorLists = [&](std::vector<RootParameter>& rootParameters, auto const& descriptions)
    {
        UINT listIndex = 0;

        for (auto& parameter : rootParameters)
            if (std::holds_alternative<RootHeapDescriptorList>(parameter))
            {
                auto& listParameter = std::get<RootHeapDescriptorList>(parameter);
                listParameter.index = static_cast<UINT>(m_descriptorLists.size());

                auto& description = descriptions[listIndex];

                auto& listData              = m_descriptorLists.emplace_back();
                listData.sizeGetter         = description.sizeGetter;
                listData.descriptorAssigner = description.descriptorAssigner;
                listData.listBuilder        = description.listBuilder;
                listData.parameter          = &listParameter;

                listParameter.isSelectionList = description.isSelectionList;

                listIndex++;
            }
    };

    initializeDescriptorLists(m_graphicsRootParameters, graphicsDescription.m_descriptorListDescriptions);
    initializeDescriptorLists(m_computeRootParameters, computeDescription.m_descriptorListDescriptions);

    Update();
}

bool ShaderResources::IsInitialized() const { return m_device != nullptr; }

ComPtr<ID3D12RootSignature> ShaderResources::GetGraphicsRootSignature() const { return m_graphicsRootSignature; }

ComPtr<ID3D12RootSignature> ShaderResources::GetComputeRootSignature() const { return m_computeRootSignature; }

void ShaderResources::RequestListRefresh(ListHandle listHandle, IntegerSet<> const& indices)
{
    REQUIRE(listHandle != ListHandle::INVALID);

    UINT const           parameterIndex = static_cast<UINT>(listHandle);
    RootParameter const& parameter      = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorList>(parameter))
    {
        auto& list        = m_descriptorLists[std::get<RootHeapDescriptorList>(parameter).index];
        list.dirtyIndices = indices;
    }
    else
        REQUIRE(FALSE);
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

        std::visit(
            [&]<typename Arg>(Arg& arg)
            {
                using T = std::decay_t<Arg>;

                if constexpr (std::is_same_v<T, RootConstant>)
                {
                    auto const& [index]  = arg;
                    auto&       constant = m_constants[index];

                    commandList->SetGraphicsRoot32BitConstant(
                        static_cast<UINT>(parameterIndex),
                        constant.getter().uInteger,
                        0);
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
                        auto& list = m_descriptorLists[index];

                        list.bind = [parameterIndex, gpuHandle, ptr = &list, increment = m_gpuDescriptorHeap.
                                GetIncrement() ](auto command)
                            {
                                command->SetGraphicsRootDescriptorTable(
                                    static_cast<UINT>(parameterIndex),
                                    CD3DX12_GPU_DESCRIPTOR_HANDLE(
                                        gpuHandle,
                                        static_cast<INT>(ptr->selection),
                                        increment));
                            };

                        // Intentionally do not bind yet, as last value might not be safe anymore.
                    }
                    else commandList->SetGraphicsRootDescriptorTable(static_cast<UINT>(parameterIndex), gpuHandle);
                }
                else
                    REQUIRE(FALSE);
            },
            parameter);
    }

    for (size_t parameterIndex = 0; parameterIndex < m_computeRootParameters.size(); ++parameterIndex)
    {
        auto& parameter = m_computeRootParameters[parameterIndex];

        std::visit(
            [&]<typename Arg>(Arg& arg)
            {
                using T = std::decay_t<Arg>;

                if constexpr (std::is_same_v<T, RootConstant>)
                {
                    auto const& [index]  = arg;
                    auto&       constant = m_constants[index];

                    commandList->SetComputeRoot32BitConstant(
                        static_cast<UINT>(parameterIndex),
                        constant.getter().uInteger,
                        0);
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
                        auto& list = m_descriptorLists[index];

                        list.bind = [parameterIndex, gpuHandle, ptr = &list, increment = m_gpuDescriptorHeap.
                                GetIncrement() ](auto command)
                            {
                                command->SetComputeRootDescriptorTable(
                                    static_cast<UINT>(parameterIndex),
                                    CD3DX12_GPU_DESCRIPTOR_HANDLE(
                                        gpuHandle,
                                        static_cast<INT>(ptr->selection),
                                        increment));
                            };

                        // Intentionally do not bind yet, as last value might not be safe anymore.
                    }
                    else commandList->SetComputeRootDescriptorTable(static_cast<UINT>(parameterIndex), gpuHandle);
                }
                else
                    REQUIRE(FALSE);
            },
            parameter);
    }
}

void ShaderResources::Update()
{
    UINT       indexOfFirstResizedList, totalListDescriptorCount;
    bool const resized = CheckListSizeUpdate(&indexOfFirstResizedList, &totalListDescriptorCount);

    if (resized || !m_cpuDescriptorHeap.IsCreated() || !m_gpuDescriptorHeap.IsCreated())
    {
        PerformSizeUpdate(indexOfFirstResizedList, totalListDescriptorCount);

        for (auto& table : m_descriptorTables) table.heap.CopyTo(m_cpuDescriptorHeap, table.externalOffset);

        m_cpuDescriptorHeapDirty = true;
    }

    UINT const maxIndexOfListsToUpdate =
        resized ? indexOfFirstResizedList : static_cast<UINT>(m_descriptorLists.size());
    for (UINT listIndex = 0; listIndex < maxIndexOfListsToUpdate; ++listIndex)
    {
        auto& list = m_descriptorLists[listIndex];

        if (!list.dirtyIndices.IsEmpty())
        {
            for (size_t const index : list.dirtyIndices)
            {
                UINT const offset = list.externalOffset + static_cast<UINT>(index);
                list.descriptorAssigner(
                    m_device.Get(),
                    static_cast<UINT>(index),
                    m_cpuDescriptorHeap.GetDescriptorHandleCPU(offset));
            }

            m_cpuDescriptorHeapDirty = true;
        }

        list.dirtyIndices.Clear();
    }
}

void ShaderResources::CreateConstantBufferView(
    Table::Entry entry, UINT const offset, ConstantBufferViewDescriptor const& descriptor) const
{
    REQUIRE(entry.IsValid());

    auto&       [parameterIndex, inHeapIndex] = entry;
    auto const& parameter                     = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        D3D12_CONSTANT_BUFFER_VIEW_DESC description;
        description.BufferLocation = descriptor.gpuAddress;
        description.SizeInBytes    = descriptor.size;

        auto const handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles) m_device->CreateConstantBufferView(&description, handle);
    }
    else
        REQUIRE(FALSE);
}

void ShaderResources::CreateShaderResourceView(
    Table::Entry entry, UINT const offset, ShaderResourceViewDescriptor const& descriptor) const
{
    REQUIRE(entry.IsValid());

    auto&       [parameterIndex, inHeapIndex] = entry;
    auto const& parameter                     = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        auto const handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles)
            m_device->CreateShaderResourceView(descriptor.resource.Get(), descriptor.description, handle);
    }
    else
        REQUIRE(FALSE);
}

void ShaderResources::CreateUnorderedAccessView(
    Table::Entry entry, UINT const offset, UnorderedAccessViewDescriptor const& descriptor) const
{
    REQUIRE(entry.IsValid());

    auto&       [parameterIndex, inHeapIndex] = entry;
    auto const& parameter                     = GetRootParameter(parameterIndex);

    if (std::holds_alternative<RootHeapDescriptorTable>(parameter))
    {
        auto const handles = GetDescriptorHandlesForWrite(parameter, inHeapIndex, offset);
        for (auto& handle : handles)
            m_device->CreateUnorderedAccessView(descriptor.resource.Get(), nullptr, descriptor.description, handle);
    }
    else
        REQUIRE(FALSE);
}

ShaderResources::RootParameter const& ShaderResources::GetRootParameter(UINT const index) const
{
    REQUIRE(index < m_graphicsRootParameters.size() + m_computeRootParameters.size());

    return m_graphicsRootParameters.size() > index
               ? m_graphicsRootParameters[index]
               : m_computeRootParameters[index - m_graphicsRootParameters.size()];
}

std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> ShaderResources::GetDescriptorHandlesForWrite(
    RootParameter const& parameter, UINT const inHeapIndex, UINT const offset) const
{
    UINT const  descriptorTableIndex = std::get<RootHeapDescriptorTable>(parameter).index;
    auto const& table                = m_descriptorTables[descriptorTableIndex];

    UINT const baseOffsetInSecondaryHeap  = table.internalOffsets[inHeapIndex];
    UINT const totalOffsetInSecondaryHeap = baseOffsetInSecondaryHeap + offset;

    UINT const totalOffsetInPrimaryHeap = table.externalOffset + totalOffsetInSecondaryHeap;

    std::vector handles = {
        m_cpuDescriptorHeap.GetDescriptorHandleCPU(totalOffsetInPrimaryHeap),
        m_gpuDescriptorHeap.GetDescriptorHandleCPU(totalOffsetInPrimaryHeap),
        table.heap.GetDescriptorHandleCPU(totalOffsetInSecondaryHeap)
    };

    return handles;
}

bool ShaderResources::CheckListSizeUpdate(UINT* firstResizedList, UINT* totalListDescriptorCount)
{
    *firstResizedList         = UINT_MAX;
    *totalListDescriptorCount = 0;

    for (UINT index = 0; index < m_descriptorLists.size(); ++index)
    {
        auto&      list         = m_descriptorLists[index];
        UINT const requiredSize = list.sizeGetter();

        if (list.size < requiredSize || list.size == 0)
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
    UINT const totalDescriptorCount = m_totalTableDescriptorCount + totalListDescriptorCount;

    m_cpuDescriptorHeap.Create(m_device, totalDescriptorCount, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, false, true);
    NAME_D3D12_OBJECT(m_cpuDescriptorHeap);

    m_gpuDescriptorHeap.Create(m_device, totalDescriptorCount, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true, false);
    NAME_D3D12_OBJECT(m_gpuDescriptorHeap);

    for (auto const& table : m_descriptorTables)
        table.parameter->gpuHandle = m_gpuDescriptorHeap.GetDescriptorHandleGPU(table.externalOffset);

    UINT externalOffset = m_totalTableOffset;
    for (auto& list : m_descriptorLists)
    {
        list.externalOffset       = externalOffset;
        list.parameter->gpuHandle = m_gpuDescriptorHeap.GetDescriptorHandleGPU(list.externalOffset);

        if (list.parameter->index >= firstResizedListIndex)
        {
            auto assigner = list.descriptorAssigner;
            auto builder  = [this, externalOffset, assigner](UINT const index)
            {
                UINT const internalOffset = externalOffset + index;
                assigner(m_device.Get(), index, m_cpuDescriptorHeap.GetDescriptorHandleCPU(internalOffset));
            };

            list.listBuilder(builder);
        }

        externalOffset += list.size;
    }
}
