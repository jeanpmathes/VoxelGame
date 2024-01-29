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

#pragma pack(push, 4)
struct CameraParametersBuffer
{
    DirectX::XMFLOAT4X4 view;
    DirectX::XMFLOAT4X4 projection;
    DirectX::XMFLOAT4X4 viewI;
    DirectX::XMFLOAT4X4 projectionI;

    FLOAT dNear;
    FLOAT dFar;
};
#pragma pack(pop)

/**
 * \brief Represents the camera of the space.
 */
class Camera final : public Object
{
    DECLARE_OBJECT_SUBCLASS(Camera)

public:
    /**
     * \brief Creates a new camera.
     * \param client The client.
     */
    explicit Camera(NativeClient& client);
    
    void Initialize();
    void Update();

    void SetPosition(const DirectX::XMFLOAT3& position);
    void SetOrientation(const DirectX::XMFLOAT3& front, const DirectX::XMFLOAT3& up);

    [[nodiscard]] const DirectX::XMFLOAT3& GetPosition() const;
    [[nodiscard]] const DirectX::XMFLOAT4X4& GetViewProjectionMatrix() const;

    void SetFov(float fov);
    void SetPlanes(float nearDistance, float farDistance);

    /**
     * \brief Get the GPU address of the camera parameter buffer. The buffer contains a CameraDataBuffer struct.
     * \return The GPU address. Will be valid for the entire lifetime of the camera, assuming it is initialized.
     */
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetCameraBufferAddress() const;

private:
    DirectX::XMFLOAT3 m_position = {};
    DirectX::XMFLOAT3 m_front = {};
    DirectX::XMFLOAT3 m_up = {};

    float m_fov = 0.0f;
    float m_near = 0.0f;
    float m_far = 0.0f;

    DirectX::XMFLOAT4X4 m_vpMatrix = {};

    Allocation<ID3D12Resource> m_spaceCameraBuffer = {};
    Mapping<ID3D12Resource, CameraParametersBuffer> m_spaceCameraBufferMapping = {};
    UINT64 m_spaceCameraBufferSize = 0;
};
