// <copyright file="Effect.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

class RasterPipeline;

#pragma pack(push, 4)
struct EffectDataBuffer
{
    DirectX::XMFLOAT4X4 pvm;
    float               zNear;
    float               zFar;
};
#pragma pack(pop)

struct EffectVertex
{
    DirectX::XMFLOAT3 position;
    UINT              data;
};

/**
 * \brief An effect, rendered in the 3D scene using raster-based techniques.
 */
class Effect final : public Drawable
{
    DECLARE_OBJECT_SUBCLASS(Effect)

public:
    explicit Effect(NativeClient& client);
    void     Initialize(RasterPipeline& pipeline);

    void Update() override;

    /**
     * \brief Set new vertices for this effect.
     * \param vertices The new vertices, must be an array of at least vertexCount elements.
     * \param vertexCount The number of vertices.
     */
    void SetNewVertices(EffectVertex const* vertices, UINT vertexCount);

    /**
     * \brief Draw this effect. May only be called by the space class.
     * \param commandList The command list to use for drawing.
     */
    void Draw(ComPtr<ID3D12GraphicsCommandList4> const& commandList) const;

    void Accept(Visitor& visitor) override;

protected:
    void DoDataUpload(
        ComPtr<ID3D12GraphicsCommandList> const& commandList,
        std::vector<D3D12_RESOURCE_BARRIER>*     barriers) override;
    void DoReset() override;

private:
    RasterPipeline* m_pipeline = nullptr;

    Allocation<ID3D12Resource>                m_instanceConstantDataBuffer            = {};
    UINT64                                    m_instanceConstantDataBufferAlignedSize = 0;
    D3D12_CONSTANT_BUFFER_VIEW_DESC           m_instanceConstantDataBufferView        = {};
    Mapping<ID3D12Resource, EffectDataBuffer> m_instanceConstantBufferMapping = {};

    Allocation<ID3D12Resource> m_geometryBuffer = {};
    D3D12_VERTEX_BUFFER_VIEW   m_geometryVBV    = {};
};
