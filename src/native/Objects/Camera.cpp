#include "stdafx.h"

Camera::Camera(NativeClient& client) : Object(client)
{
}

void Camera::Initialize()
{
    m_spaceCameraBufferSize = sizeof(CameraParametersBuffer);
    m_spaceCameraBuffer = util::AllocateConstantBuffer(GetClient(), &m_spaceCameraBufferSize);
    NAME_D3D12_OBJECT(m_spaceCameraBuffer);

    TRY_DO(m_spaceCameraBuffer.Map(&m_spaceCameraBufferMapping, 1));
}

void Camera::Update()
{
    const DirectX::XMVECTOR eye = XMLoadFloat3(&m_position);
    const DirectX::XMVECTOR forward = XMLoadFloat3(&m_front);
    const DirectX::XMVECTOR up = XMLoadFloat3(&m_up);
    
    const float fovAngleY = m_fov * DirectX::XM_PI / 180.0f;
    
    const auto view = DirectX::XMMatrixLookToRH(eye, forward, up);
    const auto projection = DirectX::XMMatrixPerspectiveFovRH(fovAngleY, GetClient().GetAspectRatio(), m_near, m_far);
    
    XMStoreFloat4x4(&m_vpMatrix, view * projection);
    
    DirectX::XMVECTOR det;
    const auto viewI = XMMatrixInverse(&det, view);
    const auto projectionI = XMMatrixInverse(&det, projection);

    CameraParametersBuffer data = {};
    XMStoreFloat4x4(&data.view, XMMatrixTranspose(view));
    XMStoreFloat4x4(&data.projection, XMMatrixTranspose(projection));
    XMStoreFloat4x4(&data.viewI, XMMatrixTranspose(viewI));
    XMStoreFloat4x4(&data.projectionI, XMMatrixTranspose(projectionI));

    data.dNear = m_near;
    data.dFar = m_far;

    // For cone tracing, a spread angle is used to get the width of the cone.
    // Here, an estimate is pre-calculated.
    data.spread = std::atan(2.0f * std::tan(fovAngleY / 2.0f) / GetSpace().GetResolution().height);

    m_spaceCameraBufferMapping.Write(data);
}

void Camera::SetPosition(const DirectX::XMFLOAT3& position)
{
    m_position = position;
}

void Camera::SetOrientation(const DirectX::XMFLOAT3& front, const DirectX::XMFLOAT3& up)
{
    m_front = front;
    m_up = up;
}

const DirectX::XMFLOAT3& Camera::GetPosition() const
{
    return m_position;
}

const DirectX::XMFLOAT4X4& Camera::GetViewProjectionMatrix() const
{
    return m_vpMatrix;
}

void Camera::SetFov(const float fov)
{
    m_fov = fov;
}

void Camera::SetPlanes(const float nearDistance, const float farDistance)
{
    REQUIRE(nearDistance > 0.0f);
    REQUIRE(farDistance > nearDistance);

    m_near = nearDistance;
    m_far = farDistance;
}

D3D12_GPU_VIRTUAL_ADDRESS Camera::GetCameraBufferAddress() const
{
    return m_spaceCameraBuffer.GetGPUVirtualAddress();
}

Space& Camera::GetSpace() const
{
    return *GetClient().GetSpace();
}
