// <copyright file="Camera.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Object.h"

class NativeClient;

/**
 * \brief Represents the camera of the space.
 */
class Camera final : public Object
{
public:
    ~Camera() override = default;
    explicit Camera(NativeClient& client);

    void Initialize();
    void Update() const;

    void SetPosition(const DirectX::XMFLOAT3& position);

    void SetBufferViewDescription(D3D12_CONSTANT_BUFFER_VIEW_DESC* cbvDesc) const;

private:
    DirectX::XMFLOAT3 m_position = {};

    ComPtr<ID3D12Resource> m_spaceCameraBuffer = nullptr;
    ComPtr<ID3D12DescriptorHeap> m_spaceConstHeap = nullptr;
    uint32_t m_spaceCameraBufferSize = 0;
};
