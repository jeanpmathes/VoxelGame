﻿// <copyright file="Mesh.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Drawable.hpp"
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

struct MeshDataBuffer // todo: check that c++ structs and respective hlsl structs have same names
{
    DirectX::XMFLOAT4X4 objectToWorld;
    DirectX::XMFLOAT4X4 objectToWorldNormal;
};
#pragma pack(pop)

struct Material;

/**
 * \brief A mesh, positioned in 3D space and target of raytracing.
 */
class Mesh final : public Drawable
{
    DECLARE_OBJECT_SUBCLASS(Mesh)

public:
    explicit Mesh(NativeClient& client);
    void Initialize(UINT materialIndex);

    void Update() override;
    
    void SetNewVertices(const SpatialVertex* vertices, UINT vertexCount);
    void SetNewBounds(const SpatialBounds* bounds, UINT boundsCount);
    
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

    void Accept(Visitor& visitor) override;

protected:
    void DoDataUpload(ComPtr<ID3D12GraphicsCommandList> commandList) override;
    void DoReset() override;

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

    void UpdateGeometryViews(UINT count, UINT stride);

    const Material* m_material = nullptr;

    Allocation<ID3D12Resource> m_instanceDataBuffer = {};
    UINT64 m_instanceDataBufferAlignedSize = 0;
    D3D12_CONSTANT_BUFFER_VIEW_DESC m_instanceDataBufferView = {};
    Mapping<ID3D12Resource, MeshDataBuffer> m_instanceConstantBufferMapping = {};
    
    Allocation<ID3D12Resource> m_sourceGeometryBuffer = {};
    Allocation<ID3D12Resource> m_destinationGeometryBuffer = {};

    D3D12_SHADER_RESOURCE_VIEW_DESC m_geometrySRV = {};
    D3D12_UNORDERED_ACCESS_VIEW_DESC m_geometryUAV = {};

    Allocation<ID3D12Resource> m_usedIndexBuffer = {};
    UINT m_usedIndexCount = 0;

    nv_helpers_dx12::BottomLevelASGenerator m_bottomLevelASGenerator = {};
    BLAS m_blas = {};
    bool m_requiresFreshBLAS = false;

    AnimationController::Handle m_animationHandle = AnimationController::Handle::INVALID;
};
