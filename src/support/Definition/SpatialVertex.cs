// <copyright file="SpatialVertex.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelGame.Support.Definition;

/// <summary>
///     The vertex layout used by all meshes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
public struct SpatialVertex
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     The color of the vertex.
    /// </summary>
    public Vector4 Color;
}

