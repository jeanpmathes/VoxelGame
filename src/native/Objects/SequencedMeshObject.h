// <copyright file="SequencedMeshObject.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "MeshObject.h"

/**
 * \brief An object that has a mesh defined by a sequence of vertices.
 */
class SequencedMeshObject final : public MeshObject
{
    DECLARE_OBJECT_SUBCLASS(SequencedMeshObject)

public:
    explicit SequencedMeshObject(NativeClient& client);

    void SetNewMesh(const SpatialVertex* vertices, UINT vertexCount);

    [[nodiscard]] bool IsMeshModified() const override;

    void EnqueueMeshUpload(ComPtr<ID3D12GraphicsCommandList> commandList) override;
    void CleanupMeshUpload() override;

    void SetupHitGroup(
        nv_helpers_dx12::ShaderBindingTableGenerator& sbt,
        StandardShaderArguments& shaderArguments) override;

    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList) override;
    ComPtr<ID3D12Resource> GetBLAS() override;

    static ComPtr<ID3D12RootSignature> CreateRootSignature(ComPtr<ID3D12Device5> device);

private:
    ComPtr<ID3D12Resource> m_vertexBufferUpload = {};

    ComPtr<ID3D12Resource> m_vertexBuffer = {};

    UINT m_vertexCount = 0;
    AccelerationStructureBuffers m_blas = {};
};
