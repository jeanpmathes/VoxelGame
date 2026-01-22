// <copyright file="ShaderResources.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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

#include <span>
#include <variant>

#include "DescriptorHeap.hpp"
#include "IntegerSet.hpp"

/**
 * Manages the resources for shaders, including on heap and as direct root parameters.
 */
class ShaderResources
{
public:
    /**
     * \brief Signals that a heap descriptor table range has an unbounded size.
     */
    static constexpr UINT UNBOUNDED = UINT_MAX;

    union Value32
    {
        INT32  sInteger;
        UINT32 uInteger;
        FLOAT  floating;
    };

private:
    enum class QueueType : BYTE
    {
        GRAPHICS = 0,
        COMPUTE  = 1
    };

    struct RootConstant
    {
        UINT      index;
        QueueType queue;
    };

    struct RootConstantBufferView
    {
        D3D12_GPU_VIRTUAL_ADDRESS gpuAddress;
    };

    struct RootShaderResourceView
    {
        D3D12_GPU_VIRTUAL_ADDRESS gpuAddress;
    };

    struct RootUnorderedAccessView
    {
        D3D12_GPU_VIRTUAL_ADDRESS gpuAddress;
    };

    struct RootHeapDescriptorTable
    {
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle;
        UINT                        index;
    };

    struct RootHeapDescriptorList
    {
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle;
        UINT                        index;
        bool                        isSelectionList;
    };

    using RootParameter = std::variant<RootConstant, RootConstantBufferView, RootShaderResourceView, RootUnorderedAccessView, RootHeapDescriptorTable, RootHeapDescriptorList>;

public:
    /**
     * Defines a resource binding location in a shader.
     */
    struct ShaderLocation
    {
        /**
         * The register index.
         */
        UINT reg = 0;
        /**
         * The register space.
         */
        UINT space = 0;
    };

    struct ConstantBufferViewDescriptor
    {
        D3D12_GPU_VIRTUAL_ADDRESS gpuAddress{};
        UINT                      size{};

        ConstantBufferViewDescriptor() = default;
        ConstantBufferViewDescriptor(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, UINT size);
        explicit(false) ConstantBufferViewDescriptor(D3D12_CONSTANT_BUFFER_VIEW_DESC const* description);

        static constexpr D3D12_DESCRIPTOR_RANGE_TYPE RANGE_TYPE = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
        void                                         Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const;
    };

    struct ShaderResourceViewDescriptor
    {
        Allocation<ID3D12Resource>             resource{};
        D3D12_SHADER_RESOURCE_VIEW_DESC const* description{};

        static constexpr D3D12_DESCRIPTOR_RANGE_TYPE RANGE_TYPE = D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
        void                                         Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const;
    };

    struct UnorderedAccessViewDescriptor
    {
        Allocation<ID3D12Resource>              resource{};
        D3D12_UNORDERED_ACCESS_VIEW_DESC const* description{};

        static constexpr D3D12_DESCRIPTOR_RANGE_TYPE RANGE_TYPE = D3D12_DESCRIPTOR_RANGE_TYPE_UAV;
        void                                         Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const;
    };

    struct Description;

    struct Table
    {
        friend struct Description;

        struct Entry
        {
            friend class ShaderResources;

            explicit     Entry(UINT heapParameterIndex, UINT inHeapIndex);
            static Entry invalid;

            [[nodiscard]] bool IsValid() const;

        private:
            UINT m_heapParameterIndex;
            UINT m_inHeapIndex;
        };

        Entry AddConstantBufferView(ShaderLocation location, UINT count = 1);
        Entry AddUnorderedAccessView(ShaderLocation location, UINT count = 1);
        Entry AddShaderResourceView(ShaderLocation location, UINT count = 1);

    private:
        Entry AddView(ShaderLocation location, UINT count, D3D12_DESCRIPTOR_RANGE_TYPE type);

        explicit Table(UINT heap);
        UINT     m_heap;

        std::vector<nv_helpers_dx12::RootSignatureGenerator::HeapRange> m_heapRanges = {};
        std::vector<UINT>                                               m_offsets    = {{0}};
    };

    enum class ConstantHandle : UINT
    {
        INVALID = UINT_MAX
    };

    enum class TableHandle : UINT
    {
        INVALID = UINT_MAX
    };

