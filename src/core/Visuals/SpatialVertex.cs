// <copyright file="SpatialVertex.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     The vertex layout used by all basic meshes.
///     A mesh is simply a sequence of quads with their vertices in CW order.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
#pragma warning disable S3898 // No equality comparison used.
public struct SpatialVertex
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     Additional data for the vertex. The complete shader data is split over the four vertices of a quad.
    /// </summary>
    public uint Data;
}
