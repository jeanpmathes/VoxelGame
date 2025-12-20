// <copyright file="Space.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

#include "nv_helpers_dx12/ShaderBindingTableGenerator.hpp"

#include "Common.hpp"

#include "Objects/Camera.hpp"
#include "Objects/Effect.hpp"
#include "Objects/Light.hpp"
#include "Objects/Mesh.hpp"
#include "Objects/RasterPipeline.hpp"
#include "Tools/DrawablesGroup.hpp"
#include "Tools/ShaderResources.hpp"

class ShaderBuffer;
class Texture;

struct MaterialDescription
{
    LPWSTR name;
    BOOL   visible;
    BOOL   shadowCaster;
    BOOL   opaque;

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
    UINT   symbolCount;
};

struct SpacePipelineDescription
{
    ShaderFileDescription* shaderFiles;
    UINT                   shaderCount;

    LPWSTR* symbols;

    UINT anisotropy;

    MaterialDescription* materials;
    UINT                 materialCount;

    Texture** textures;
    UINT      textureCountFirstSlot;
    UINT      textureCountSecondSlot;

    UINT customDataBufferSize;

    UINT meshSpoolCount;
    UINT effectSpoolCount;

    NativeErrorFunc onShaderLoadingError;
};

enum class MaterialFlags : BYTE
{
    VISIBLE       = 1 << 0,
    SHADOW_CASTER = 1 << 1,
};

DEFINE_ENUM_FLAG_OPERATORS(MaterialFlags)

#pragma pack(push, 4)
struct GlobalBuffer
{
    float            time;
    DirectX::XMUINT3 textureSize;

    DirectX::XMFLOAT3 lightDirection;
    float             minLight;
    float             minShadow;
};

struct MaterialBuffer
{
    UINT index;
};
#pragma pack(pop)

struct Material
{
    std::wstring                   name{};
    UINT                           index{};
    bool                           isOpaque{};
    std::optional<UINT>            animationID{};
    D3D12_RAYTRACING_GEOMETRY_TYPE geometryType{};
    MaterialFlags                  flags{};

    std::wstring                normalHitGroup{};
    ComPtr<ID3D12RootSignature> normalRootSignature{};

    std::wstring                shadowHitGroup{};
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

    void PerformInitialSetupStepOne(ComPtr<ID3D12CommandQueue> const& commandQueue);
    void PerformResolutionDependentSetup(Resolution const& resolution);
    bool PerformInitialSetupStepTwo(SpacePipelineDescription const& pipeline);

    /**
     * Create a new mesh with a given material. 
     */
    Mesh& CreateMesh(UINT materialIndex);
    /**
     * Create a new effect.
     */
    Effect& CreateEffect(RasterPipeline* pipeline);
    /**
     * Mark a drawable as modified, so that instance data can be updated.
     */
    void MarkDrawableModified(Drawable* drawable);
    /**
     * Activate a drawable for rendering. It must have a valid mesh.
     */
    void ActivateDrawable(Drawable* drawable);
    /**
     * Deactivate a drawable.
     */
    void DeactivateDrawable(Drawable* drawable);
    /**
     * Return a drawable to the creator.
     * The space is allowed to reuse or free the drawable.
     * Therefore, the object should not be used after this call.
     */
    void ReturnDrawable(Drawable* drawable);

    [[nodiscard]] Material const& GetMaterial(UINT index) const;

    /**
     * Resets the command allocator and command list for the given frame.
     */
    void Reset(UINT frameIndex);

    /**
     * Get a buffer containing indices for the given vertex count.
     * The indices are valid for a vertex buffer that contains a list of quads.
     */
    [[nodiscard]] std::pair<Allocation<ID3D12Resource>, UINT> GetIndexBuffer(
        UINT                                 vertexCount,
        std::vector<D3D12_RESOURCE_BARRIER>* barriers);

    struct RenderData
    {
        D3D12_CPU_DESCRIPTOR_HANDLE const* rtv;
        D3D12_CPU_DESCRIPTOR_HANDLE const* dsv;
        RasterInfo const*                  viewport;
    };

    void SpoolUp();

    void Update(double delta);
    void Render(
        Allocation<ID3D12Resource> const& color,
        Allocation<ID3D12Resource> const& depth,
        RenderData const&                 data);
    void CleanupRender();

    /**
     * Get the native client.
     */
    [[nodiscard]] NativeClient& GetNativeClient() const;
    [[nodiscard]] ShaderBuffer* GetCustomDataBuffer() const;

    Camera* GetCamera();
    Light*  GetLight();

    [[nodiscard]] Resolution const& GetResolution() const;

    std::shared_ptr<ShaderResources>          GetShaderResources();
    std::shared_ptr<RasterPipeline::Bindings> GetEffectBindings();

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

    void InitializePipelineResourceViews(SpacePipelineDescription const& pipeline);

