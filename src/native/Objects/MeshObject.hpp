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

struct SpatialBounds
{
    D3D12_RAYTRACING_AABB aabb;
    DirectX::XMUINT4 data;
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

class Material;

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
    void SetNewVertices(const SpatialVertex* vertices, UINT vertexCount);
    void SetNewBounds(const SpatialBounds* bounds, UINT boundsCount);

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
    CreateBottomLevelASFromVertices(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers) const;

    [[nodiscard]] AccelerationStructureBuffers
    CreateBottomLevelASFromBounds(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> boundsBuffers) const;

private:
    const Material& m_material;

    Allocation<ID3D12Resource> m_instanceConstantBuffer = {};
    UINT64 m_instanceConstantBufferAlignedSize = 0;
    InstanceConstantBuffer m_instanceConstantBufferData = {};

    Allocation<ID3D12Resource> m_geometryBufferUpload = {};
    Allocation<ID3D12Resource> m_geometryBuffer = {};
    UINT m_geometryElementCount = 0;

    Allocation<ID3D12Resource> m_usedIndexBuffer = {};
    UINT m_usedIndexCount = 0;

    AccelerationStructureBuffers m_blas = {};

    std::optional<Handle> m_handle = std::nullopt;
    bool m_enabled = true;
    bool m_modified = false;

    bool m_uploadRequired = false;
    bool m_uploadEnqueued = false;
};
