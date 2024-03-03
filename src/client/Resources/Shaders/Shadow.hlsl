//  <copyright file="Shadow.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "PayloadRT.hlsl"

[shader("miss")]void ShadowMiss(inout native::rt::ShadowHitInfo hitInfo) { hitInfo.isHit = false; }