    bool CreateRaytracingPipeline(SpacePipelineDescription const& pipelineDescription);
    static std::pair<std::vector<ComPtr<IDxcBlob>>, bool> CompileShaderLibraries(
        NativeClient&                                 client,
        SpacePipelineDescription const&               pipelineDescription,
        nv_helpers_dx12::RayTracingPipelineGenerator& pipeline);
    std::unique_ptr<Material> SetUpMaterial(
        MaterialDescription const&                    description,
        UINT                                          index,
        nv_helpers_dx12::RayTracingPipelineGenerator& pipeline) const;
    void CreateAnimations(SpacePipelineDescription const& pipeline);
    void SetUpStaticResourceLayout(ShaderResources::Description* description);
    void SetUpDynamicResourceLayout(ShaderResources::Description* description);
    void SetUpAnimationResourceLayout(ShaderResources::Description* description);
    void InitializeAnimations();
    void CreateRaytracingOutputBuffer();

    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateRayGenSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMissSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMaterialSignature() const;

    void CreateShaderBindingTable();
    void EnqueueUploads() const;
    void RunAnimations();
    void BuildAccelerationStructures();
    void CreateTLAS();
    void DispatchRays() const;
    void CopyOutputToBuffers(Allocation<ID3D12Resource> const& color, Allocation<ID3D12Resource> const& depth) const;
    void DrawEffects(RenderData const& data);

    void UpdateOutputResourceViews();
    void UpdateTopLevelAccelerationStructureView() const;
    void UpdateGlobalShaderResources();

    NativeClient* m_client;
    Resolution    m_resolution = {};

    InBufferAllocator m_resultBufferAllocator;
    InBufferAllocator m_scratchBufferAllocator;

    Camera m_camera;
    Light  m_light;

    UINT m_meshSpoolCount   = 0;
    UINT m_effectSpoolCount = 0;

    Allocation<ID3D12Resource>            m_globalConstantBuffer        = {};
    UINT64                                m_globalConstantBufferSize    = 0;
    Mapping<ID3D12Resource, GlobalBuffer> m_globalConstantBufferMapping = {};

    std::unique_ptr<ShaderBuffer> m_customDataBuffer = nullptr;

    std::vector<ComPtr<IDxcBlob>>          m_shaderBlobs = {};
    std::vector<std::unique_ptr<Material>> m_materials   = {};

    CommandAllocatorGroup m_commandGroup;

    ComPtr<ID3D12RootSignature> m_globalRootSignature;
    ComPtr<ID3D12RootSignature> m_rayGenSignature;
    ComPtr<ID3D12RootSignature> m_missSignature;

    nv_helpers_dx12::ShaderBindingTableGenerator m_sbtHelper{};
    Allocation<ID3D12Resource>                   m_sbtStorage;

    ComPtr<ID3D12StateObject>           m_rtStateObject;
    ComPtr<ID3D12StateObjectProperties> m_rtStateObjectProperties;

    Allocation<ID3D12Resource> m_colorOutput;
    D3D12_RESOURCE_DESC        m_colorOutputDescription = {};
    Allocation<ID3D12Resource> m_depthOutput;
    D3D12_RESOURCE_DESC        m_depthOutputDescription = {};
    bool                       m_outputResourcesFresh   = false;

    struct TextureSlot
    {
        UINT                          size  = 0;
        ShaderResources::Table::Entry entry = ShaderResources::Table::Entry::invalid;
    };

    Texture*                        m_sentinelTexture    = nullptr;
    D3D12_SHADER_RESOURCE_VIEW_DESC m_sentinelTextureSRV = {};
    TextureSlot                     m_textureSlot1       = {};
    TextureSlot                     m_textureSlot2       = {};

    std::shared_ptr<ShaderResources>          m_globalShaderResources;
    ShaderResources::Table::Entry             m_rtColorDataForRasterEntry = ShaderResources::Table::Entry::invalid;
    ShaderResources::Table::Entry             m_rtDepthDataForRasterEntry = ShaderResources::Table::Entry::invalid;
    std::shared_ptr<RasterPipeline::Bindings> m_effectBindings;

    ShaderResources::TableHandle  m_unchangedCommonResourceHandle = ShaderResources::TableHandle::INVALID;
    ShaderResources::TableHandle  m_changedCommonResourceHandle   = ShaderResources::TableHandle::INVALID;
    ShaderResources::Table::Entry m_colorOutputEntry              = ShaderResources::Table::Entry::invalid;
    ShaderResources::Table::Entry m_depthOutputEntry              = ShaderResources::Table::Entry::invalid;
    ShaderResources::Table::Entry m_bvhEntry                      = ShaderResources::Table::Entry::invalid;
    ShaderResources::ListHandle   m_meshInstanceDataList          = ShaderResources::ListHandle::INVALID;
    ShaderResources::ListHandle   m_meshGeometryBufferList        = ShaderResources::ListHandle::INVALID;

    Drawable::BaseContainer m_drawables;
    DrawablesGroup<Mesh>    m_meshes{*m_client, m_drawables};
    DrawablesGroup<Effect>  m_effects{*m_client, m_drawables};
    std::vector<Drawables*> m_drawableGroups = {&m_meshes, &m_effects};

    nv_helpers_dx12::TopLevelASGenerator m_tlasGenerator;
    TLAS                                 m_topLevelASBuffers;

    std::vector<AnimationController> m_animations = {};

    SharedIndexBuffer m_indexBuffer;

    std::vector<ID3D12Resource*> m_uavs;
};
