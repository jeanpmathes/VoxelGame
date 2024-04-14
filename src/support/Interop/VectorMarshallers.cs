// <copyright file="VectorMarshallers.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Support.Interop;

[CustomMarshaller(typeof(Vector3), MarshalMode.ManagedToUnmanagedIn, typeof(Vector3Marshaller))]
internal static class Vector3Marshaller
{
    internal static Unmanaged ConvertToUnmanaged(Vector3 managed)
    {
        return new Unmanaged
        {
            x = managed.X,
            y = managed.Y,
            z = managed.Z
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal ref struct Unmanaged
    {
        internal Single x;
        internal Single y;
        internal Single z;
    }
}

[CustomMarshaller(typeof(Vector4), MarshalMode.ManagedToUnmanagedIn, typeof(Vector4Marshaller))]
internal static class Vector4Marshaller
{
    internal static Unmanaged ConvertToUnmanaged(Vector4 managed)
    {
        return new Unmanaged
        {
            x = managed.X,
            y = managed.Y,
            z = managed.Z,
            w = managed.W
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal ref struct Unmanaged
    {
        internal Single x;
        internal Single y;
        internal Single z;
        internal Single w;
    }
}
