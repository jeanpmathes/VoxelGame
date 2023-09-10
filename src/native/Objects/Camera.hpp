// <copyright file="Camera.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Object.hpp"

class NativeClient;

struct BasicCameraData
{
    DirectX::XMFLOAT3 position;
    DirectX::XMFLOAT3 front;
    DirectX::XMFLOAT3 up;
};

struct AdvancedCameraData
{
    FLOAT fov;
    FLOAT nearDistance;
    FLOAT farDistance;
};

/**
 * \brief Represents the camera of the space.
 */
class Camera final : public Object
{
    DECLARE_OBJECT_SUBCLASS(Camera)

public:
    explicit Camera(NativeClient& client);

    void Initialize();
    void Update() const;

    void SetPosition(const DirectX::XMFLOAT3& position);
    void SetOrientation(const DirectX::XMFLOAT3& front, const DirectX::XMFLOAT3& up);

    void SetFov(float fov);
    void SetPlanes(float nearDistance, float farDistance);

    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetCameraBufferAddress() const;

private:
    DirectX::XMFLOAT3 m_position = {};
    DirectX::XMFLOAT3 m_front = {};
    DirectX::XMFLOAT3 m_up = {};

    float m_fov = 0.0f;
    float m_near = 0.0f;
    float m_far = 0.0f;

    Allocation<ID3D12Resource> m_spaceCameraBuffer = {};
    UINT64 m_spaceCameraBufferSize = 0;
};
