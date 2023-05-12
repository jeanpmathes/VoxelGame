// <copyright file="MeshObject.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <optional>

#include "SpatialObject.h"

struct SpatialVertex
{
    DirectX::XMFLOAT3 position;
    UINT data;
};

struct InstanceConstantBuffer
{
    DirectX::XMFLOAT4X4 objectToWorld;
    DirectX::XMFLOAT4X4 objectToWorldNormal;
};

struct StandardShaderArguments
{
    void* heap;
    void* globalBuffer;
    void* instanceBuffer;
};

struct AccelerationStructureBuffers
{
    ComPtr<ID3D12Resource> scratch;
    ComPtr<ID3D12Resource> result;
    ComPtr<ID3D12Resource> instanceDesc;
};

/**
 * \brief An object that has a mesh of any kind.
 */
class MeshObject final : public SpatialObject
{
    DECLARE_OBJECT_SUBCLASS(MeshObject)

public:
    explicit MeshObject(NativeClient& client, UINT materialIndex);

    void Update();

    void SetEnabledState(bool enabled);
    void SetNewMesh(const SpatialVertex* vertices, UINT vertexCount, const UINT* indices, UINT indexCount);

    [[nodiscard]] bool IsMeshModified() const;
    [[nodiscard]] bool IsEnabled() const;

    /**
     * Enqueues commands to upload the mesh to the GPU.
     * Should only be called when the mesh is modified.
     */
    void EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList);

    /**
     * Finalizes the mesh upload.
     * Can be called every frame, but only when all commands have been executed.
     */
    void CleanupMeshUpload();

    void FillArguments(StandardShaderArguments& shaderArguments) const;

    void SetupHitGroup(
        nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
        StandardShaderArguments& shaderArguments) const;

    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList);
    ComPtr<ID3D12Resource> GetBLAS();

    [[nodiscard]] UINT GetMaterialIndex() const;

    using Handle = std::list<std::unique_ptr<MeshObject>>::iterator;

    /**
     * Associate this object with a handle. This is performed by the space automatically.
     */
    void AssociateWithHandle(Handle handle);

    /**
     * Free this object.
     */
    void Free() const;

protected:
    [[nodiscard]] AccelerationStructureBuffers
    CreateBottomLevelAS(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>> vertexBuffers,
        std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>> indexBuffers = {}) const;

private:
    UINT m_materialIndex;
    
    ComPtr<ID3D12Resource> m_instanceConstantBuffer = nullptr;
    InstanceConstantBuffer m_instanceConstantBufferData = {};

    ComPtr<ID3D12Resource> m_vertexBufferUpload = {};
    ComPtr<ID3D12Resource> m_indexBufferUpload = {};

    ComPtr<ID3D12Resource> m_vertexBuffer = {};
    ComPtr<ID3D12Resource> m_indexBuffer = {};

    UINT m_vertexCount = 0;
    UINT m_indexCount = 0;
    AccelerationStructureBuffers m_blas = {};

    std::optional<Handle> m_handle = std::nullopt;
    bool m_enabled = true;
    bool m_modified = false;
};
