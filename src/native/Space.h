// <copyright file="Space.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "nv_helpers_dx12/ShaderBindingTableGenerator.h"

#include "Common.h"

#include "Objects/Camera.h"
#include "Objects/Light.h"
#include "Objects/MeshObject.h"

class IndexedMeshObject;
class SequencedMeshObject;

struct GlobalConstantBuffer
{
    float time;
    DirectX::XMFLOAT3 lightPosition;
    float minLight;
};

struct ShaderPaths
{
    std::wstring rayGenShader;
    std::wstring missShader;
    std::wstring hitShader;
    std::wstring shadowShader;
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
    void PerformInitialSetupStepTwo(const ShaderPaths& paths);

    SequencedMeshObject& CreateSequencedMeshObject();
    IndexedMeshObject& CreateIndexedMeshObject();

    /**
     * Resets the command allocator and command list for the given frame.
     */
    void Reset(UINT frameIndex) const;

    /**
     * Adds commands that setup rendering to the command list.
     * This should be called before each frame.
     */
    void EnqueueRenderSetup();
    void CleanupRenderSetup() const;

    /**
     * Dispatches rays into the space.
     */
    void DispatchRays() const;

    /**
     * Copies the raytracing output to the given buffer.
     */
    void CopyOutputToBuffer(ComPtr<ID3D12Resource> buffer) const;

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
    void CreateRaytracingPipeline(const ShaderPaths& paths);
    void CreateRaytracingOutputBuffer();

    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateRayGenSignature() const;
    [[nodiscard]] ComPtr<ID3D12RootSignature> CreateMissSignature() const;

    void CreateShaderBindingTable();
    void CreateTopLevelAS();

    NativeClient& m_nativeClient;
    Resolution m_resolution{};

    Camera m_camera;
    Light m_light;
    
    ComPtr<ID3D12Resource> m_globalConstantBuffer;
    GlobalConstantBuffer m_globalConstantBufferData{};

    ComPtr<IDxcBlob> m_rayGenLibrary;
    ComPtr<IDxcBlob> m_missLibrary;
    ComPtr<IDxcBlob> m_hitLibrary;
    ComPtr<IDxcBlob> m_shadowLibrary;

    ComPtr<ID3D12CommandAllocator> m_commandAllocators[FRAME_COUNT];
    ComPtr<ID3D12GraphicsCommandList4> m_commandList;

    ComPtr<ID3D12RootSignature> m_rayGenSignature;
    ComPtr<ID3D12RootSignature> m_missSignature;
    ComPtr<ID3D12RootSignature> m_hitSignatureSequenced;
    ComPtr<ID3D12RootSignature> m_hitSignatureIndexed;
    ComPtr<ID3D12RootSignature> m_shadowSignatureSequenced;
    ComPtr<ID3D12RootSignature> m_shadowSignatureIndexed;

    nv_helpers_dx12::ShaderBindingTableGenerator m_sbtHelper{};
    ComPtr<ID3D12Resource> m_sbtStorage;

    ComPtr<ID3D12StateObject> m_rtStateObject;
    ComPtr<ID3D12StateObjectProperties> m_rtStateObjectProperties;
    ComPtr<ID3D12Resource> m_outputResource;

    ComPtr<ID3D12DescriptorHeap> m_srvUavHeap;

    AccelerationStructureBuffers m_topLevelASBuffers;

    std::vector<std::unique_ptr<MeshObject>> m_meshes = {};
};
