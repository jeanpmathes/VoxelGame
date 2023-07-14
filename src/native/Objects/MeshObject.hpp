// <copyright file="MeshObject.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <optional>

#include "SpatialObject.hpp"
#include "../Common.hpp"

#pragma pack(push, 4)
struct SpatialVertex
{
    DirectX::XMFLOAT3 position;
    UINT data;
};
#pragma pack(pop)

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
    void SetNewMesh(const SpatialVertex* vertices, UINT vertexCount);

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
    Allocation<ID3D12Resource> GetBLAS();

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
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers) const;

private:
    UINT m_materialIndex;

    Allocation<ID3D12Resource> m_instanceConstantBuffer = {};
    UINT64 m_instanceConstantBufferAlignedSize = 0;
    InstanceConstantBuffer m_instanceConstantBufferData = {};

    Allocation<ID3D12Resource> m_vertexBufferUpload = {};
    Allocation<ID3D12Resource> m_vertexBuffer = {};
    UINT m_vertexCount = 0;

    Allocation<ID3D12Resource> m_usedIndexBuffer = {};
    UINT m_usedIndexCount = 0;
    
    AccelerationStructureBuffers m_blas = {};

    std::optional<Handle> m_handle = std::nullopt;
    bool m_enabled = true;
    bool m_modified = false;
    
    bool m_uploadRequired = false;
    bool m_uploadEnqueued = false;
};
