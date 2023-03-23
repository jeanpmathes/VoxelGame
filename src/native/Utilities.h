//  <copyright file="Utilities.h" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#pragma once

inline DirectX::XMMATRIX XMMatrixToNormal(const DirectX::XMMATRIX& matrix)
{
    DirectX::XMMATRIX upper = matrix;

    upper.r[0].m128_f32[3] = 0.f;
    upper.r[1].m128_f32[3] = 0.f;
    upper.r[2].m128_f32[3] = 0.f;
    upper.r[3].m128_f32[0] = 0.f;
    upper.r[3].m128_f32[1] = 0.f;
    upper.r[3].m128_f32[2] = 0.f;
    upper.r[3].m128_f32[3] = 1.f;

    DirectX::XMVECTOR det;
    return XMMatrixTranspose(XMMatrixInverse(&det, upper));
}
