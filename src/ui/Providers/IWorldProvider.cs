// <copyright file="IWorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides worlds that are loaded from disk or newly created.
/// </summary>
public interface IWorldProvider
{
    /// <summary>
    ///     Get all currently known worlds.
    /// </summary>
    IEnumerable<WorldData> Worlds { get; }

    /// <summary>
    ///     Get the date and time of the last load of a world.
    /// </summary>
    /// <param name="data">The world.</param>
    /// <returns>The data and time of the last load, or null if the world has never been loaded.</returns>
    DateTime? GetDateTimeOfLastLoad(WorldData data);

    /// <summary>
    ///     Refresh all known worlds.
    /// </summary>
    void Refresh();

    /// <summary>
    ///     Load a specific world from disk.
    /// </summary>
    /// <param name="data">The world to load.</param>
    void LoadWorld(WorldData data);

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

    /// <summary>
    ///     Delete a world.
    /// </summary>
    /// <param name="data">The world to delete.</param>
    void DeleteWorld(WorldData data);
}
