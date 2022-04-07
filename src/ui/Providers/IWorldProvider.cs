// <copyright file="IWorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    IEnumerable<(WorldInformation info, string path)> Worlds { get; }

    /// <summary>
    ///     Refresh all known worlds.
    /// </summary>
    void Refresh();

    /// <summary>
    ///     Load a specific world from disk.
    /// </summary>
    /// <param name="information">(Information describing the world to load.</param>
    /// <param name="path">The path to the world to load.</param>
    void LoadWorld(WorldInformation information, string path);

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
    /// <param name="path">The path to the world to delete.</param>
    void DeleteWorld(string path);
}
