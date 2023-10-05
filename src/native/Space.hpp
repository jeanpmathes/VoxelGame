﻿// <copyright file="Space.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "nv_helpers_dx12/ShaderBindingTableGenerator.hpp"

#include "Common.hpp"

#include "Objects/Camera.hpp"
#include "Objects/Light.hpp"
#include "Objects/MeshObject.hpp"

class Texture;

struct MaterialDescription
{
    LPWSTR name;
    BOOL visible;
    BOOL shadowCaster;
    BOOL opaque;

    LPWSTR normalClosestHitSymbol;
    LPWSTR normalAnyHitSymbol;
    LPWSTR normalIntersectionSymbol;

    LPWSTR shadowClosestHitSymbol;
    LPWSTR shadowAnyHitSymbol;
    LPWSTR shadowIntersectionSymbol;
};

struct ShaderFileDescription
{
    LPWSTR path;
    UINT symbolCount;
};

struct SpacePipelineDescription
{
    UINT shaderCount;
    UINT materialCount;

    UINT textureCountFirstSlot;
    UINT textureCountSecondSlot;

    NativeErrorFunc onShaderLoadingError;
};

class SpacePipeline
{
public:
    ShaderFileDescription* shaderFiles;
    LPWSTR* symbols;
    MaterialDescription* materials;
    Texture** textures;
    SpacePipelineDescription description;
};

enum class MaterialFlags : BYTE
{
    VISIBLE = 1 << 0,
    SHADOW_CASTER = 1 << 1,
};

DEFINE_ENUM_FLAG_OPERATORS(MaterialFlags)

#pragma pack(push, 4)
struct GlobalConstantBuffer
{
    float time;
    DirectX::XMFLOAT3 lightDirection;
    float minLight;
    DirectX::XMUINT2 textureSize;
};

struct MaterialConstantBuffer
{
    UINT index;
};
#pragma pack(pop)

class Material
{
public:
    std::wstring name;
    UINT index;
    bool isOpaque{};
    D3D12_RAYTRACING_GEOMETRY_TYPE geometryType{};
    MaterialFlags flags{};
    
    std::wstring normalHitGroup;
    ComPtr<ID3D12RootSignature> normalRootSignature;

    std::wstring shadowHitGroup;
    ComPtr<ID3D12RootSignature> shadowRootSignature;

    Allocation<ID3D12Resource> materialConstantBuffer;
};

/**
 * Contains all spatial objects and controls the render pipeline for the space.
 */
class Space
{
public:
    explicit Space(NativeClient& nativeClient);

    void PerformInitialSetupStepOne(ComPtr<ID3D12CommandQueue> commandQueue);
    void PerformResolutionDependentSetup(const Resolution& resolution);
    bool PerformInitialSetupStepTwo(const SpacePipeline& pipeline);

    /**
     * Create a new mesh object with a given material. 
     */
    MeshObject& CreateMeshObject(UINT materialIndex);
    /**
     * Mark a mesh object as modified, so that instance data can be updated.
     */
    void MarkMeshObjectModified(MeshObject::Handle handle);
    /**
     * Activate a mesh object for rendering. It must have a valid mesh.
     */
    size_t ActivateMeshObject(MeshObject::Handle handle);
    /**
     * Deactivate a mesh object.
     */
    void DeactivateMeshObject(size_t index);
    /**
     * Return a mesh object.
     */
    void ReturnMeshObject(MeshObject::Handle handle);

    [[nodiscard]] const Material& GetMaterial(UINT index) const;

    /**
     * Resets the command allocator and command list for the given frame.
     */
    void Reset(UINT frameIndex);

    /**
     * Adds commands that setup rendering to the command list.
     * This should be called before each frame.
     */
    void EnqueueRenderSetup();
    void CleanupRenderSetup();

    /**
     * Get a buffer containing indices for the given vertex count.
     * The indices are valid for a vertex buffer that contains a list of quads.
     */
    [[nodiscard]] std::pair<Allocation<ID3D12Resource>, UINT> GetIndexBuffer(UINT vertexCount);

    /**
     * Dispatches rays into the space.
     */
    void DispatchRays();

    /**
     * Copies the raytracing output to the given buffer.
     */
    void CopyOutputToBuffer(Allocation<ID3D12Resource> buffer) const;

    void Update(double delta);

