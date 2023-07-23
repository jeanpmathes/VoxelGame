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

struct GlobalConstantBuffer
{
    float time;
    DirectX::XMFLOAT3 lightDirection;
    float minLight;
};

struct MaterialDescription
{
    LPWSTR debugName;

    LPWSTR closestHitSymbol;
    LPWSTR shadowHitSymbol;
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

    NativeErrorMessageFunc onShaderLoadingError;
};

class SpacePipeline
{
public:
    ShaderFileDescription* shaderFiles;
    LPWSTR* symbols;
    MaterialDescription* materials;
    SpacePipelineDescription description;
};

class Material
{
public:
    std::wstring normalHitGroup;
    ComPtr<ID3D12RootSignature> normalRootSignature;

    std::wstring shadowHitGroup;
    ComPtr<ID3D12RootSignature> shadowRootSignature;
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

    MeshObject& CreateMeshObject(UINT materialIndex);
    void FreeMeshObject(MeshObject::Handle handle);

    [[nodiscard]] const Material& GetMaterial(UINT index) const;

    /**
     * Resets the command allocator and command list for the given frame.
     */
    void Reset(UINT frameIndex) const;

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
    void DispatchRays() const;

    /**
     * Copies the raytracing output to the given buffer.
     */
    void CopyOutputToBuffer(Allocation<ID3D12Resource> buffer) const;

    void Update(double delta);

    Camera* GetCamera();
    Light* GetLight();

    /**
     * Get the internal command list.
     */
    [[nodiscard]] ComPtr<ID3D12GraphicsCommandList4> GetCommandList() const;
    
private:
    [[nodiscard]] ComPtr<ID3D12Device5> GetDevice() const;

    void CreateGlobalConstBuffer();
    void UpdateGlobalConstBuffer() const;
    void CreateShaderResourceHeap();
    void UpdateShaderResourceHeap() const;
    bool CreateRaytracingPipeline(const SpacePipeline& pipelineDescription);
    void CreateRaytracingOutputBuffer();

    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateRayGenSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMissSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMaterialSignature() const;

    void CreateShaderBindingTable();
    void CreateTopLevelAS();
    
    NativeClient& m_nativeClient;
    Resolution m_resolution{};

    Camera m_camera;
    Light m_light;

    Allocation<ID3D12Resource> m_globalConstantBuffer;
    UINT64 m_globalConstantBufferSize = 0;
    GlobalConstantBuffer m_globalConstantBufferData = {};

    std::vector<ComPtr<IDxcBlob>> m_shaderBlobs = {};
    std::vector<std::unique_ptr<Material>> m_materials = {};

    CommandAllocatorGroup m_commandGroup;

    ComPtr<ID3D12RootSignature> m_rayGenSignature;
    ComPtr<ID3D12RootSignature> m_missSignature;
    
    nv_helpers_dx12::ShaderBindingTableGenerator m_sbtHelper{};
    Allocation<ID3D12Resource> m_sbtStorage;

    ComPtr<ID3D12StateObject> m_rtStateObject;
    ComPtr<ID3D12StateObjectProperties> m_rtStateObjectProperties;
    ComPtr<ID3D12RootSignature> m_rtGlobalRootSignature;
    Allocation<ID3D12Resource> m_outputResource;

    ComPtr<ID3D12DescriptorHeap> m_srvUavHeap;

    AccelerationStructureBuffers m_topLevelASBuffers;

    std::list<std::unique_ptr<MeshObject>> m_meshes = {};

    std::vector<UINT> m_indices = {};
    Allocation<ID3D12Resource> m_sharedIndexBuffer = {};
    UINT m_sharedIndexCount = 0;
    std::vector<std::pair<Allocation<ID3D12Resource>, Allocation<ID3D12Resource>>> m_indexBufferUploads = {};
};
