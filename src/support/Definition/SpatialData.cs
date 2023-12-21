// <copyright file="SpatialData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Data of a spatial object that is often updated.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
public struct SpatialData
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     The position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     The rotation, as a quaternion.
    /// </summary>
    public Vector4 Rotation;
}
