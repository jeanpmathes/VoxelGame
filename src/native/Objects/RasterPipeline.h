// <copyright file="RasterPipeline.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "NativeClient.h"

enum class ShaderPreset : uint8_t
{
    SPACE_3D,
    POST_PROCESSING
};

struct PipelineDescription
{
    const wchar_t* vertexShaderPath;
    const wchar_t* pixelShaderPath;
    ShaderPreset shaderPreset;
};

/**
 * Wraps a pipeline for raster-based rendering.
 */
class RasterPipeline final : public Object
{
    DECLARE_OBJECT_SUBCLASS(RasterPipeline)

public:
    /**
     * Create a new pipeline from a description.
     * Shader compile errors are reported to the callback.
     * Should a shader compile error occur, the pipeline is not created and nullptr is returned.
     */
    static std::unique_ptr<RasterPipeline> Create(
        NativeClient& client,
        const PipelineDescription& description,
        NativeErrorMessageFunc callback);

    /**
     * Create a pipeline from an already initialized pipeline state object and associated root signature.
     */
    RasterPipeline(
        NativeClient& client,
        ComPtr<ID3D12RootSignature> rootSignature,
        ComPtr<ID3D12PipelineState> pipelineState);

    /**
     * Reset the command allocator and command list.
     */
    void Reset(UINT frameIndex) const;

    /**
     * Get the command list of the pipeline.
     */
    [[nodiscard]] ComPtr<ID3D12GraphicsCommandList4> GetCommandList() const;

    /**
     * Get the root signature of the pipeline.
     */
    [[nodiscard]] ComPtr<ID3D12RootSignature> GetRootSignature() const;

private:
    ComPtr<ID3D12RootSignature> m_rootSignature;
    ComPtr<ID3D12PipelineState> m_pipelineState;


    ComPtr<ID3D12CommandAllocator> m_commandAllocators[NativeClient::FRAME_COUNT];
    ComPtr<ID3D12GraphicsCommandList4> m_commandList;
};
