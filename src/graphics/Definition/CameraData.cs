// <copyright file="CameraData.cs" company="VoxelGame">
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
///     Data of a camera that is often updated.
/// </summary>
/// <param name="Position">The position.</param>
/// <param name="Front">The front vector.</param>
/// <param name="Up">The up vector.</param>
[NativeMarshalling(typeof(BasicCameraDataMarshaller))]
public record struct BasicCameraData(Vector3 Position, Vector3 Front, Vector3 Up);

[CustomMarshaller(typeof(BasicCameraData), MarshalMode.ManagedToUnmanagedIn, typeof(BasicCameraDataMarshaller))]
internal static class BasicCameraDataMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(BasicCameraData managed)
    {
        return new Unmanaged
        {
            position = Vector3Marshaller.ConvertToUnmanaged(managed.Position),
            front = Vector3Marshaller.ConvertToUnmanaged(managed.Front),
            up = Vector3Marshaller.ConvertToUnmanaged(managed.Up)
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        Vector3Marshaller.Free(unmanaged.position);
        Vector3Marshaller.Free(unmanaged.front);
        Vector3Marshaller.Free(unmanaged.up);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal ref struct Unmanaged
    {
        internal Vector3Marshaller.Unmanaged position;
        internal Vector3Marshaller.Unmanaged front;
        internal Vector3Marshaller.Unmanaged up;
    }
}

/// <summary>
///     Data of a camera that is rarely updated.
/// </summary>
/// <param name="Fov">The field of view.</param>
/// <param name="Near">The distance to the near plane.</param>
/// <param name="Far">The distance to the far plane.</param>
[NativeMarshalling(typeof(AdvancedCameraDataMarshaller))]
public record struct AdvancedCameraData(Single Fov, Single Near, Single Far);

[CustomMarshaller(typeof(AdvancedCameraData), MarshalMode.ManagedToUnmanagedIn, typeof(AdvancedCameraDataMarshaller))]
internal static class AdvancedCameraDataMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(AdvancedCameraData managed)
    {
        return new Unmanaged
        {
            fov = managed.Fov,
            near = managed.Near,
            far = managed.Far
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal ref struct Unmanaged
    {
        internal Single fov;
        internal Single near;
        internal Single far;
    }
}
