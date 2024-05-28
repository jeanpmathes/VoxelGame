﻿// <copyright file="AnimationController.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Bag.hpp"
#include "IntegerSet.hpp"
#include "ShaderResources.hpp"

class NativeClient;
class Mesh;

/**
 * Controls compute-shader based animations and all necessary resources.
 * Each thread group uses 16 by 4 threads, so 16 submissions are processed per thread group.
 */
class AnimationController
{
public:
    enum class Handle : size_t
    {
        INVALID = std::numeric_limits<size_t>::max()
    };

    /**
     * Creates a new animation controller.
     * The shader binds both UAV and SRV resources and occupies one space in each.
     */
    AnimationController(ComPtr<IDxcBlob> const& shader, UINT space);

    void SetupResourceLayout(ShaderResources::Description* description);
    void Initialize(NativeClient& client, ComPtr<ID3D12RootSignature> const& rootSignature);

    void AddMesh(Mesh& mesh);
    void UpdateMesh(Mesh const& mesh);
    void RemoveMesh(Mesh& mesh);

    /**
     * \brief Updates shader resource data, must be called before running the animation.
     * \param resources The shader resources.
     */
    void Update(ShaderResources& resources);
    /**
     * \brief Runs the animation.
     * \param resources The shader resources.
     * \param commandList The command list to use for running.
     */
    void Run(ShaderResources& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList);
    /**
     * \brief Create the BLAS for every mesh that uses this animation.
     * \param commandList The command list to use for creating the BLAS.
     * \param uavs A list that will receive the UAVs for the BLAS.
     */
    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<ID3D12Resource*>* uavs);

private:
    void CreateBarriers();
    
    ShaderResources::ShaderLocation m_threadGroupDataLocation;
    ShaderResources::ShaderLocation m_inputGeometryListLocation;
    ShaderResources::ShaderLocation m_outputGeometryListLocation;

    ComPtr<ID3DBlob> m_shader = {};

    Bag<Mesh*, Handle> m_meshes        = {};
    IntegerSet<Handle> m_changedMeshes = {};
    IntegerSet<Handle> m_removedMeshes = {};

    ShaderResources::Value32 m_workIndex = {};
    ShaderResources::Value32 m_workSize  = {};

    ShaderResources::ConstantHandle m_workIndexConstant = ShaderResources::ConstantHandle::INVALID;
    ShaderResources::ConstantHandle m_workSizeConstant  = ShaderResources::ConstantHandle::INVALID;
    ShaderResources::ListHandle     m_srcGeometryList   = ShaderResources::ListHandle::INVALID;
    ShaderResources::ListHandle     m_dstGeometryList   = ShaderResources::ListHandle::INVALID;

    NativeClient* m_client = {};
    ComPtr<ID3D12PipelineState> m_pipelineState = {};

    std::vector<CD3DX12_RESOURCE_BARRIER> m_entryBarriers = {};
    std::vector<CD3DX12_RESOURCE_BARRIER> m_exitBarriers  = {};
};
