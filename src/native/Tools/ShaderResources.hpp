// <copyright file="ShaderResources.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
        INT32 sInteger;
        UINT32 uInteger;
        FLOAT floating;
    };

private:
    struct RootConstant
    {
        UINT index;
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
        UINT index;
    };

    struct RootHeapDescriptorList
    {
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle;
        UINT index;
        bool isSelectionList;
    };

    using RootParameter = std::variant<RootConstant, RootConstantBufferView, RootShaderResourceView,
                                       RootUnorderedAccessView, RootHeapDescriptorTable, RootHeapDescriptorList>;
    
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
        UINT size{};

        ConstantBufferViewDescriptor() = default;
        ConstantBufferViewDescriptor(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, UINT size);
        explicit(false) ConstantBufferViewDescriptor(const D3D12_CONSTANT_BUFFER_VIEW_DESC* description);
        
        static constexpr D3D12_DESCRIPTOR_RANGE_TYPE RANGE_TYPE = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
        void Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const;
    };

    struct ShaderResourceViewDescriptor
    {
        Allocation<ID3D12Resource> resource{};
        const D3D12_SHADER_RESOURCE_VIEW_DESC* description{};

        static constexpr D3D12_DESCRIPTOR_RANGE_TYPE RANGE_TYPE = D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
        void Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const;
    };

    struct UnorderedAccessViewDescriptor
    {
        Allocation<ID3D12Resource> resource{};
        const D3D12_UNORDERED_ACCESS_VIEW_DESC* description{};

        static constexpr D3D12_DESCRIPTOR_RANGE_TYPE RANGE_TYPE = D3D12_DESCRIPTOR_RANGE_TYPE_UAV;
        void Create(ComPtr<ID3D12Device> device, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle) const;
    };

    struct Description;

    struct Table
    {
        friend struct Description;

        struct Entry
        {
            friend class ShaderResources;

            explicit Entry(UINT heapParameterIndex, UINT inHeapIndex);
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
        UINT m_heap;

        std::vector<nv_helpers_dx12::RootSignatureGenerator::HeapRange> m_heapRanges = {};
        std::vector<UINT> m_offsets = {{0}};
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
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor>
            or std::is_same_v<Descriptor, ShaderResourceViewDescriptor>
            or std::is_same_v<Descriptor, UnorderedAccessViewDescriptor>)
    class SelectionList;

    struct Description
    {
        friend class ShaderResources;

        /**
         * Add a root constant directly in the root signature.
         */
        ConstantHandle AddRootConstant(const std::function<Value32()>& getter, ShaderLocation location);

        /**
         * Add a CBV directly in the root signature.
         */
        void AddConstantBufferView(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, ShaderLocation location);

        /**
         * Add a SRV directly in the root signature.
         */
        void AddShaderResourceView(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, ShaderLocation location);

        /**
         * Add a UAV directly in the root signature.
         */
        void AddUnorderedAccessView(D3D12_GPU_VIRTUAL_ADDRESS gpuAddress, ShaderLocation location);
        
        /**
         * Add a static heap descriptor table, containing CBVs, SRVs and UAVs.
         * Contains multiple parameters and cannot be resized.
         */
        TableHandle AddHeapDescriptorTable(const std::function<void(Table&)>& builder);

        /**
         * \brief Add a static texture sampler.
         * \param location The shader location of the sampler.
         * \param filter The sampler filter.
         */
        void AddStaticSampler(ShaderLocation location, D3D12_FILTER filter);

        /**
         * \brief Enable the input assembler option in the root signature.
         */
        void EnableInputAssembler();

        using DescriptorBuilder = std::function<void(UINT)>;
        using DescriptorAssigner = std::function<void(ID3D12Device*, UINT, D3D12_CPU_DESCRIPTOR_HANDLE)>;

        using SizeGetter = std::function<UINT()>;
        template <class Descriptor>
        using DescriptorGetter = std::function<Descriptor(UINT)>;
        using ListBuilder = std::function<void(const DescriptorBuilder&)>;

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddConstantBufferViewDescriptorList(
            ShaderLocation location,
            SizeGetter count,
            const DescriptorGetter<ConstantBufferViewDescriptor>& descriptor,
            ListBuilder builder);

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddShaderResourceViewDescriptorList(
            ShaderLocation location,
            SizeGetter count,
            const DescriptorGetter<ShaderResourceViewDescriptor>& descriptor,
            ListBuilder builder);

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddUnorderedAccessViewDescriptorList(
            ShaderLocation location,
            SizeGetter count,
            const DescriptorGetter<UnorderedAccessViewDescriptor>& descriptor,
            ListBuilder builder);

    private:
        template <class Descriptor>
            requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor>
                or std::is_same_v<Descriptor, ShaderResourceViewDescriptor>
                or std::is_same_v<Descriptor, UnorderedAccessViewDescriptor>)
        ListHandle AddDescriptorList(
            const ShaderLocation& location,
            SizeGetter&& count,
            const DescriptorGetter<Descriptor>& descriptor,
            ListBuilder&& builder,
            std::optional<UINT> numberOfDescriptorsIfSelectionList)
        {
            const UINT number = numberOfDescriptorsIfSelectionList.value_or(UNBOUNDED);
            const UINT listHandle = static_cast<UINT>(m_rootParameters.size()) + m_existingRootParameterCount;

            m_rootSignatureGenerator.AddHeapRangesParameter({
                {location.reg, number, location.space, Descriptor::RANGE_TYPE, 0},
            });
            m_rootParameters.push_back(RootHeapDescriptorList{});
            m_descriptorListDescriptions.emplace_back(std::move(count),
                                                      [descriptor](auto device, auto index, auto cpuHandle)
                                                      {
                                                          const Descriptor view = descriptor(index);
                                                          view.Create(device, cpuHandle);
                                                      }, std::move(builder),
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
        SelectionList<ConstantBufferViewDescriptor> AddConstantBufferViewDescriptorSelectionList(
            ShaderLocation location, UINT window = 1);

        /**
         * \brief Add a SRV selection list.
         * \param location The shader location of the SRV.
         * \param window The size of the selection window.
         * \return The selection list.
         */
        SelectionList<ShaderResourceViewDescriptor> AddShaderResourceViewDescriptorSelectionList(
            ShaderLocation location, UINT window = 1);

        /**
         * \brief Add a UAV selection list.
         * \param location The shader location of the UAV.
         * \param window The size of the selection window.
         * \return The selection list.
         */
        SelectionList<UnorderedAccessViewDescriptor> AddUnorderedAccessViewDescriptorSelectionList(
            ShaderLocation location, UINT window = 1);

    private:
        template <class Descriptor>
            requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor>
                or std::is_same_v<Descriptor, ShaderResourceViewDescriptor>
                or std::is_same_v<Descriptor, UnorderedAccessViewDescriptor>)
        SelectionList<Descriptor> AddSelectionList(const ShaderLocation& location, const UINT window)
        {
            REQUIRE(window > 0);

            return SelectionList<Descriptor>(location, this, window);
        }
        
        void AddRootParameter(ShaderLocation location, D3D12_ROOT_PARAMETER_TYPE type, RootParameter&& parameter);
        
        ComPtr<ID3D12RootSignature> GenerateRootSignature(ComPtr<ID3D12Device> device);

        explicit Description(UINT existingRootParameterCount);
        UINT m_existingRootParameterCount = 0;

        std::vector<RootParameter> m_rootParameters = {};
        nv_helpers_dx12::RootSignatureGenerator m_rootSignatureGenerator = {};

        std::vector<std::function<Value32()>> m_rootConstants = {};
        
        std::vector<std::vector<UINT>> m_heapDescriptorTableOffsets = {};
        UINT m_heapDescriptorTableCount = 0;

        struct DescriptorListDescription
        {
            SizeGetter sizeGetter = {};
            DescriptorAssigner descriptorAssigner = {};
            ListBuilder listBuilder = {};

            bool isSelectionList = false;
        };

        std::vector<DescriptorListDescription> m_descriptorListDescriptions = {};
    };

    /**
     * \brief A selection list is a list of descriptors of which a window is selected as parameters.
     * \tparam Descriptor The descriptor type.
     */
    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor>
            or std::is_same_v<Descriptor, ShaderResourceViewDescriptor>
            or std::is_same_v<Descriptor, UnorderedAccessViewDescriptor>)
    class SelectionList
    {
    public:
        SelectionList() : m_data(nullptr)
        {
        }

        ~SelectionList() = default;

        SelectionList(const SelectionList& other) = delete;
        SelectionList& operator=(const SelectionList& other) = delete;
        SelectionList(SelectionList&& other) noexcept = default;
        SelectionList& operator=(SelectionList&& other) noexcept = default;

        friend Description;
        friend ShaderResources;

    private:
        SelectionList(const ShaderLocation location, Description* description, UINT window) : m_data(
            std::make_unique<Data>())
        {
            m_data->window = window;
            m_data->handle = description->AddDescriptorList<Descriptor>(location, [ptr = m_data.get()]() -> UINT
                                                                        {
                                                                            return static_cast<UINT>(ptr->descriptors.
                                                                                size());
                                                                        }, [ptr = m_data.get()](
                                                                        UINT index) -> Descriptor
                                                                        {
                                                                            return ptr->descriptors[index];
                                                                        }, [ptr = m_data.get()](
                                                                        const Description::DescriptorBuilder& builder)
                                                                        {
                                                                            for (UINT i = 0; i < ptr->count; i++)
                                                                            {
                                                                                builder(i);
                                                                            }
                                                                        }, window);
        }

        void SetDescriptors(const std::vector<Descriptor>& descriptors)
        {
            REQUIRE(descriptors.size() >= m_data->window || m_data->window == UNBOUNDED);
            
            m_data->count = static_cast<UINT>(descriptors.size());
            m_data->descriptors.resize(std::max(static_cast<size_t>(m_data->count), m_data->descriptors.size()));

            for (UINT index = 0; index < m_data->count; index++)
            {
                m_data->descriptors[index] = descriptors[index];
            }
        }

        struct Data
        {
            ListHandle handle = ListHandle::INVALID;
            std::vector<Descriptor> descriptors = {};

            UINT window = 0;
            UINT count = 0;
        };

        std::unique_ptr<Data> m_data = nullptr;
    };

    using Builder = std::function<void(Description&)>;
    void Initialize(const Builder& graphics, const Builder& compute, ComPtr<ID3D12Device5> device);

    [[nodiscard]] bool IsInitialized() const;

    [[nodiscard]] ComPtr<ID3D12RootSignature> GetGraphicsRootSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> GetComputeRootSignature() const;

    /**
     * Requests a refresh of descriptors in the given list.
     * Each index in the list will be refreshed when update is called.
     * If the list is resized, no duplicate refreshes will be performed.
     */
    void RequestListRefresh(ListHandle listHandle, const IntegerSet<>& indices);

    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor>
            or std::is_same_v<Descriptor, ShaderResourceViewDescriptor>
            or std::is_same_v<Descriptor, UnorderedAccessViewDescriptor>)
    void SetSelectionListContent(SelectionList<Descriptor>& list, const std::vector<Descriptor>& descriptors)
    {
        list.SetDescriptors(descriptors);
        RequestListRefresh(list.m_data->handle, IntegerSet<>::Full(list.m_data->count));
    }

    void Bind(ComPtr<ID3D12GraphicsCommandList> commandList);

    template <class Descriptor>
        requires (std::is_same_v<Descriptor, ConstantBufferViewDescriptor>
            or std::is_same_v<Descriptor, ShaderResourceViewDescriptor>
            or std::is_same_v<Descriptor, UnorderedAccessViewDescriptor>)
    void BindSelectionListIndex(SelectionList<Descriptor>& list, UINT index,
                                const ComPtr<ID3D12GraphicsCommandList> commandList)
    {
        const UINT parameterIndex = static_cast<UINT>(list.m_data->handle);
        const RootParameter& parameter = GetRootParameter(parameterIndex);

        if (std::holds_alternative<RootHeapDescriptorList>(parameter))
        {
            auto& data = m_descriptorLists[std::get<RootHeapDescriptorList>(parameter).index];

            REQUIRE(list.m_data->count > index);

            data.selection = index;
            data.bind(commandList);
        }
        else
        {
            REQUIRE(FALSE);
        }
    }

    void Update();
    
    /**
     * Creates a constant buffer view at a given table entry.
     * If the entry contains multiple descriptors, use the offset, else zero.
     */
    void CreateConstantBufferView(Table::Entry entry, UINT offset,
                                  const ConstantBufferViewDescriptor& descriptor) const;
    /**
     * Creates a shader resource view at a given table entry.
     * If the entry contains multiple descriptors, use the offset, else zero.
     */
    void CreateShaderResourceView(Table::Entry entry, UINT offset,
                                  const ShaderResourceViewDescriptor& descriptor) const;
    /**
     * Creates an unordered access view at a given table entry.
     * If the entry contains multiple descriptors, use the offset, else zero.
     */
    void CreateUnorderedAccessView(Table::Entry entry, UINT offset,
                                   const UnorderedAccessViewDescriptor& descriptor) const;

