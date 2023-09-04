// <copyright file="Synchronizer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using VoxelGame.Core;
using VoxelGame.Core.Collections;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     Holds and synchronizes all <see cref="NativeObject" />s.
/// </summary>
public class Synchronizer
{
    private const int DoNotCall = -1;
    private readonly GappedList<Entry?> objects = new(gapValue: null);

    private readonly GappedList<NativeObject?> preSyncList = new(gapValue: null);
    private readonly GappedList<NativeObject?> syncList = new(gapValue: null);

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
        foreach (NativeObject? nativeObject in preSyncList.AsSpan()) nativeObject?.PrepareSynchronization();
        foreach (NativeObject? nativeObject in syncList.AsSpan()) nativeObject?.Synchronize();
    }

    /// <summary>
    ///     Register a new native object.
    /// </summary>
    internal Handle RegisterObject(NativeObject nativeObject)
    {
        ApplicationInformation.Instance.EnsureMainThread(objects);

        int preSyncIndex = preSyncList.Add(nativeObject);
        int syncIndex = syncList.Add(nativeObject);

        return new Handle(objects.Add(new Entry(nativeObject, preSyncIndex, syncIndex)));
    }

    /// <summary>
    ///     Disable the call of <see cref="NativeObject.PrepareSynchronization" /> for the given handle.
    /// </summary>
    internal void DisablePreSync(Handle handle)
    {
        ApplicationInformation.Instance.EnsureMainThread(objects);

        Entry? entry = objects[handle.Index];
        Debug.Assert(entry != null);

        if (entry.PreSyncIndex == DoNotCall) return;

        preSyncList.RemoveAt(entry.PreSyncIndex);
        objects[handle.Index] = entry with {PreSyncIndex = DoNotCall};
    }

    /// <summary>
    ///     Disable the call of <see cref="NativeObject.Synchronize" /> for the given handle.
    /// </summary>
    internal void DisableSync(Handle handle)
    {
        ApplicationInformation.Instance.EnsureMainThread(objects);

        Entry? entry = objects[handle.Index];
        Debug.Assert(entry != null);

        if (entry.SyncIndex == DoNotCall) return;

        syncList.RemoveAt(entry.SyncIndex);
        objects[handle.Index] = entry with {SyncIndex = DoNotCall};
    }

    /// <summary>
    ///     De-register a native object.
    /// </summary>
    internal void DeRegisterObject(Handle handle)
    {
        ApplicationInformation.Instance.EnsureMainThread(objects);

        Entry? entry = objects[handle.Index];
        Debug.Assert(entry != null);

        if (entry.PreSyncIndex != DoNotCall) preSyncList.RemoveAt(entry.PreSyncIndex);
        if (entry.SyncIndex != DoNotCall) syncList.RemoveAt(entry.SyncIndex);

        objects.RemoveAt(handle.Index);
    }

    /// <summary>
    ///     A handle to a native object that is registered in the synchronizer.
    /// </summary>
    internal record struct Handle(int Index);

    private sealed record Entry(NativeObject NativeObject, int PreSyncIndex, int SyncIndex);
}
