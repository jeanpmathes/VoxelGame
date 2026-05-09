// <copyright file="AnimationController.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Bag.hpp"
#include "IntegerSet.hpp"
#include "ShaderLocation.hpp"
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
    AnimationController(ComPtr<IDxcBlob> const& shaderBlob, UINT space);

    void SetUpResourceLayout(ShaderResources::Description* description);
    void Initialize(NativeClient& usedClient, ComPtr<ID3D12RootSignature> const& rootSignature);

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
    void Run(ShaderResources const& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList);
    /**
     * \brief Create the BLAS for every mesh that uses this animation.
     * \param commandList The command list to use for creating the BLAS.
     * \param uavs A list that will receive the UAVs for the BLAS.
     */
    void CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<ID3D12Resource*>* uavs);

private:
    void CreateBarriers();

    ShaderLocation threadGroupDataLocation;
    ShaderLocation inputGeometryListLocation;
    ShaderLocation outputGeometryListLocation;

    ComPtr<ID3DBlob> shader = {};

    Bag<Mesh*, Handle> meshes        = {};
    IntegerSet<Handle> changedMeshes = {};
    IntegerSet<Handle> removedMeshes = {};

    ShaderResources::Value32 workIndex = {};
    ShaderResources::Value32 workSize  = {};

    ShaderResources::ConstantHandle workIndexConstant = ShaderResources::ConstantHandle::INVALID;
    ShaderResources::ConstantHandle workSizeConstant  = ShaderResources::ConstantHandle::INVALID;
    ShaderResources::ListHandle     srcGeometryList   = ShaderResources::ListHandle::INVALID;
    ShaderResources::ListHandle     dstGeometryList   = ShaderResources::ListHandle::INVALID;

    NativeClient*               client        = {};
    ComPtr<ID3D12PipelineState> pipelineState = {};

    std::vector<CD3DX12_RESOURCE_BARRIER> entryBarriers = {};
    std::vector<CD3DX12_RESOURCE_BARRIER> exitBarriers  = {};
};
