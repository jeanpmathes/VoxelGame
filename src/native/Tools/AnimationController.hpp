// <copyright file="AnimationController.hpp" company="VoxelGame">
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

namespace anim
{
    constexpr UINT SUBMISSIONS_PER_THREAD_GROUP = 16;
    constexpr UINT MAX_ELEMENTS_PER_SUBMISSION  = 4 * 512;

#pragma pack(push, 4)
    struct Submission
    {
        UINT meshIndex     = 0;
        UINT instanceIndex = 0;

        UINT offset = 0;
        UINT count  = 0;
    };

    struct ThreadGroup
    {
        Submission submissions[SUBMISSIONS_PER_THREAD_GROUP] = {0};
    };
#pragma pack(pop)
}

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
     * \param commandList The command list to use for uploading.
     */
    void Update(ShaderResources& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList);
    /**
     * \brief Runs the animation.
     * \param commandList The command list to use for running.
     */
    void Run(ComPtr<ID3D12GraphicsCommandList4> const& commandList);
    /**
     * \brief Create the BLAS for every mesh that uses this animation.
     * \param commandList The command list to use for creating the BLAS.
     * \param uavs A list that will receive the UAVs for the BLAS.
     */
    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<ID3D12Resource*>* uavs);

private:
    void UpdateThreadGroupData();
    void UploadThreadGroupData(ShaderResources const& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList);

    void CreateBarriers();
    
    ShaderResources::ShaderLocation m_threadGroupDataLocation;
    ShaderResources::ShaderLocation m_inputGeometryListLocation;
    ShaderResources::ShaderLocation m_outputGeometryListLocation;

    ComPtr<ID3DBlob> m_shader = {};

    Bag<Mesh*, Handle> m_meshes        = {};
    IntegerSet<Handle> m_changedMeshes = {};
    IntegerSet<Handle> m_removedMeshes = {};

    ShaderResources::TableHandle  m_resourceTable        = ShaderResources::TableHandle::INVALID;
    ShaderResources::Table::Entry m_threadGroupDataEntry = ShaderResources::Table::Entry::invalid;
    ShaderResources::ListHandle   m_srcGeometryList      = ShaderResources::ListHandle::INVALID;
    ShaderResources::ListHandle   m_dstGeometryList      = ShaderResources::ListHandle::INVALID;

    Allocation<ID3D12Resource>                 m_threadGroupDataBuffer          = {};
    Allocation<ID3D12Resource>                 m_threadGroupDataUploadBuffer    = {};
    std::vector<anim::ThreadGroup>             m_threadGroupData                = {};
    Mapping<ID3D12Resource, anim::ThreadGroup> m_threadGroupDataMapping         = {};
    D3D12_SHADER_RESOURCE_VIEW_DESC            m_threadGroupDataViewDescription = {};

    NativeClient* m_client = {};
    ComPtr<ID3D12PipelineState> m_pipelineState = {};

    std::vector<CD3DX12_RESOURCE_BARRIER> m_entryBarriers = {};
    std::vector<CD3DX12_RESOURCE_BARRIER> m_exitBarriers  = {};
};
