// <copyright file="RasterPipeline.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

class ShaderBuffer;

enum class ShaderPreset : BYTE
{
    /**
     * \brief Used for post-processing, using an input texture.
     */
    POST_PROCESSING,

    /**
     * \brief Used to draw 2D elements directly to the screen.
     */
    DRAW_2D,

    /**
     * \brief Used for effects that are used as part of the 3D space.
     */
    SPATIAL_EFFECT
};

/**
 * \brief The topology used by the pipeline. Only valid for SPATIAL_EFFECT.
 */
enum class Topology : BYTE
{
    TRIANGLE,
    LINE
};

/**
 * \brief The filter to use for the texture sampler. Only valid for POST_PROCESSING and DRAW_2D.
 */
enum class Filter : BYTE
{
    LINEAR,
    CLOSEST
};

struct RasterPipelineDescription
{
    wchar_t const* vertexShaderPath;
    wchar_t const* pixelShaderPath;
    ShaderPreset   shaderPreset;
    UINT           bufferSize;

    Topology topology = {};
    Filter   filter   = {};
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
        NativeClient&                    client,
        RasterPipelineDescription const& description,
        NativeErrorFunc                  callback);

    struct Bindings
    {
        struct Draw2dBindings
        {
            ShaderResources::SelectionList<ShaderResources::ConstantBufferViewDescriptor> booleans{};
            ShaderResources::SelectionList<ShaderResources::ShaderResourceViewDescriptor> textures{};
        };

        struct PostProcessingBindings
        {
            ShaderResources::Table::Entry color = ShaderResources::Table::Entry::invalid;
            ShaderResources::Table::Entry depth = ShaderResources::Table::Entry::invalid;
        };

        struct SpatialEffectBindings
        {
            ShaderResources::Table::Entry instanceData = ShaderResources::Table::Entry::invalid;
            ShaderResources::Table::Entry customData   = ShaderResources::Table::Entry::invalid;
        };

        explicit Bindings(ShaderPreset const preset)
        {
            using enum ShaderPreset;

            switch (preset)
            {
            case DRAW_2D:
                m_preset = Draw2dBindings();
                break;
            case POST_PROCESSING:
                m_preset = PostProcessingBindings();
                break;
            case SPATIAL_EFFECT:
                m_preset = SpatialEffectBindings();
                break;
            }
        }

        Draw2dBindings&         Draw2D() { return std::get<Draw2dBindings>(m_preset); }
        PostProcessingBindings& PostProcessing() { return std::get<PostProcessingBindings>(m_preset); }
        SpatialEffectBindings&  SpatialEffect() { return std::get<SpatialEffectBindings>(m_preset); }

    private:
        std::variant<Draw2dBindings, PostProcessingBindings, SpatialEffectBindings> m_preset;
    };

    /**
     * \brief Used by the 3D space to set up the bindings in the shader resources used for all space rendering.
     * \param client The client.
     * \param description The description (builder) of the shader resources.
     * \return The bindings to use for spatial effects.
     */
    static std::shared_ptr<Bindings> SetUpEffectBindings(
        NativeClient const&           client,
        ShaderResources::Description& description);

    struct PipelineConfiguration
    {
        ShaderPreset             preset;
        D3D12_PRIMITIVE_TOPOLOGY topology;
        std::wstring             name;
    };

    struct PipelineObjects
    {
        std::unique_ptr<ShaderBuffer>    shaderBuffer;
        std::shared_ptr<ShaderResources> resources;
        std::shared_ptr<Bindings>        bindings;
        ComPtr<ID3D12PipelineState>      pipelineState;
    };

    /**
     * Create a pipeline from an already initialized pipeline state object and associated root signature.
     */
    RasterPipeline(NativeClient& client, PipelineConfiguration configuration, PipelineObjects objects);

    /**
     * \brief Set the pipeline state object and root signature on the command list. Will not perform resource binding.
     * \param commandList The command list.
     */
    void SetPipeline(ComPtr<ID3D12GraphicsCommandList4> commandList) const;

    /**
     * \brief Bind the resources to the command list.
     * \param commandList The command list.
     */
    void BindResources(ComPtr<ID3D12GraphicsCommandList4> commandList);

    [[nodiscard]] Bindings&                GetBindings() const;
    [[nodiscard]] ShaderPreset             GetPreset() const;
    [[nodiscard]] LPCWSTR                  GetName() const;
    [[nodiscard]] D3D12_PRIMITIVE_TOPOLOGY GetTopology() const;
    [[nodiscard]] ShaderBuffer*            GetShaderBuffer() const;

    void CreateConstantBufferView(
        ShaderResources::Table::Entry                        entry,
        UINT                                                 index,
        ShaderResources::ConstantBufferViewDescriptor const& descriptor);
    void CreateShaderResourceView(
        ShaderResources::Table::Entry                        entry,
        UINT                                                 index,
        ShaderResources::ShaderResourceViewDescriptor const& descriptor);
    void CreateUnorderedAccessView(
        ShaderResources::Table::Entry                         entry,
        UINT                                                  index,
        ShaderResources::UnorderedAccessViewDescriptor const& descriptor);

    /**
     * \brief Set the content of a selection list.
     * \tparam Descriptor The descriptor type.
     * \param selectionList The selection list, must be part of the bindings of this pipeline.
     * \param descriptors The descriptors to set.
     */
    template <class Descriptor>
    void SetSelectionListContent(
        ShaderResources::SelectionList<Descriptor>& selectionList,
        std::vector<Descriptor> const&              descriptors)
    {
        m_resources->SetSelectionListContent(selectionList, descriptors);
    }

    /**
     * \brief Bind an entry of a selection list for active use.
     * \tparam Descriptor The descriptor type.
     * \param commandList The command list.
     * \param selectionList The selection list, must be part of the bindings of this pipeline.
     * \param index The index of the entry to bind.
     */
    template <class Descriptor>
    void BindSelectionIndex(
        ComPtr<ID3D12GraphicsCommandList4> commandList,
        ShaderResources::SelectionList<Descriptor>& selectionList,
        UINT index) { m_resources->BindSelectionListIndex(selectionList, index, commandList); }

private:
    /**
     * \brief Ensure that the resources have been updated at least once. This is required to allow creating descriptors.
     */
    void EnsureFirstUpdate();

    ShaderPreset             m_preset;
    D3D12_PRIMITIVE_TOPOLOGY m_topology;
    std::wstring             m_name;

    std::shared_ptr<ShaderResources> m_resources;
    std::shared_ptr<Bindings>        m_bindings;

    ComPtr<ID3D12PipelineState> m_pipelineState;

    std::unique_ptr<ShaderBuffer> m_shaderBuffer = nullptr;
    bool                          m_update       = false;
};
