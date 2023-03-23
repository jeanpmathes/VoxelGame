#include "stdafx.h"

Camera::Camera(NativeClient& client) : Object(client)
{
}

void Camera::Initialize()
{
    constexpr uint32_t matrixCount = 4;
    m_spaceCameraBufferSize = matrixCount * sizeof(DirectX::XMMATRIX);
    m_spaceCameraBuffer = nv_helpers_dx12::CreateBuffer(GetClient().GetDevice().Get(), m_spaceCameraBufferSize,
                                                        D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
                                                        nv_helpers_dx12::kUploadHeapProps);
    m_spaceConstHeap = nv_helpers_dx12::CreateDescriptorHeap(GetClient().GetDevice().Get(), 1,
                                                             D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, true);

    D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc;
    cbvDesc.BufferLocation = m_spaceCameraBuffer->GetGPUVirtualAddress();
    cbvDesc.SizeInBytes = m_spaceCameraBufferSize;

    const D3D12_CPU_DESCRIPTOR_HANDLE srvHandle = m_spaceConstHeap->GetCPUDescriptorHandleForHeapStart();
    GetClient().GetDevice()->CreateConstantBufferView(&cbvDesc, srvHandle);
}

void Camera::Update() const
{
    const DirectX::XMVECTOR eye = DirectX::XMVectorSet(m_position.x, m_position.y, m_position.z, 0.0f);
    const DirectX::XMVECTOR forward = DirectX::XMVectorSet(1.0f, -1.0f, 1.0f, 0.0f);
    const DirectX::XMVECTOR up = DirectX::XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);

    const auto view = DirectX::XMMatrixLookAtRH(eye, DirectX::XMVectorAdd(eye, forward), up);
    constexpr float fovAngleY = 70.0f * DirectX::XM_PI / 180.0f;
    const auto projection = DirectX::XMMatrixPerspectiveFovRH(fovAngleY, GetClient().GetAspectRatio(), 0.1f, 1000.0f);

    DirectX::XMVECTOR det;
    const auto viewI = XMMatrixInverse(&det, view);
    const auto projectionI = XMMatrixInverse(&det, projection);

    std::vector<DirectX::XMFLOAT4X4> matrices(4);
    XMStoreFloat4x4(&matrices[0], view);
    XMStoreFloat4x4(&matrices[1], projection);
    XMStoreFloat4x4(&matrices[2], viewI);
    XMStoreFloat4x4(&matrices[3], projectionI);

    uint8_t* pData;
    TRY_DO(m_spaceCameraBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));
    memcpy(pData, matrices.data(), m_spaceCameraBufferSize);
    m_spaceCameraBuffer->Unmap(0, nullptr);
}

void Camera::SetPosition(const DirectX::XMFLOAT3& position)
{
    m_position = position;
}

void Camera::SetBufferViewDescription(D3D12_CONSTANT_BUFFER_VIEW_DESC* cbvDesc) const
{
    assert(cbvDesc);

    cbvDesc->BufferLocation = m_spaceCameraBuffer->GetGPUVirtualAddress();
    cbvDesc->SizeInBytes = m_spaceCameraBufferSize;
}
