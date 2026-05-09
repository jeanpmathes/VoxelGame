#include "stdafx.h"

#include "Light.hpp"

Light::Light(NativeClient& client)
    : Spatial(client)
{
}

void Light::SetDirection(DirectX::XMFLOAT3 const& newDirection) { direction = newDirection; }

DirectX::XMFLOAT3 const& Light::GetDirection() const { return direction; }

void Light::SetColor(DirectX::XMFLOAT3 const& newColor) { color = newColor; }

DirectX::XMFLOAT3 const& Light::GetColor() const { return color; }

void Light::SetIntensity(float const newIntensity) { intensity = newIntensity; }

float Light::GetIntensity() const { return intensity; }
