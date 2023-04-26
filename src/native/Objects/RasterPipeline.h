// <copyright file="RasterPipeline.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

class ShaderBuffer;

enum class ShaderPreset : BYTE
{
    POST_PROCESSING,
    DRAW_2D,
};

struct PipelineDescription
{
    const wchar_t* vertexShaderPath;
    const wchar_t* pixelShaderPath;
    ShaderPreset shaderPreset;
    uint64_t bufferSize;
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
        NativeClient& client, ShaderPreset preset,
        std::unique_ptr<ShaderBuffer> shaderBuffer,
        ComPtr<ID3D12DescriptorHeap> descriptorHeap,
        ComPtr<ID3D12RootSignature> rootSignature,
        ComPtr<ID3D12PipelineState> pipelineState);

    /**
     * Set the pipeline on the command list.
     */
    void SetPipeline(ComPtr<ID3D12GraphicsCommandList4> commandList) const;

    /**
     * Get the root signature of the pipeline.
     */
    [[nodiscard]] ComPtr<ID3D12RootSignature> GetRootSignature() const;

    /**
     * Get the shader buffer of the pipeline, or nullptr if none.
     */
    [[nodiscard]] ShaderBuffer* GetShaderBuffer() const;

    /**
     * Create a resource view for a single primary resource, apart from the potential shader buffer.
     */
    void CreateResourceView(ComPtr<ID3D12Resource> resource) const;

    /**
     * Create resource views for a set of constant buffers, followed by a set of textures.
     */
    void CreateResourceViews(
        const std::vector<D3D12_CONSTANT_BUFFER_VIEW_DESC>& cbuffers,
        const std::vector<std::tuple<ComPtr<ID3D12Resource>, D3D12_SHADER_RESOURCE_VIEW_DESC>>& textures);

    /**
     * Setup the descriptor heap for the pipeline.
     */
    void SetupHeaps(ComPtr<ID3D12GraphicsCommandList4> commandList) const;

    /**
     * Setup the root descriptor table for the pipeline.
     */
    void SetupRootDescriptorTable(ComPtr<ID3D12GraphicsCommandList4> commandList) const;

    /**
     * Bind a descriptor on the heap, created with e.g. CreateResourceViews, to a slot in the root signature.
     */
    void BindDescriptor(ComPtr<ID3D12GraphicsCommandList4> commandList, UINT slot, UINT descriptor) const;

    [[nodiscard]] UINT GetResourceSlot(UINT index) const;
    [[nodiscard]] D3D12_CPU_DESCRIPTOR_HANDLE GetCpuResourceHandle(UINT index) const;
    [[nodiscard]] D3D12_GPU_DESCRIPTOR_HANDLE GetGpuResourceHandle(UINT index) const;

private:
    ShaderPreset m_preset;
    ComPtr<ID3D12DescriptorHeap> m_descriptorHeap;
    ComPtr<ID3D12RootSignature> m_rootSignature;
    ComPtr<ID3D12PipelineState> m_pipelineState;
    
    std::unique_ptr<ShaderBuffer> m_shaderBuffer = nullptr;
};
