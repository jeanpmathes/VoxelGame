// <copyright file="IWorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Updates;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides worlds that are loaded from disk or newly created.
/// </summary>
public interface IWorldProvider
{
    /// <summary>
    ///     Get all currently known worlds.
    ///     Only valid after a successful <see cref="Refresh"/>.
    /// </summary>
    IEnumerable<WorldData> Worlds { get; }

    /// <summary>
    ///     Start an operation to refresh the world provider.
    ///     This will change the status of the world provider.
    /// </summary>
    Operation Refresh();

    /// <summary>
    ///     Get the date and time of the last load of a world.
    ///     Only valid after a successful &lt;see cref="Refresh"/&gt;.
    /// </summary>
    /// <param name="data">The world.</param>
    /// <returns>The data and time of the last load, or null if the world has never been loaded.</returns>
    DateTime? GetDateTimeOfLastLoad(WorldData data);

    /// <summary>
    ///     Load a specific world from disk.
    ///     Only valid after a successful &lt;see cref="Refresh"/&gt;.
    /// </summary>
    /// <param name="data">The world to load, must be an object from <see cref="Worlds"/>, retrieved after a successful <see cref="Refresh"/>.</param>
    void LoadWorld(WorldData data);

    /// <summary>
    ///     Delete a world.
    ///     Only valid after a successful &lt;see cref="Refresh"/&gt;.
    /// </summary>
    /// <param name="data">
    ///     The world to delete, must be an object from <see cref="Worlds" />, retrieved after a successful
    ///     <see cref="Refresh" />.
    /// </param>
    void DeleteWorld(WorldData data);

    /// <summary>
    ///     Create a new world.
    /// </summary>
    /// <param name="name">The name of the world to create.</param>
    void CreateWorld(string name);

    /// <summary>
    ///     Check if a name is valid for a world.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if the given name is valid.</returns>
    bool IsWorldNameValid(string name);
}
