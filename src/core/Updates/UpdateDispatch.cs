// <copyright file="UpdateDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Stores and updates all operations and routines implementing <see cref="IUpdate"/>.
/// </summary>
public class UpdateDispatch
{
    private readonly Bag<IUpdate> entries = new(null!);

    /// <summary>
    ///     Create a new operation update dispatch instance.
    /// </summary>
    /// <param name="singleton">Whether to make this the singleton instance.</param>
    public UpdateDispatch(Boolean singleton = false)
    {
        if (!singleton) return;

        Debug.Assert(Instance == null);

        Instance = this;
    }

    /// <summary>
    ///     The singleton instance of the operation update dispatch.
    /// </summary>
    public static UpdateDispatch? Instance { get; private set; }

    /// <summary>
    ///     Perform an update.
    /// </summary>
    public void Update()
    {
        entries.Apply(entry =>
        {
            entry.Update();

            return entry.IsRunning;
        });
    }

    /// <summary>
    ///     Add an entry to the dispatch.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    public void Add(IUpdate entry)
    {
        entries.Add(entry);
    }
}
