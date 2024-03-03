// <copyright file="Common.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

/**
 * The resolution of a window.
 */
struct Resolution
{
    UINT width  = 0;
    UINT height = 0;

    Resolution operator*(float scale) const;
    bool       operator==(Resolution const& other) const = default;
};

/**
 * \brief Information to set up the raster stage.
 */
struct RasterInfo
{
    CD3DX12_VIEWPORT viewport{0.0f, 0.0f, 0.0f, 0.0f};
    CD3DX12_RECT     scissorRect{0, 0, 0, 0};

    void Set(ComPtr<ID3D12GraphicsCommandList4> commandList) const;
};

inline constexpr UINT FRAME_COUNT = 2;

/**
 * Get the name of a D3D12 object.
 * If the object has no name, an empty string is returned.
 */
std::wstring GetObjectName(ComPtr<ID3D12Object> object);

/**
 * \brief Set the name of a D3D12 object.
 * \param object The object to name.
 * \param name The name to set.
 */
void SetObjectName(ComPtr<ID3D12Object> object, std::wstring const& name);

/**
 * A group of command allocators and a command list.
 */
struct CommandAllocatorGroup
{
    std::array<ComPtr<ID3D12CommandAllocator>, FRAME_COUNT> commandAllocators;
    ComPtr<ID3D12GraphicsCommandList4>                      commandList;
    bool                                                    open = false;

    static void Initialize(ComPtr<ID3D12Device> device, CommandAllocatorGroup* group, D3D12_COMMAND_LIST_TYPE type);

    void Reset(UINT frameIndex, ComPtr<ID3D12PipelineState> pipelineState = nullptr);
    void Close();
};

#define INITIALIZE_COMMAND_ALLOCATOR_GROUP(client, group, type) \
    do { \
        CommandAllocatorGroup::Initialize(client, (group), type); \
        for (UINT n = 0; n < FRAME_COUNT; n++) \
        { \
            NAME_D3D12_OBJECT_INDEXED((group)->commandAllocators, n); \
        } \
        NAME_D3D12_OBJECT((group)->commandList); \
    } while (false)

inline DirectX::XMMATRIX XMMatrixToNormal(DirectX::XMMATRIX const& matrix)
{
    DirectX::XMMATRIX upper = matrix;

    upper.r[0].m128_f32[3] = 0.f;
    upper.r[1].m128_f32[3] = 0.f;
    upper.r[2].m128_f32[3] = 0.f;
    upper.r[3].m128_f32[0] = 0.f;
    upper.r[3].m128_f32[1] = 0.f;
    upper.r[3].m128_f32[2] = 0.f;
    upper.r[3].m128_f32[3] = 1.f;

    DirectX::XMVECTOR det;
    return XMMatrixTranspose(XMMatrixInverse(&det, upper));
}
