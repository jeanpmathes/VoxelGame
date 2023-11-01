// <copyright file="ShaderResources.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <variant>

#include "DescriptorHeap.hpp"

/**
 * Manages the resources for shaders, including on heap and as direct root parameters.
 */
class ShaderResources
{
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
    };

    using RootParameter = std::variant<RootConstantBufferView, RootShaderResourceView, RootUnorderedAccessView,
                                       RootHeapDescriptorTable, RootHeapDescriptorList>;
    
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
    };

    struct ShaderResourceViewDescriptor
    {
        Allocation<ID3D12Resource> resource{};
        const D3D12_SHADER_RESOURCE_VIEW_DESC* description{};
    };

    struct UnorderedAccessViewDescriptor
    {
        Allocation<ID3D12Resource> resource{};
        const D3D12_UNORDERED_ACCESS_VIEW_DESC* description{};
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

    enum class TableHandle : UINT
    {
        INVALID = UINT_MAX
    };

    enum class ListHandle : UINT
    {
        INVALID = UINT_MAX
    };

    struct Description
    {
        friend class ShaderResources;

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
            DescriptorGetter<ConstantBufferViewDescriptor> descriptor,
            ListBuilder builder);

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddShaderResourceViewDescriptorList(
            ShaderLocation location,
            SizeGetter count,
            DescriptorGetter<ShaderResourceViewDescriptor> descriptor,
            ListBuilder builder);

        /**
         * A list of descriptors of uniform type, placed as heap descriptors.
         * The list requires an external backing container.
         */
        ListHandle AddUnorderedAccessViewDescriptorList(
            ShaderLocation location,
            SizeGetter count,
            DescriptorGetter<UnorderedAccessViewDescriptor> descriptor,
            ListBuilder builder);

    private:
        void AddRootParameter(ShaderLocation location, D3D12_ROOT_PARAMETER_TYPE type, RootParameter&& parameter);
        
        ComPtr<ID3D12RootSignature> GenerateRootSignature(ComPtr<ID3D12Device> device);

        explicit Description(UINT existingRootParameterCount);
        UINT m_existingRootParameterCount = 0;

        std::vector<RootParameter> m_rootParameters = {};
        nv_helpers_dx12::RootSignatureGenerator m_rootSignatureGenerator = {};

        std::vector<std::vector<UINT>> m_heapDescriptorTableOffsets = {};
        UINT m_heapDescriptorTableCount = 0;

        std::vector<std::tuple<SizeGetter, DescriptorAssigner, ListBuilder>> m_descriptorListFunctions = {};
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
    void RequestListRefresh(ListHandle listHandle, const std::set<size_t>& indices);

    void Bind(ComPtr<ID3D12GraphicsCommandList> commandList);
    void Update();

    void CreateConstantBufferView(Table::Entry entry, UINT offset, const ConstantBufferViewDescriptor& descriptor);
    void CreateShaderResourceView(Table::Entry entry, UINT offset, const ShaderResourceViewDescriptor& descriptor);
    void CreateUnorderedAccessView(Table::Entry entry, UINT offset, const UnorderedAccessViewDescriptor& descriptor);

private:
    [[nodiscard]] const RootParameter& GetRootParameter(UINT index) const;
    D3D12_CPU_DESCRIPTOR_HANDLE GetDescriptorHandleForWrite(
        const RootParameter& parameter,
        UINT inHeapIndex,
        UINT offset);

    bool CheckListSizeUpdate(UINT* firstResizedList, UINT* totalListDescriptorCount);
    void PerformSizeUpdate(UINT firstResizedListIndex, UINT totalListDescriptorCount);

    DescriptorHeap m_descriptorHeap = {};
    ComPtr<ID3D12Device5> m_device = nullptr;

    struct DescriptorTable
    {
        DescriptorHeap heap;
        RootHeapDescriptorTable* parameter = nullptr;

        std::vector<UINT> internalOffsets = {};
        UINT externalOffset = 0;

        bool dirty = false;
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
        std::set<size_t> dirtyIndices = {};
    };

    std::vector<DescriptorList> m_descriptorLists = {};

    ComPtr<ID3D12RootSignature> m_graphicsRootSignature = nullptr;
    std::vector<RootParameter> m_graphicsRootParameters = {};

    ComPtr<ID3D12RootSignature> m_computeRootSignature = nullptr;
    std::vector<RootParameter> m_computeRootParameters = {};
};

template <typename Entry>
ShaderResources::Description::SizeGetter CreateSizeGetter(GappedList<Entry>* list)
{
    REQUIRE(list != nullptr);

    return [list]() -> UINT
    {
        return static_cast<UINT>(list->GetCapacity());
    };
}

template <typename Entry>
ShaderResources::Description::ListBuilder CreateListBuilder(GappedList<Entry>* list,
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
