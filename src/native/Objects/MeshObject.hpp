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

struct InstanceConstantBuffer
{
    DirectX::XMFLOAT4X4 objectToWorld;
    DirectX::XMFLOAT4X4 objectToWorldNormal;
};
#pragma pack(pop)

class Material;

/**
 * \brief An object that has a mesh of any kind.
 */
class MeshObject final : public SpatialObject
{
    DECLARE_OBJECT_SUBCLASS(MeshObject)

public:
    explicit MeshObject(NativeClient& client, UINT materialIndex);

    enum class Handle : size_t
    {
    };
    
    void Update();

    void SetEnabledState(bool enabled);
    void SetNewVertices(const SpatialVertex* vertices, UINT vertexCount);
    void SetNewBounds(const SpatialBounds* bounds, UINT boundsCount);
    
    [[nodiscard]] std::optional<size_t> GetActiveIndex() const;
    [[nodiscard]] const Material& GetMaterial() const;

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

    /**
     * Create views for the instance data and geometry buffers on a given descriptor heap.
     */
    void CreateInstanceResourceViews(const DescriptorHeap& heap, UINT data, UINT geometry) const;

    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList);
    Allocation<ID3D12Resource> GetBLAS();

    /**
     * Associate this object with a handle. This is performed by the space automatically.
     */
    void AssociateWithHandle(Handle handle);

    /**
     * Free this object.
     */
    void Free();

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
    void UpdateActiveState();
    void UpdateGeometryBufferView(UINT stride);
    
    const Material& m_material;

    Allocation<ID3D12Resource> m_instanceDataBuffer = {};
    UINT64 m_instanceDataBufferAlignedSize = 0;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_instanceDataBufferView = {};
    Mapping<ID3D12Resource, InstanceConstantBuffer> m_instanceConstantBufferMapping = {};

    Allocation<ID3D12Resource> m_geometryBufferUpload = {};
    Allocation<ID3D12Resource> m_geometryBuffer = {};
    D3D12_SHADER_RESOURCE_VIEW_DESC m_geometryBufferView = {};
    UINT m_geometryElementCount = 0;

    Allocation<ID3D12Resource> m_usedIndexBuffer = {};
    UINT m_usedIndexCount = 0;

    AccelerationStructureBuffers m_blas = {};

    std::optional<Handle> m_handle = std::nullopt;
    std::optional<size_t> m_active = std::nullopt;
    bool m_enabled = true;

    bool m_uploadRequired = false;
    bool m_uploadEnqueued = false;
};
