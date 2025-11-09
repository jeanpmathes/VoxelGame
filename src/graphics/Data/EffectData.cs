// <copyright file="EffectData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Data;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Vertex type used by the <see cref="Effect" /> class.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EffectVertex
{
    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     Additional data for the vertex, for any purpose determined by the shader.
    /// </summary>
    public UInt32 Data;
}
