// <copyright file="Synchronizer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Core;

/// <summary>
///     Holds and synchronizes all <see cref="NativeObject" />s.
/// </summary>
public class Synchronizer
{
    private const Int32 DoNotCall = -1;
    private readonly Bag<Entry?> objects = new(gapValue: null);

    private readonly Bag<NativeObject?> preSyncBag = new(gapValue: null);
    private readonly Bag<NativeObject?> syncBag = new(gapValue: null);

    /// <summary>
    ///     Get all registered objects.
    /// </summary>
    public IEnumerable<NativeObject?> Objects
    {
        get
        {
            List<NativeObject?> result = new();

            foreach (Entry? nativeObject in objects.AsSpan())
                if (nativeObject != null)
                    result.Add(nativeObject.NativeObject);

            return result;
        }
    }

    /// <summary>
    ///     Update the synchronizer and synchronize all objects.
    /// </summary>
    public void Update()
    {
        foreach (NativeObject? nativeObject in preSyncBag.AsSpan()) nativeObject?.PrepareSynchronization();
        foreach (NativeObject? nativeObject in syncBag.AsSpan()) nativeObject?.Synchronize();
    }

    /// <summary>
    ///     Register a new native object.
    /// </summary>
    internal Handle RegisterObject(NativeObject nativeObject)
    {
        Throw.IfNotOnMainThread(objects);

        Int32 preSyncIndex = preSyncBag.Add(nativeObject);
        Int32 syncIndex = syncBag.Add(nativeObject);

        return new Handle(objects.Add(new Entry(nativeObject, preSyncIndex, syncIndex)));
    }

    /// <summary>
    ///     Disable the call of <see cref="NativeObject.PrepareSynchronization" /> for the given handle.
    /// </summary>
    internal void DisablePreSync(Handle handle)
    {
        Throw.IfNotOnMainThread(objects);

        Entry? entry = objects[handle.Index];
        Debug.Assert(entry != null);

        if (entry.PreSyncIndex == DoNotCall) return;

        preSyncBag.RemoveAt(entry.PreSyncIndex);
        objects[handle.Index] = entry with {PreSyncIndex = DoNotCall};
    }

    /// <summary>
    ///     Disable the call of <see cref="NativeObject.Synchronize" /> for the given handle.
    /// </summary>
    internal void DisableSync(Handle handle)
    {
        Throw.IfNotOnMainThread(objects);

        Entry? entry = objects[handle.Index];
        Debug.Assert(entry != null);

        if (entry.SyncIndex == DoNotCall) return;

        syncBag.RemoveAt(entry.SyncIndex);
        objects[handle.Index] = entry with {SyncIndex = DoNotCall};
    }

    /// <summary>
    ///     De-register a native object.
    /// </summary>
    internal void DeRegisterObject(Handle handle)
    {
        Throw.IfNotOnMainThread(objects);

        Entry? entry = objects[handle.Index];
        Debug.Assert(entry != null);

        if (entry.PreSyncIndex != DoNotCall) preSyncBag.RemoveAt(entry.PreSyncIndex);
        if (entry.SyncIndex != DoNotCall) syncBag.RemoveAt(entry.SyncIndex);

        objects.RemoveAt(handle.Index);
    }

    /// <summary>
    ///     A handle to a native object that is registered in the synchronizer.
    /// </summary>
    internal record struct Handle(Int32 Index);

    private sealed record Entry(NativeObject NativeObject, Int32 PreSyncIndex, Int32 SyncIndex);
}