private:
    [[nodiscard]] const RootParameter& GetRootParameter(UINT index) const;
    std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> GetDescriptorHandlesForWrite(
        const RootParameter& parameter,
        UINT inHeapIndex,
        UINT offset) const;

    bool CheckListSizeUpdate(UINT* firstResizedList, UINT* totalListDescriptorCount);
    void PerformSizeUpdate(UINT firstResizedListIndex, UINT totalListDescriptorCount);

    DescriptorHeap m_cpuDescriptorHeap = {};
    DescriptorHeap m_gpuDescriptorHeap = {};
    bool m_cpuDescriptorHeapDirty = false;
    
    ComPtr<ID3D12Device5> m_device = nullptr;

    struct Constant
    {
        std::function<Value32()> getter = {};
    };

    std::vector<Constant> m_constants = {};

    struct DescriptorTable
    {
        DescriptorHeap heap;
        RootHeapDescriptorTable* parameter = nullptr;

        std::vector<UINT> internalOffsets = {};
        UINT externalOffset = 0;
    };

    std::vector<DescriptorTable> m_descriptorTables = {};
    UINT m_totalTableDescriptorCount = 0;
    UINT m_totalTableOffset = 0;

    struct DescriptorList
    {
        Description::SizeGetter sizeGetter = {};
        Description::DescriptorAssigner descriptorAssigner = {};
        Description::ListBuilder listBuilder = {};
        RootHeapDescriptorList* parameter = nullptr;

        UINT externalOffset = 0;

        UINT size = 0;
        IntegerSet<> dirtyIndices = {};

        UINT selection = 0;
        std::function<void(ComPtr<ID3D12GraphicsCommandList>)> bind = {};
    };

    std::vector<DescriptorList> m_descriptorLists = {};

    ComPtr<ID3D12RootSignature> m_graphicsRootSignature = nullptr;
    std::vector<RootParameter> m_graphicsRootParameters = {};

    ComPtr<ID3D12RootSignature> m_computeRootSignature = nullptr;
    std::vector<RootParameter> m_computeRootParameters = {};
};

template <typename Entry, typename Index>
ShaderResources::Description::SizeGetter CreateSizeGetter(Bag<Entry, Index>* list)
{
    REQUIRE(list != nullptr);

    return [list]() -> UINT
    {
        return static_cast<UINT>(list->GetCapacity());
    };
}

template <typename Entry, typename Index>
ShaderResources::Description::ListBuilder CreateListBuilder(Bag<Entry, Index>* list,
                                                            std::function<UINT(const Entry&)> indexProvider)
{
    REQUIRE(list != nullptr);

    return [list, indexProvider](const ShaderResources::Description::DescriptorBuilder& builder)
    {
        for (const auto& entry : *list)
        {
            builder(indexProvider(entry));
        }
    };
}
