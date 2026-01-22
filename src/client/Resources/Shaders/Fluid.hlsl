// <copyright file="Fluid.hlsl" company="VoxelGame">
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

#include "Payload.hlsl"
#include "Section.hlsl"

[shader("closesthit")]void FluidSectionClosestHit(inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    float const path = vg::ray::GetPathLength(payload);

    vg::spatial::Info const info  = vg::spatial::GetCurrentInfo(attributes);
    float4 const            tint  = vg::decode::GetTintColor(info.data);
    float4 const            color = vg::section::GetFluidBaseColor(path, info) * tint;

    float4 const dominant = vg::section::GetFluidDominantColor(info);
    vg::ray::SetFogColor(payload, dominant.rgb * tint.rgb);

    SET_FINAL_HIT_INFO(payload, info, float4(vg::spatial::CalculateShading(info, color.rgb), color.a));
}

[shader("closesthit")]void FluidShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes) { hitInfo.isHit = true; }
