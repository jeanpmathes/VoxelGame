#include "stdafx.h"

Camera::Camera(NativeClient& client)
    : Object(client)
{
}

void Camera::Initialize()
{
    spaceCameraBufferSize = sizeof(CameraParametersBuffer);
    spaceCameraBuffer     = util::AllocateConstantBuffer(GetClient(), &spaceCameraBufferSize);
    NAME_D3D12_OBJECT(spaceCameraBuffer);

    TryDo(spaceCameraBuffer.Map(&spaceCameraBufferMapping, 1));
}

void Camera::Update()
{
    DirectX::XMVECTOR const eye     = XMLoadFloat3(&position);
    DirectX::XMVECTOR const forward = XMLoadFloat3(&front);
    DirectX::XMVECTOR const up      = XMLoadFloat3(&top);

    float const fovAngleY = fov * DirectX::XM_PI / 180.0f;

    auto const view       = DirectX::XMMatrixLookToRH(eye, forward, up);
    auto const projection = DirectX::XMMatrixPerspectiveFovRH(fovAngleY, GetClient().GetAspectRatio(), nearZ, farZ);

    XMStoreFloat4x4(&vMatrix, view);
    XMStoreFloat4x4(&pMatrix, projection);
    XMStoreFloat4x4(&vpMatrix, view * projection);

    DirectX::XMVECTOR det;
    auto const        viewI       = XMMatrixInverse(&det, view);
    auto const        projectionI = XMMatrixInverse(&det, projection);

    CameraParametersBuffer data = {};
    XMStoreFloat4x4(&data.view, XMMatrixTranspose(view));
    XMStoreFloat4x4(&data.projection, XMMatrixTranspose(projection));
    XMStoreFloat4x4(&data.viewI, XMMatrixTranspose(viewI));
    XMStoreFloat4x4(&data.projectionI, XMMatrixTranspose(projectionI));

    data.dNear = nearZ;
    data.dFar  = farZ;

    auto height = static_cast<float>(GetSpace().GetResolution().height);

    // For cone tracing, a spread angle is used to get the width of the cone.
    // Here, an estimate is pre-calculated.
    data.spread = std::atan(2.0f * std::tan(fovAngleY / 2.0f) / height);

    spaceCameraBufferMapping.Write(data);
}

void Camera::SetPosition(DirectX::XMFLOAT3 const& newPosition) { position = newPosition; }

void Camera::SetOrientation(DirectX::XMFLOAT3 const& newFront, DirectX::XMFLOAT3 const& newTop)
{
    front = newFront;
    top   = newTop;
}

DirectX::XMFLOAT3 const& Camera::GetPosition() const { return position; }

DirectX::XMFLOAT4X4 const& Camera::GetViewMatrix() const { return vMatrix; }

DirectX::XMFLOAT4X4 const& Camera::GetProjectionMatrix() const { return pMatrix; }

DirectX::XMFLOAT4X4 const& Camera::GetViewProjectionMatrix() const { return vpMatrix; }

float Camera::GetNearPlane() const { return nearZ; }

float Camera::GetFarPlane() const { return farZ; }

void Camera::SetFov(float const newFov) { fov = newFov; }

void Camera::SetPlanes(float const nearDistance, float const farDistance)
{
    Require(nearDistance > 0.0f);
    Require(farDistance > nearDistance);

    nearZ = nearDistance;
    farZ  = farDistance;
}

D3D12_GPU_VIRTUAL_ADDRESS Camera::GetCameraBufferAddress() const { return spaceCameraBuffer.GetGPUVirtualAddress(); }

Space& Camera::GetSpace() const { return *GetClient().GetSpace(); }