    enum class ListHandle : UINT
    {
        INVALID = UINT_MAX
    };

    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor> || std::is_same_v<Descriptor, ShaderResourceViewDescriptor> || std::is_same_v<
            Descriptor, UnorderedAccessViewDescriptor>)
    class SelectionList;

    struct Description
    {
        friend class ShaderResources;

        /**
         * Add a root constant directly in the root signature.
         */
        ConstantHandle AddRootConstant(std::function<Value32()> const& getter, ShaderLocation location);

        /**
         * Add a CBV directly in the root signature.
         */
        void AddConstantBufferView(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, ShaderLocation location);

        /**
         * Add an SRV directly in the root signature.
         */
        void AddShaderResourceView(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, ShaderLocation location);

        /**
         * Add an UAV directly in the root signature.
         */
        void AddUnorderedAccessView(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, ShaderLocation location);

        /**
         * Add a static heap descriptor table, containing CBVs, SRVs and UAVs.
         * Contains multiple parameters and cannot be resized.
         */
        template <class TableBuilder>
        TableHandle AddHeapDescriptorTable(TableBuilder builder)
        {
            auto const handle = static_cast<UINT>(m_rootParameters.size()) + m_existingRootParameterCount;
            Table      table(handle);

            builder(table);

            m_heapDescriptorTableCount += table.m_offsets.back();

            m_rootSignatureGenerator.AddHeapRangesParameter(table.m_heapRanges);
            m_rootParameters.emplace_back(RootHeapDescriptorTable{});
            m_heapDescriptorTableOffsets.emplace_back(std::move(table.m_offsets));

            return static_cast<TableHandle>(handle);
        }

        /**
         * \brief Add a static texture sampler.
         * \param location The shader location of the sampler.
         * \param filter The sampler filter.
         * \param mode The texture address mode, used when sampling outside [0, 1].
         * \param maxAnisotropy The maximum anisotropy level.
         */
        void AddStaticSampler(ShaderLocation location, D3D12_FILTER filter, D3D12_TEXTURE_ADDRESS_MODE mode, UINT maxAnisotropy = 1);

        /**
         * \brief Enable the input assembler option in the root signature.
         */
        void EnableInputAssembler();

        using DescriptorBuilder  = std::function<void(UINT)>;
        using DescriptorAssigner = std::function<void(ID3D12Device*, UINT, D3D12_CPU_DESCRIPTOR_HANDLE)>;

        using SizeGetter = std::function<UINT()>;
        template <class Descriptor>
        using DescriptorGetter = std::function<Descriptor(UINT)>;
        using ListBuilder      = std::function<void(DescriptorBuilder const&)>;

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddConstantBufferViewDescriptorList(
            ShaderLocation                                        location,
            SizeGetter                                            count,
            DescriptorGetter<ConstantBufferViewDescriptor> const& descriptor,
            ListBuilder                                           builder);

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddShaderResourceViewDescriptorList(
            ShaderLocation                                        location,
            SizeGetter                                            count,
            DescriptorGetter<ShaderResourceViewDescriptor> const& descriptor,
            ListBuilder                                           builder);

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddUnorderedAccessViewDescriptorList(
            ShaderLocation                                         location,
            SizeGetter                                             count,
            DescriptorGetter<UnorderedAccessViewDescriptor> const& descriptor,
            ListBuilder                                            builder);

    private:
        template <class Descriptor>
            requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor> || std::is_same_v<Descriptor, ShaderResourceViewDescriptor> || std::is_same_v<
                Descriptor, UnorderedAccessViewDescriptor>)
        ListHandle AddDescriptorList(
            ShaderLocation const&               location,
            SizeGetter&&                        count,
            DescriptorGetter<Descriptor> const& descriptor,
            ListBuilder&&                       builder,
            std::optional<UINT> const           numberOfDescriptorsIfSelectionList)
        {
            UINT const number     = numberOfDescriptorsIfSelectionList.value_or(UNBOUNDED);
            auto const listHandle = static_cast<UINT>(m_rootParameters.size()) + m_existingRootParameterCount;

            m_rootSignatureGenerator.AddHeapRangesParameter({{location.reg, number, location.space, Descriptor::RANGE_TYPE, 0},});
            m_rootParameters.emplace_back(RootHeapDescriptorList{});
            m_descriptorListDescriptions.emplace_back(
                std::move(count),
                [descriptor](auto device, auto index, auto cpuHandle)
                {
                    Descriptor const view = descriptor(index);
                    view.Create(device, cpuHandle);
                },
                std::move(builder),
                numberOfDescriptorsIfSelectionList.has_value());

            return static_cast<ListHandle>(listHandle);
        }

    public:
        /**
         * \brief Add a CBV selection list.
         * \param location The shader location of the CBV.
         * \param window The size of the selection window.
         * \return The selection list.
         */
        SelectionList<ConstantBufferViewDescriptor> AddConstantBufferViewDescriptorSelectionList(ShaderLocation location, UINT window = 1);

        /**
         * \brief Add an SRV selection list.
         * \param location The shader location of the SRV.
         * \param window The size of the selection window.
         * \return The selection list.
         */
        SelectionList<ShaderResourceViewDescriptor> AddShaderResourceViewDescriptorSelectionList(ShaderLocation location, UINT window = 1);

        /**
         * \brief Add a UAV selection list.
         * \param location The shader location of the UAV.
         * \param window The size of the selection window.
         * \return The selection list.
         */
        SelectionList<UnorderedAccessViewDescriptor> AddUnorderedAccessViewDescriptorSelectionList(ShaderLocation location, UINT window = 1);

    private:
        template <class Descriptor>
            requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor> || std::is_same_v<Descriptor, ShaderResourceViewDescriptor> || std::is_same_v<
                Descriptor, UnorderedAccessViewDescriptor>)
        SelectionList<Descriptor> AddSelectionList(ShaderLocation const& location, UINT const window)
        {
            Require(window > 0);

            return SelectionList<Descriptor>(location, this, window);
        }

        void AddRootParameter(ShaderLocation location, D3D12_ROOT_PARAMETER_TYPE type, RootParameter parameter);

        ComPtr<ID3D12RootSignature> GenerateRootSignature(ComPtr<ID3D12Device> const& device);

        explicit Description(UINT existingRootParameterCount);
        UINT     m_existingRootParameterCount = 0;

        std::vector<RootParameter>              m_rootParameters         = {};
        nv_helpers_dx12::RootSignatureGenerator m_rootSignatureGenerator = {};

        std::vector<std::function<Value32()>> m_rootConstants = {};

        std::vector<std::vector<UINT>> m_heapDescriptorTableOffsets = {};
        UINT                           m_heapDescriptorTableCount   = 0;

        struct DescriptorListDescription
        {
            SizeGetter         sizeGetter         = {};
            DescriptorAssigner descriptorAssigner = {};
            ListBuilder        listBuilder        = {};

            bool isSelectionList = false;
        };

        std::vector<DescriptorListDescription> m_descriptorListDescriptions = {};
    };

    /**
     * \brief A selection list is a list of descriptors of which a window is selected as parameters.
     * \tparam Descriptor The descriptor type.
     */
    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor> || std::is_same_v<Descriptor, ShaderResourceViewDescriptor> || std::is_same_v<
            Descriptor, UnorderedAccessViewDescriptor>)
    class SelectionList
    {
    public:
        SelectionList()  = default;
        ~SelectionList() = default;

        SelectionList(SelectionList const& other)                = delete;
        SelectionList& operator=(SelectionList const& other)     = delete;
        SelectionList(SelectionList&& other) noexcept            = default;
        SelectionList& operator=(SelectionList&& other) noexcept = default;

        friend Description;
        friend ShaderResources;

    private:
        SelectionList(ShaderLocation const location, Description* description, UINT window)
            : m_data(std::make_unique<Data>())
        {
            m_data->window = window;
            m_data->handle = description->AddDescriptorList<Descriptor>(
                location,
                [ptr = m_data.get()] { return static_cast<UINT>(ptr->descriptors.size()); },
                [ptr = m_data.get()](UINT index) -> Descriptor { return ptr->descriptors[index]; },
                [ptr = m_data.get()](Description::DescriptorBuilder const& builder) { for (UINT i = 0; i < ptr->count; i++) builder(i); },
                window);
        }

        void SetDescriptors(std::vector<Descriptor> const& descriptors)
        {
            Require(descriptors.size() >= m_data->window || m_data->window == UNBOUNDED);

            m_data->count = static_cast<UINT>(descriptors.size());
            m_data->descriptors.resize(std::max(static_cast<size_t>(m_data->count), m_data->descriptors.size()));

            for (UINT index = 0; index < m_data->count; index++) m_data->descriptors[index] = descriptors[index];
        }

        struct Data
        {
            ListHandle              handle      = ListHandle::INVALID;
            std::vector<Descriptor> descriptors = {};

            UINT window = 0;
            UINT count  = 0;
        };

        std::unique_ptr<Data> m_data = nullptr;
    };

    template <class GraphicsBuilder, class ComputeBuilder>
    void Initialize(GraphicsBuilder graphics, ComputeBuilder compute, ComPtr<ID3D12Device5> device)
    {
        m_device = device;

        UINT        rootParameterCount = 0;
        Description graphicsDesc(rootParameterCount);
        graphics(graphicsDesc);

        rootParameterCount = static_cast<UINT>(graphicsDesc.m_rootParameters.size());
        Description computeDesc(rootParameterCount);
        compute(computeDesc);

        m_graphicsRootSignature  = graphicsDesc.GenerateRootSignature(device);
        m_graphicsRootParameters = std::move(graphicsDesc.m_rootParameters);
        NAME_D3D12_OBJECT(m_graphicsRootSignature);

        m_computeRootSignature  = computeDesc.GenerateRootSignature(device);
        m_computeRootParameters = std::move(computeDesc.m_rootParameters);
        NAME_D3D12_OBJECT(m_computeRootSignature);

        auto initializeConstants = [&](std::vector<RootParameter>& rootParameters, std::vector<std::function<Value32()>>&& getters, QueueType const queue)
        {
            UINT index = 0;

            for (UINT rootParameterIndex = 0; rootParameterIndex < rootParameters.size(); rootParameterIndex++)
                if (std::holds_alternative<RootConstant>(rootParameters[rootParameterIndex]))
                {
                    RootConstant& rootConstant = std::get<RootConstant>(rootParameters[rootParameterIndex]);
                    rootConstant.index         = static_cast<UINT>(m_constants.size());
                    rootConstant.queue         = queue;

                    Constant& constant          = m_constants.emplace_back();
                    constant.getter             = std::move(getters[index]);
                    constant.rootParameterIndex = rootParameterIndex;

                    index++;
                }
        };

        initializeConstants(m_graphicsRootParameters, std::move(graphicsDesc.m_rootConstants), QueueType::GRAPHICS);
        initializeConstants(m_computeRootParameters, std::move(computeDesc.m_rootConstants), QueueType::COMPUTE);

        m_totalTableDescriptorCount = graphicsDesc.m_heapDescriptorTableCount + computeDesc.m_heapDescriptorTableCount;

        auto initializeDescriptorTables = [&](std::vector<RootParameter>& rootParameters, std::vector<std::vector<UINT>>& internalOffsets, UINT* externalOffset)
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

        initializeDescriptorTables(m_graphicsRootParameters, graphicsDesc.m_heapDescriptorTableOffsets, &m_totalTableOffset);
        initializeDescriptorTables(m_computeRootParameters, computeDesc.m_heapDescriptorTableOffsets, &m_totalTableOffset);

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

        initializeDescriptorLists(m_graphicsRootParameters, graphicsDesc.m_descriptorListDescriptions);
        initializeDescriptorLists(m_computeRootParameters, computeDesc.m_descriptorListDescriptions);

        Update();
    }

    [[nodiscard]] bool IsInitialized() const;

    [[nodiscard]] ComPtr<ID3D12RootSignature> GetGraphicsRootSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> GetComputeRootSignature() const;

    /**
     * Requests a refresh of descriptors in the given list.
     * Each index in the list will be refreshed when update is called.
     * If the list is resized, no duplicate refreshes will be performed.
     */
    void RequestListRefresh(ListHandle listHandle, IntegerSet<> const& indices);

    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor> || std::is_same_v<Descriptor, ShaderResourceViewDescriptor> || std::is_same_v<
            Descriptor, UnorderedAccessViewDescriptor>)
    void SetSelectionListContent(SelectionList<Descriptor>& list, std::vector<Descriptor> const& descriptors)
    {
        list.SetDescriptors(descriptors);
        RequestListRefresh(list.m_data->handle, IntegerSet<>::Full(list.m_data->count));
    }

    void Bind(ComPtr<ID3D12GraphicsCommandList> commandList);

    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor> || std::is_same_v<Descriptor, ShaderResourceViewDescriptor> || std::is_same_v<
            Descriptor, UnorderedAccessViewDescriptor>)
    void BindSelectionListIndex(SelectionList<Descriptor>& list, UINT index, ComPtr<ID3D12GraphicsCommandList> const commandList)
    {
        auto const           parameterIndex = static_cast<UINT>(list.m_data->handle);
        RootParameter const& parameter      = GetRootParameter(parameterIndex);

        if (std::holds_alternative<RootHeapDescriptorList>(parameter))
        {
            auto& data = m_descriptorLists[std::get<RootHeapDescriptorList>(parameter).index];

            Require(list.m_data->count > index);

            data.selection = index;
            data.bind(commandList);
        }
        else Require(FALSE);
    }

    /**
     * \brief Trigger an update of a root constant while the resources are bound. Will use the getter.
     * \param handle The constant handle.
     * \param commandList The command list to use for updating.
     */
    void UpdateConstant(ConstantHandle handle, ComPtr<ID3D12GraphicsCommandList> const& commandList) const;

    void Update();

    /**
     * Creates a constant buffer view at a given table entry.
     * If the entry contains multiple descriptors, use the offset, else zero.
     */
    void CreateConstantBufferView(Table::Entry entry, UINT offset, ConstantBufferViewDescriptor const& descriptor) const;
    /**
     * Creates a shader resource view at a given table entry.
     * If the entry contains multiple descriptors, use the offset, else zero.
     */
    void CreateShaderResourceView(Table::Entry entry, UINT offset, ShaderResourceViewDescriptor const& descriptor) const;
    /**
     * Creates an unordered access view at a given table entry.
     * If the entry contains multiple descriptors, use the offset, else zero.
     */
    void CreateUnorderedAccessView(Table::Entry entry, UINT offset, UnorderedAccessViewDescriptor const& descriptor) const;

