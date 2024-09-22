#include "stdafx.h"

Camera::Camera(NativeClient& client)
    : Object(client)
{
}

void Camera::Initialize()
{
    m_spaceCameraBufferSize = sizeof(CameraParametersBuffer);
    m_spaceCameraBuffer     = util::AllocateConstantBuffer(GetClient(), &m_spaceCameraBufferSize);
    NAME_D3D12_OBJECT(m_spaceCameraBuffer);

    TryDo(m_spaceCameraBuffer.Map(&m_spaceCameraBufferMapping, 1));
}

void Camera::Update()
{
    DirectX::XMVECTOR const eye     = XMLoadFloat3(&m_position);
    DirectX::XMVECTOR const forward = XMLoadFloat3(&m_front);
    DirectX::XMVECTOR const up      = XMLoadFloat3(&m_up);

    float const fovAngleY = m_fov * DirectX::XM_PI / 180.0f;

    auto const view       = DirectX::XMMatrixLookToRH(eye, forward, up);
    auto const projection = DirectX::XMMatrixPerspectiveFovRH(fovAngleY, GetClient().GetAspectRatio(), m_near, m_far);

    XMStoreFloat4x4(&m_vpMatrix, view * projection);

    DirectX::XMVECTOR det;
    auto const        viewI       = XMMatrixInverse(&det, view);
    auto const        projectionI = XMMatrixInverse(&det, projection);

    CameraParametersBuffer data = {};
    XMStoreFloat4x4(&data.view, XMMatrixTranspose(view));
    XMStoreFloat4x4(&data.projection, XMMatrixTranspose(projection));
    XMStoreFloat4x4(&data.viewI, XMMatrixTranspose(viewI));
    XMStoreFloat4x4(&data.projectionI, XMMatrixTranspose(projectionI));

    data.dNear = m_near;
    data.dFar  = m_far;

    auto height = static_cast<float>(GetSpace().GetResolution().height);

    // For cone tracing, a spread angle is used to get the width of the cone.
    // Here, an estimate is pre-calculated.
    data.spread = std::atan(2.0f * std::tan(fovAngleY / 2.0f) / height);

    m_spaceCameraBufferMapping.Write(data);
}

void Camera::SetPosition(DirectX::XMFLOAT3 const& position) { m_position = position; }

void Camera::SetOrientation(DirectX::XMFLOAT3 const& front, DirectX::XMFLOAT3 const& up)
{
    m_front = front;
    m_up    = up;
}

DirectX::XMFLOAT3 const& Camera::GetPosition() const { return m_position; }

DirectX::XMFLOAT4X4 const& Camera::GetViewProjectionMatrix() const { return m_vpMatrix; }

void Camera::SetFov(float const fov) { m_fov = fov; }

void Camera::SetPlanes(float const nearDistance, float const farDistance)
{
    Require(nearDistance > 0.0f);
    Require(farDistance > nearDistance);

    m_near = nearDistance;
    m_far  = farDistance;
}

D3D12_GPU_VIRTUAL_ADDRESS Camera::GetCameraBufferAddress() const { return m_spaceCameraBuffer.GetGPUVirtualAddress(); }

Space& Camera::GetSpace() const { return *GetClient().GetSpace(); }
