#include "stdafx.h"

#include "Light.hpp"

Light::Light(NativeClient& client)
    : Spatial(client)
{
}

void Light::SetDirection(DirectX::XMFLOAT3 const& direction) { m_direction = direction; }

DirectX::XMFLOAT3 const& Light::GetDirection() const { return m_direction; }
