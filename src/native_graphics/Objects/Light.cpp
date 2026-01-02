#include "stdafx.h"

#include "Light.hpp"

Light::Light(NativeClient& client)
    : Spatial(client)
{
}

void Light::SetDirection(DirectX::XMFLOAT3 const& direction) { m_direction = direction; }

DirectX::XMFLOAT3 const& Light::GetDirection() const { return m_direction; }

void Light::SetColor(DirectX::XMFLOAT3 const& color) { m_color = color; }

DirectX::XMFLOAT3 const& Light::GetColor() const { return m_color; }

void Light::SetIntensity(float const intensity) { m_intensity = intensity; }

float Light::GetIntensity() const { return m_intensity; }
