// <copyright file="MeshObject.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include <optional>

#include "SpatialObject.hpp"
#include "Tools/InBufferAllocator.hpp"

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

struct Material;

/**
 * \brief An object that has a mesh of any kind.
 */
class MeshObject final : public SpatialObject
{
    DECLARE_OBJECT_SUBCLASS(MeshObject)

public:
    explicit MeshObject(NativeClient& client);
    void Initialize(UINT materialIndex);

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
     * Get the number of units (quads, bounds) in the geometry buffer.
     */
    [[nodiscard]] UINT GetGeometryUnitCount() const;
    /**
     * Get the geometry buffer.
     * If this object is animated, this will be the destination buffer.
     */
    [[nodiscard]] Allocation<ID3D12Resource> GetGeometryBuffer() const;

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

    [[nodiscard]] ShaderResources::ConstantBufferViewDescriptor GetInstanceDataViewDescriptor() const;
    [[nodiscard]] ShaderResources::ShaderResourceViewDescriptor GetGeometryBufferViewDescriptor() const;
    [[nodiscard]] ShaderResources::ShaderResourceViewDescriptor GetAnimationSourceBufferViewDescriptor() const;
    [[nodiscard]] ShaderResources::UnorderedAccessViewDescriptor GetAnimationDestinationBufferViewDescriptor() const;

    /**
     * \brief Create the BLAS for this mesh.
     * \param commandList The command list to use.
     * \param uavs The UAVs to use for the BLAS.
     * \param isForAnimation Whether the BLAS is created for animation. If true and the mesh is modified and a BLAS will be created later anyway, this call will be ignored.
     */
    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList, std::vector<ID3D12Resource*>* uavs,
                    bool isForAnimation = false);
    const BLAS& GetBLAS();

    void SetAnimationHandle(AnimationController::Handle handle);
    [[nodiscard]] AnimationController::Handle GetAnimationHandle() const;
    
    /**
     * Associate this object with a handle. This is performed by the space automatically.
     */
    void AssociateWithHandle(Handle handle);

    /**
     * Return this object to the space. This will allow the space to reuse the object later.
     */
    void Return();

private:
    void CreateBottomLevelASFromVertices(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> vertexBuffers,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> indexBuffers);

    void CreateBottomLevelASFromBounds(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        std::vector<std::pair<Allocation<ID3D12Resource>, uint32_t>> boundsBuffers);

    void CreateBottomLevelAS(
        ComPtr<ID3D12GraphicsCommandList4> commandList);

    Allocation<ID3D12Resource>& GeometryBuffer();
    
    void UpdateActiveState();
    void UpdateGeometryViews(UINT stride);

    const Material* m_material = nullptr;

    Allocation<ID3D12Resource> m_instanceDataBuffer = {};
    UINT64 m_instanceDataBufferAlignedSize = 0;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_instanceDataBufferView = {};
    Mapping<ID3D12Resource, InstanceConstantBuffer> m_instanceConstantBufferMapping = {};

    Allocation<ID3D12Resource> m_geometryBufferUpload = {};
    Allocation<ID3D12Resource> m_sourceGeometryBuffer = {};
    Allocation<ID3D12Resource> m_destinationGeometryBuffer = {};
    UINT m_geometryElementCount = 0;

    D3D12_SHADER_RESOURCE_VIEW_DESC m_geometrySRV = {};
    D3D12_UNORDERED_ACCESS_VIEW_DESC m_geometryUAV = {};

    Allocation<ID3D12Resource> m_usedIndexBuffer = {};
    UINT m_usedIndexCount = 0;

    nv_helpers_dx12::BottomLevelASGenerator m_bottomLevelASGenerator = {};
    BLAS m_blas = {};
    bool m_requiresFreshBLAS = false;

    std::optional<Handle> m_handle = std::nullopt;
    std::optional<size_t> m_active = std::nullopt;
    bool m_enabled = true;

    AnimationController::Handle m_animationHandle = AnimationController::Handle::INVALID;

    bool m_uploadRequired = false;
    bool m_uploadEnqueued = false;
};
