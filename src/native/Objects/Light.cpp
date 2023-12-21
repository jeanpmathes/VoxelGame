#include "stdafx.h"

#include "Light.hpp"

Light::Light(NativeClient& client) : Spatial(client)
{
}

void Light::SetDirection(const DirectX::XMFLOAT3& direction)
{
    m_direction = direction;
}

const DirectX::XMFLOAT3& Light::GetDirection() const
{
    return m_direction;
}
