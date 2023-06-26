// <copyright file="Common.h" company="VoxelGame">
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
    UINT width = 0;
    UINT height = 0;
};

/**
 * All resources required to build an acceleration structure.
 */
struct AccelerationStructureBuffers
{
    Allocation<ID3D12Resource> scratch;
    Allocation<ID3D12Resource> result;
    Allocation<ID3D12Resource> instanceDesc;
};

inline constexpr UINT FRAME_COUNT = 2;

/**
 * Get the name of a D3D12 object.
 */
std::wstring GetObjectName(ComPtr<ID3D12Object> object);

/**
 * A group of command allocators and a command list.
 */
struct CommandAllocatorGroup
{
    ComPtr<ID3D12CommandAllocator> commandAllocators[FRAME_COUNT];
    ComPtr<ID3D12GraphicsCommandList4> commandList;

    static void Initialize(ComPtr<ID3D12Device> device, CommandAllocatorGroup* group, D3D12_COMMAND_LIST_TYPE type);

    void Reset(UINT frameIndex, ComPtr<ID3D12PipelineState> pipelineState = nullptr) const;
    void Close() const;
};

#define INITIALIZE_COMMAND_ALLOCATOR_GROUP(device, group, type) \
    do { \
        CommandAllocatorGroup::Initialize(device, (group), type); \
        for (UINT n = 0; n < FRAME_COUNT; n++) \
        { \
            NAME_D3D12_OBJECT_INDEXED((group)->commandAllocators, n); \
        } \
        NAME_D3D12_OBJECT((group)->commandList); \
    } while (false)

inline DirectX::XMMATRIX XMMatrixToNormal(const DirectX::XMMATRIX& matrix)
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
