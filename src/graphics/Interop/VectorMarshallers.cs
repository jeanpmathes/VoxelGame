// <copyright file="VectorMarshallers.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Interop;

/// <summary>
///     Marshaller for <see cref="Vector3" />.
/// </summary>
[CustomMarshaller(typeof(Vector3), MarshalMode.ManagedToUnmanagedIn, typeof(Vector3Marshaller))]
public static class Vector3Marshaller
{
    /// <summary>
    ///     Convert a managed <see cref="Vector3" /> to an unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static Unmanaged ConvertToUnmanaged(Vector3 managed)
    {
        return new Unmanaged
        {
            x = managed.X,
            y = managed.Y,
            z = managed.Z
        };
    }

    /// <summary>
    ///     Free the unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    /// <summary>
    ///     The unmanaged representation of <see cref="Vector3" />.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public ref struct Unmanaged
    {
        internal Single x;
        internal Single y;
        internal Single z;
    }
}

/// <summary>
///     Marshaller for <see cref="Vector4" />.
/// </summary>
[CustomMarshaller(typeof(Vector4), MarshalMode.ManagedToUnmanagedIn, typeof(Vector4Marshaller))]
public static class Vector4Marshaller
{
    /// <summary>
    ///     Convert a managed <see cref="Vector4" /> to an unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static Unmanaged ConvertToUnmanaged(Vector4 managed)
    {
        return new Unmanaged
        {
            x = managed.X,
            y = managed.Y,
            z = managed.Z,
            w = managed.W
        };
    }

    /// <summary>
    ///     Free the unmanaged <see cref="Unmanaged" />.
    /// </summary>
    public static void Free(Unmanaged unmanaged)
    {
        // Nothing to do here.
    }

    /// <summary>
    ///     The unmanaged representation of <see cref="Vector4" />.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public ref struct Unmanaged
    {
        internal Single x;
        internal Single y;
        internal Single z;
        internal Single w;
    }
}
