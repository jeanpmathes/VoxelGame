#include "stdafx.h"

Spatial::Spatial(NativeClient& client)
    : Object(client)
{
    XMStoreFloat3(&position, DirectX::XMVectorZero());
    XMStoreFloat4(&rotation, DirectX::XMQuaternionIdentity());
    RecalculateTransform();
}

bool Spatial::ClearTransformDirty()
{
    bool const wasDirty = transformDirty;
    transformDirty      = false;
    return wasDirty;
}

void Spatial::SetPosition(DirectX::XMFLOAT3 const& newPosition)
{
    position = newPosition;
    RecalculateTransform();
}

DirectX::XMFLOAT3 const& Spatial::GetPosition() const { return position; }

void Spatial::SetRotation(DirectX::XMFLOAT4 const& newRotation)
{
    rotation = newRotation;
    RecalculateTransform();
}

DirectX::XMFLOAT4 const& Spatial::GetRotation() const { return rotation; }

DirectX::XMFLOAT4X4 const& Spatial::GetTransform() const { return transform; }

void Spatial::RecalculateTransform()
{
    DirectX::XMVECTOR const p = XMLoadFloat3(&position);
    DirectX::XMVECTOR const r = XMLoadFloat4(&rotation);
    DirectX::XMVECTOR const s = DirectX::XMVectorSet(1.0f, 1.0f, 1.0f, 0.0f);

    DirectX::XMMATRIX const t = DirectX::XMMatrixAffineTransformation(s, DirectX::XMVectorZero(), r, p);

    XMStoreFloat4x4(&transform, t);
    transformDirty = true;
}
