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
public struct CameraData
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     The position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     Creates a new instance of <see cref="CameraData" />.
    /// </summary>
    public CameraData(Vector3 position)
    {
        Position = position;
    }
}

