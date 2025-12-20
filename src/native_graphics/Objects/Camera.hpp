// <copyright file="Camera.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

    FLOAT spread;
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

    void SetPosition(DirectX::XMFLOAT3 const& position);
    void SetOrientation(DirectX::XMFLOAT3 const& front, DirectX::XMFLOAT3 const& up);

    [[nodiscard]] DirectX::XMFLOAT3 const&   GetPosition() const;
    [[nodiscard]] DirectX::XMFLOAT4X4 const& GetViewMatrix() const;
    [[nodiscard]] DirectX::XMFLOAT4X4 const& GetProjectionMatrix() const;
    [[nodiscard]] DirectX::XMFLOAT4X4 const& GetViewProjectionMatrix() const;

    [[nodiscard]] float GetNearPlane() const;
    [[nodiscard]] float GetFarPlane() const;

    void SetFov(float fov);
    void SetPlanes(float nearDistance, float farDistance);

    /**
     * \brief Get the GPU address of the camera parameter buffer. The buffer contains a CameraDataBuffer struct.
     * \return The GPU address. Will be valid for the entire lifetime of the camera, assuming it is initialized.
     */
    [[nodiscard]] D3D12_GPU_VIRTUAL_ADDRESS GetCameraBufferAddress() const;

    [[nodiscard]] Space& GetSpace() const;

private:
    DirectX::XMFLOAT3 m_position = {};
    DirectX::XMFLOAT3 m_front    = {};
    DirectX::XMFLOAT3 m_up       = {};

    float m_fov  = 0.0f;
    float m_near = 0.0f;
    float m_far  = 0.0f;

    DirectX::XMFLOAT4X4 m_vMatrix  = {};
    DirectX::XMFLOAT4X4 m_pMatrix  = {};
    DirectX::XMFLOAT4X4 m_vpMatrix = {};

    Allocation<ID3D12Resource>                      m_spaceCameraBuffer        = {};
    Mapping<ID3D12Resource, CameraParametersBuffer> m_spaceCameraBufferMapping = {};
    UINT64                                          m_spaceCameraBufferSize    = 0;
};
