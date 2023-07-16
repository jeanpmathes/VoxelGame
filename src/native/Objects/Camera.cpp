#include "stdafx.h"

Camera::Camera(NativeClient& client) : Object(client)
{
}

void Camera::Initialize()
{
    constexpr uint32_t matrixCount = 4;
    m_spaceCameraBufferSize = matrixCount * sizeof(DirectX::XMMATRIX);
    m_spaceCameraBuffer = util::AllocateBuffer(GetClient(), m_spaceCameraBufferSize,
                                               D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
                                               D3D12_HEAP_TYPE_UPLOAD);
    m_spaceConstHeap = CreateDescriptorHeap(GetClient().GetDevice().Get(), 1,
                                            D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true);

    NAME_D3D12_OBJECT(m_spaceCameraBuffer);
    NAME_D3D12_OBJECT(m_spaceConstHeap);

    D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc;
    cbvDesc.BufferLocation = m_spaceCameraBuffer.resource->GetGPUVirtualAddress();
    cbvDesc.SizeInBytes = m_spaceCameraBufferSize;

    const D3D12_CPU_DESCRIPTOR_HANDLE srvHandle = m_spaceConstHeap->GetCPUDescriptorHandleForHeapStart();
    GetClient().GetDevice()->CreateConstantBufferView(&cbvDesc, srvHandle);
}

void Camera::Update() const
{
    const DirectX::XMVECTOR eye = DirectX::XMVectorSet(m_position.x, m_position.y, m_position.z, 0.0f);
    const DirectX::XMVECTOR forward = DirectX::XMVectorSet(m_front.x, m_front.y, m_front.z, 0.0f);
    const DirectX::XMVECTOR up = DirectX::XMVectorSet(m_up.x, m_up.y, m_up.z, 0.0f);

    const auto view = DirectX::XMMatrixLookAtRH(eye, DirectX::XMVectorAdd(eye, forward), up);
    const float fovAngleY = m_fov * DirectX::XM_PI / 180.0f;
    const auto projection = DirectX::XMMatrixPerspectiveFovRH(fovAngleY, GetClient().GetAspectRatio(), m_near, m_far);

    DirectX::XMVECTOR det;
    const auto viewI = XMMatrixInverse(&det, view);
    const auto projectionI = XMMatrixInverse(&det, projection);

    std::vector<DirectX::XMFLOAT4X4> matrices(4);
    XMStoreFloat4x4(&matrices[0], view);
    XMStoreFloat4x4(&matrices[1], projection);
    XMStoreFloat4x4(&matrices[2], viewI);
    XMStoreFloat4x4(&matrices[3], projectionI);

    TRY_DO(util::MapAndWrite(m_spaceCameraBuffer, matrices.data(), static_cast<UINT>(matrices.size())));
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


void Camera::SetBufferViewDescription(D3D12_CONSTANT_BUFFER_VIEW_DESC* cbvDesc) const
{
    REQUIRE(cbvDesc);

    cbvDesc->BufferLocation = m_spaceCameraBuffer.resource->GetGPUVirtualAddress();
    cbvDesc->SizeInBytes = m_spaceCameraBufferSize;
}
