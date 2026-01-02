// <copyright file="Light.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#include "Spatial.hpp"

/**
 * \brief Represents the light of the space. It is an directional light.
 */
class Light final : public Spatial
{
    DECLARE_OBJECT_SUBCLASS(Light)

public:
    explicit Light(NativeClient& client);

    void                                   SetDirection(DirectX::XMFLOAT3 const& direction);
    [[nodiscard]] DirectX::XMFLOAT3 const& GetDirection() const;

    void                                   SetColor(DirectX::XMFLOAT3 const& color);
    [[nodiscard]] DirectX::XMFLOAT3 const& GetColor() const;

    void                SetIntensity(float intensity);
    [[nodiscard]] float GetIntensity() const;

private:
    DirectX::XMFLOAT3 m_direction = {0.0f, 0.0f, 0.0f};
    DirectX::XMFLOAT3 m_color     = {1.0f, 1.0f, 1.0f};
    float             m_intensity = 1.0f;
};
