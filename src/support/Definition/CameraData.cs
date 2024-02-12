// <copyright file="CameraData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Data of a camera that is often updated.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
public struct BasicCameraData
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     The position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     The front vector.
    /// </summary>
    public Vector3 Front;

    /// <summary>
    ///     The up vector.
    /// </summary>
    public Vector3 Up;
}

/// <summary>
///     Data of a camera that is rarely updated.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
public struct AdvancedCameraData
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     The field of view.
    /// </summary>
    public float Fov;

    /// <summary>
    ///     The distance to the near plane.
    /// </summary>
    public float Near;

    /// <summary>
    ///     The distance to the far plane.
    /// </summary>
    public float Far;
}
