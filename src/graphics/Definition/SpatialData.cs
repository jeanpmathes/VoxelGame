// <copyright file="SpatialData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Interop;

namespace VoxelGame.Graphics.Definition;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Data of a spatial object that is often updated.
/// </summary>
/// <param name="Position">The position of the spatial object.</param>
/// <param name="Rotation">The rotation of the spatial object, as a quaternion.</param>
[NativeMarshalling(typeof(SpatialDataMarshaller))]
public record struct SpatialData(Vector3 Position, Vector4 Rotation);

[CustomMarshaller(typeof(SpatialData), MarshalMode.ManagedToUnmanagedIn, typeof(SpatialDataMarshaller))]
internal static class SpatialDataMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(SpatialData managed)
    {
        return new Unmanaged
        {
            position = Vector3Marshaller.ConvertToUnmanaged(managed.Position),
            rotation = Vector4Marshaller.ConvertToUnmanaged(managed.Rotation)
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        Vector3Marshaller.Free(unmanaged.position);
        Vector4Marshaller.Free(unmanaged.rotation);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal ref struct Unmanaged
    {
        internal Vector3Marshaller.Unmanaged position;
        internal Vector4Marshaller.Unmanaged rotation;
    }
}
