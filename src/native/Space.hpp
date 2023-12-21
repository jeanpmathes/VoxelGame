// <copyright file="Space.hpp" company="VoxelGame">
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
#include "Tools/ShaderResources.hpp"

class ShaderBuffer;
class Texture;

struct MaterialDescription
{
    LPWSTR name;
    BOOL visible;
    BOOL shadowCaster;
    BOOL opaque;

    BOOL isAnimated;
    UINT animationShaderIndex;

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

    UINT customDataBufferSize;

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
    float minShadow;
    
    DirectX::XMUINT2 textureSize;
};

struct MaterialConstantBuffer
{
    UINT index;
};
#pragma pack(pop)

struct Material
{
    std::wstring name{};
    UINT index{};
    bool isOpaque{};
    std::optional<UINT> animationID{};
    D3D12_RAYTRACING_GEOMETRY_TYPE geometryType{};
    MaterialFlags flags{};

    std::wstring normalHitGroup{};
    ComPtr<ID3D12RootSignature> normalRootSignature{};

    std::wstring shadowHitGroup{};
    ComPtr<ID3D12RootSignature> shadowRootSignature{};

    Allocation<ID3D12Resource> materialConstantBuffer{};

    [[nodiscard]] bool IsAnimated() const;
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
     * Get a buffer containing indices for the given vertex count.
     * The indices are valid for a vertex buffer that contains a list of quads.
     */
    [[nodiscard]] std::pair<Allocation<ID3D12Resource>, UINT> GetIndexBuffer(UINT vertexCount);

    void Update(double delta);
    void Render(double delta, Allocation<ID3D12Resource> outputBuffer);
    void CleanupRender();

    /**
     * Get the native client.
     */
    NativeClient& GetNativeClient() const;
    [[nodiscard]] ShaderBuffer* GetCustomDataBuffer() const;
    
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

    void InitializePipelineResourceViews(const SpacePipeline& pipeline);
    
    bool CreateRaytracingPipeline(const SpacePipeline& pipelineDescription);
    static std::pair<std::vector<ComPtr<IDxcBlob>>, bool> CompileShaderLibraries(
        NativeClient& nativeClient,
        const SpacePipeline& pipelineDescription,
        nv_helpers_dx12::RayTracingPipelineGenerator& pipeline);
    std::unique_ptr<Material> SetupMaterial(const MaterialDescription& description, UINT index,
                                            nv_helpers_dx12::RayTracingPipelineGenerator& pipeline) const;
    void CreateAnimations(const SpacePipeline& pipeline);
    void SetupStaticResourceLayout(ShaderResources::Description* description);
    void SetupDynamicResourceLayout(ShaderResources::Description* description);
    void SetupAnimationResourceLayout(ShaderResources::Description* description);
    void InitializeAnimations();
    void CreateRaytracingOutputBuffer();
    
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateRayGenSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMissSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMaterialSignature() const;

    void CreateShaderBindingTable();
    void EnqueueUploads();
    void RunAnimations();
    void BuildAccelerationStructures();
    void CreateTLAS();
    void DispatchRays() const;
    void CopyOutputToBuffer(Allocation<ID3D12Resource> buffer) const;

    void UpdateOutputResourceView();
    void UpdateTopLevelAccelerationStructureView();
    void UpdateGlobalShaderResources();

    NativeClient& m_nativeClient; // todo: make pointer, check other offenders of warning
    Resolution m_resolution{};

    Camera m_camera;
    Light m_light;

    Allocation<ID3D12Resource> m_globalConstantBuffer = {};
    UINT64 m_globalConstantBufferSize = 0;
    double m_renderTime = 0.0;
    Mapping<ID3D12Resource, GlobalConstantBuffer> m_globalConstantBufferMapping = {};

    std::unique_ptr<ShaderBuffer> m_customDataBuffer = nullptr;

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
        UINT size = 0;
        ShaderResources::Table::Entry entry = ShaderResources::Table::Entry::invalid;
    };

    Allocation<ID3D12Resource> m_sentinelTexture;
    D3D12_SHADER_RESOURCE_VIEW_DESC m_sentinelTextureViewDescription = {};
    TextureSlot m_textureSlot1 = {};
    TextureSlot m_textureSlot2 = {};

    ShaderResources m_globalShaderResources;

    ShaderResources::TableHandle m_commonResourceTable = ShaderResources::TableHandle::INVALID;
    ShaderResources::Table::Entry m_outputTextureEntry = ShaderResources::Table::Entry::invalid;
    ShaderResources::Table::Entry m_bvhEntry = ShaderResources::Table::Entry::invalid;
    ShaderResources::ListHandle m_meshInstanceDataList = ShaderResources::ListHandle::INVALID;
    ShaderResources::ListHandle m_meshGeometryBufferList = ShaderResources::ListHandle::INVALID;

    TLAS m_topLevelASBuffers;

    InBufferAllocator m_resultBufferAllocator;
    InBufferAllocator m_scratchBufferAllocator;

    Bag<std::unique_ptr<MeshObject>> m_meshes = {};
    std::vector<std::unique_ptr<MeshObject>> m_meshPool = {};
    IntegerSet<MeshObject::Handle> m_modifiedMeshes = {};
    Bag<MeshObject*> m_activeMeshes = {};
    IntegerSet<> m_activatedMeshes = {};

    std::vector<AnimationController> m_animations = {};

    SharedIndexBuffer m_indexBuffer;
};