private:
    [[nodiscard]] RootParameter const&       GetRootParameter(UINT index) const;
    std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> GetDescriptorHandlesForWrite(RootParameter const& parameter, UINT inHeapIndex, UINT offset) const;

    bool CheckListSizeUpdate(UINT* firstResizedList, UINT* totalListDescriptorCount);
    void PerformSizeUpdate(UINT firstResizedListIndex, UINT totalListDescriptorCount);

    DescriptorHeap m_cpuDescriptorHeap      = {};
    DescriptorHeap m_gpuDescriptorHeap      = {};
    bool           m_cpuDescriptorHeapDirty = false;

    ComPtr<ID3D12Device5> m_device = nullptr;

    struct Constant
    {
        std::function<Value32()> getter             = {};
        UINT                     rootParameterIndex = 0;
    };

    std::vector<Constant> m_constants = {};

    struct DescriptorTable
    {
        DescriptorHeap           heap;
        RootHeapDescriptorTable* parameter = nullptr;

        std::vector<UINT> internalOffsets = {};
        UINT              externalOffset  = 0;
    };

    std::vector<DescriptorTable> m_descriptorTables          = {};
    UINT                         m_totalTableDescriptorCount = 0;
    UINT                         m_totalTableOffset          = 0;

    struct DescriptorList
    {
        Description::SizeGetter         sizeGetter         = {};
        Description::DescriptorAssigner descriptorAssigner = {};
        Description::ListBuilder        listBuilder        = {};
        RootHeapDescriptorList*         parameter          = nullptr;

        UINT externalOffset = 0;

        UINT         size         = 0;
        IntegerSet<> dirtyIndices = {};

        UINT                                                   selection = 0;
        std::function<void(ComPtr<ID3D12GraphicsCommandList>)> bind      = {};
    };

    std::vector<DescriptorList> m_descriptorLists = {};

    ComPtr<ID3D12RootSignature> m_graphicsRootSignature  = nullptr;
    std::vector<RootParameter>  m_graphicsRootParameters = {};

    ComPtr<ID3D12RootSignature> m_computeRootSignature  = nullptr;
    std::vector<RootParameter>  m_computeRootParameters = {};
};

template <typename Entry, typename Index>
ShaderResources::Description::SizeGetter CreateSizeGetter(Bag<Entry, Index>* list)
{
    Require(list != nullptr);

    return [list] { return static_cast<UINT>(list->GetCapacity()); };
}

template <typename Entry, typename Index>
ShaderResources::Description::ListBuilder CreateBagBuilder(Bag<Entry, Index>* bag, std::function<UINT(Entry const&)> indexProvider)
{
    Require(bag != nullptr);

    return [bag, indexProvider](ShaderResources::Description::DescriptorBuilder const& builder)
    {
        bag->ForEach([indexProvider, &builder](Entry const& entry) { builder(indexProvider(entry)); });
    };
}
