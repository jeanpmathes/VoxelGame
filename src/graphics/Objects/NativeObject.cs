// <copyright file="NativeObject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     Base class for all native objects, which are objects that are created by the native API and used over a pointer.
///     The lifetime of the native object is bound to the native client.
/// </summary>
[NativeMarshalling(typeof(NativeObjectMarshaller))]
public class NativeObject
{
    private readonly Synchronizer.Handle handle;

    /// <summary>
    ///     Creates a new instance of the <see cref="NativeObject" /> class.
    /// </summary>
    /// <param name="nativePointer">The native pointer for this object.</param>
    /// <param name="client">The native client.</param>
    protected NativeObject(IntPtr nativePointer, Client client)
    {
        Self = nativePointer;
        Client = client;

        handle = client.Sync.RegisterObject(this);
    }

    /// <summary>
    ///     The native pointer.
    /// </summary>
    public IntPtr Self { get; }

    /// <summary>
    ///     The native client.
    /// </summary>
    protected Client Client { get; }

    /// <summary>
    ///     De-registers the object from the client.
    /// </summary>
    protected void Deregister()
    {
        Client.Sync.DeRegisterObject(handle);
    }

    /// <summary>
    ///     Synchronizes the native object with the managed object.
    /// </summary>
    internal virtual void Synchronize()
    {
        Client.Sync.DisableSync(handle);
    }

    /// <summary>
    ///     Called before the native object is synchronized.
    ///     Use this for effects that should be considered by other native objects during their synchronization.
    /// </summary>
    internal virtual void PrepareSynchronization()
    {
        Client.Sync.DisablePreSync(handle);
    }
}

[CustomMarshaller(typeof(NativeObject), MarshalMode.ManagedToUnmanagedIn, typeof(NativeObjectMarshaller))]
internal static class NativeObjectMarshaller
{
    internal static IntPtr ConvertToUnmanaged(NativeObject managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }

#pragma warning disable S1694
    internal abstract class Marshaller : IMarshaller<NativeObject, IntPtr>
#pragma warning restore S1694
    {
        static IntPtr IMarshaller<NativeObject, IntPtr>.ConvertToUnmanaged(NativeObject managed)
        {
            return ConvertToUnmanaged(managed);
        }

        static void IMarshaller<NativeObject, IntPtr>.Free(IntPtr unmanaged)
        {
            Free(unmanaged);
        }
    }
}
