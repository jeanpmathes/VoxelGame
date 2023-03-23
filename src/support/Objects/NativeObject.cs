// <copyright file="NativeObject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Objects;

/// <summary>
///     Base class for all native objects, which are objects that are created by the native API and used over a pointer.
///     The lifetime of the native object is bound to the native client.
/// </summary>
public abstract class NativeObject
{
    /// <summary>
    ///     Creates a new instance of the <see cref="NativeObject" /> class.
    /// </summary>
    /// <param name="nativePointer">The native pointer for this object.</param>
    /// <param name="client">The native client.</param>
    protected NativeObject(IntPtr nativePointer, Client client)
    {
        Self = nativePointer;
        Client = client;
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
    ///     Synchronizes the native object with the managed object.
    /// </summary>
    public abstract void Synchronize();

    /// <summary>
    ///     Called before the native object is synchronized.
    /// </summary>
    public virtual void PrepareSynchronization() {}
}