    /**
     * Get the native client.
     */
    NativeClient& GetNativeClient() const;
    
    Camera* GetCamera();
    Light* GetLight();

    /**
     * Get the internal command list.
     */
    [[nodiscard]] ComPtr<ID3D12GraphicsCommandList4> GetCommandList() const;

    /**
     * Allocate a BLAS. 
     */
    BLAS AllocateBLAS(UINT64 resultSize, UINT64 scratchSize);

private:
    struct TLAS
    {
        Allocation<ID3D12Resource> scratch;
        Allocation<ID3D12Resource> result;
        Allocation<ID3D12Resource> instanceDescription;
    };
    
    [[nodiscard]] ComPtr<ID3D12Device5> GetDevice() const;

    void CreateGlobalConstBuffer();
    void UpdateGlobalConstBuffer();
    void CreateShaderResourceHeap(const SpacePipeline& pipeline);
    void InitializePipelineResourceHeap(const SpacePipeline& pipeline);

    void UpdateGlobalShaderResourceHeap();
    void UpdateGSRHeapSize();
    void UpdateGSRHeapContents();
    void UpdateGSRHeapBase() const;
    std::pair<UINT, UINT> GetTextureSlotIndices(const MeshObject* mesh, UINT offset) const;
    std::pair<UINT, UINT> GetTextureSlotIndices(UINT slot, UINT offset) const;
    
    void UpdateOutputResourceView();
    void UpdateAccelerationStructureView() const;
    bool CreateRaytracingPipeline(const SpacePipeline& pipelineDescription);
    void CreateRaytracingOutputBuffer();
    
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateGlobalRootSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateRayGenSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMissSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMaterialSignature() const;

    void CreateShaderBindingTable();
    void CreateTopLevelAS();

    NativeClient& m_nativeClient;
    Resolution m_resolution{};

    Camera m_camera;
    Light m_light;

    Allocation<ID3D12Resource> m_globalConstantBuffer = {};
    UINT64 m_globalConstantBufferSize = 0;
    GlobalConstantBuffer m_globalConstantBufferData = {};
    Mapping<ID3D12Resource, GlobalConstantBuffer> m_globalConstantBufferMapping = {};

    std::vector<ComPtr<IDxcBlob>> m_shaderBlobs = {};
    std::vector<std::unique_ptr<Material>> m_materials = {};

    CommandAllocatorGroup m_commandGroup;

    ComPtr<ID3D12RootSignature> m_globalRootSignature;
    ComPtr<ID3D12RootSignature> m_rayGenSignature;
    ComPtr<ID3D12RootSignature> m_missSignature;

    nv_helpers_dx12::ShaderBindingTableGenerator m_sbtHelper{};
    Allocation<ID3D12Resource> m_sbtStorage;

    ComPtr<ID3D12StateObject> m_rtStateObject;
    ComPtr<ID3D12StateObjectProperties> m_rtStateObjectProperties;
    
    Allocation<ID3D12Resource> m_outputResource;
    bool m_outputResourceFresh = false;

    struct TextureSlot
    {
        UINT size;
        UINT offset;
    };

    Allocation<ID3D12Resource> m_sentinelTexture;
    D3D12_SHADER_RESOURCE_VIEW_DESC m_sentinelTextureViewDescription = {};
    TextureSlot m_textureSlot1 = {0, 0};
    TextureSlot m_textureSlot2 = {0, 0};

    DescriptorHeap m_commonPipelineResourceHeap;
    DescriptorHeap m_globalShaderResourceHeap;
    UINT m_globalShaderResourceHeapSlots = 0;
    D3D12_GPU_DESCRIPTOR_HANDLE m_instanceDataHeap{};
    D3D12_GPU_DESCRIPTOR_HANDLE m_geometryDataHeap{};

    TLAS m_topLevelASBuffers;

    InBufferAllocator m_resultBufferAllocator;
    InBufferAllocator m_scratchBufferAllocator;
    
    GappedList<std::unique_ptr<MeshObject>> m_meshes = {};
    std::vector<std::unique_ptr<MeshObject>> m_meshPool = {};
    std::set<MeshObject::Handle> m_modifiedMeshes = {};
    GappedList<MeshObject*> m_activeMeshes = {};
    std::set<size_t> m_activatedMeshes = {};

    SharedIndexBuffer m_indexBuffer;
};
