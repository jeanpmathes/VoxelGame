// <copyright file="MeshObject.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "SpatialObject.h"

struct SpatialVertex
{
    DirectX::XMFLOAT3 position;
    DirectX::XMFLOAT4 color;
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
class MeshObject : public SpatialObject
{
    DECLARE_OBJECT_SUBCLASS(MeshObject)

public:
    explicit MeshObject(NativeClient& client);

    void Update();

    [[nodiscard]] virtual bool IsMeshModified() const = 0;

    /**
     * Enqueues commands to upload the mesh to the GPU.
     * Should only be called when the mesh is modified.
     */
    virtual void EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList) = 0;

    /**
     * Finalizes the mesh upload.
     * Can be called every frame, but only when all commands have been executed.
     */
    virtual void CleanupMeshUpload() = 0;

    void FillArguments(StandardShaderArguments& shaderArguments) const;

    virtual void SetupHitGroup(
        nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
        StandardShaderArguments& shaderArguments) = 0;

    virtual void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList) = 0;
    virtual ComPtr<ID3D12Resource> GetBLAS() = 0;

protected:
    [[nodiscard]] AccelerationStructureBuffers
    CreateBottomLevelAS(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>> vertexBuffers,
        std::vector<std::pair<ComPtr<ID3D12Resource>, uint32_t>> indexBuffers = {}) const;

private:
    ComPtr<ID3D12Resource> m_instanceConstantBuffer = nullptr;
    InstanceConstantBuffer m_instanceConstantBufferData = {};
};
