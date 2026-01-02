// <copyright file="Spatial.hpp" company="VoxelGame">
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

#include "Object.hpp"

class NativeClient;

struct SpatialData
{
    DirectX::XMFLOAT3 position;
    DirectX::XMFLOAT4 rotation;
};

/**
 * \brief The base class of all objects in the space that can be observed. This explicitly excludes the camera.
 */
class Spatial : public Object
{
    DECLARE_OBJECT_SUBCLASS(Spatial)

protected:
    explicit Spatial(NativeClient& client);

    [[nodiscard]] bool ClearTransformDirty();

public:
    void                                   SetPosition(DirectX::XMFLOAT3 const& position);
    [[nodiscard]] DirectX::XMFLOAT3 const& GetPosition() const;

    void                                   SetRotation(DirectX::XMFLOAT4 const& rotation);
    [[nodiscard]] DirectX::XMFLOAT4 const& GetRotation() const;

    [[nodiscard]] DirectX::XMFLOAT4X4 const& GetTransform() const;

private:
    void RecalculateTransform();

    DirectX::XMFLOAT3   m_position{};
    DirectX::XMFLOAT4   m_rotation{};
    DirectX::XMFLOAT4X4 m_transform{};

    bool m_transformDirty = true;
};
