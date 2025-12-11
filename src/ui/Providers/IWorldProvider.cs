// <copyright file="IWorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Updates;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides worlds that are loaded from disk or newly created.
/// </summary>
public interface IWorldProvider
{
    /// <summary>
    ///     The directory where the worlds are stored.
    /// </summary>
    DirectoryInfo WorldsDirectory { get; }

    /// <summary>
    ///     Get all currently known worlds.
    ///     Only valid after a successful <see cref="Refresh" />.
    /// </summary>
    IEnumerable<IWorldInfo> Worlds { get; }

    /// <summary>
    ///     Start an operation to refresh the world provider.
    ///     This will change the status of the world provider.
    /// </summary>
    Operation Refresh();

    /// <summary>
    ///     Determine properties of a world.
    ///     Properties are extended information that might take time to load.
    ///     Only valid after a successful <see cref="Refresh" />.
    /// </summary>
    /// <param name="info">
    ///     The world, must be an object from <see cref="Worlds" />, retrieved after a successful
    ///     <see cref="Refresh" />.
    /// </param>
    /// <returns>The operation to get the properties of the world.</returns>
    Operation<Property> GetWorldProperties(IWorldInfo info);

    /// <summary>
    ///     Load a specific world from disk.
    ///     Only valid after a successful <see cref="Refresh" />.
    ///     Will cause a scene change.
    /// </summary>
    /// <param name="info">
    ///     The world to load, must be an object from <see cref="Worlds" />, retrieved after a successful
    ///     <see cref="Refresh" />.
    /// </param>
    void LoadAndActivateWorld(IWorldInfo info);

    /// <summary>
    ///     Create a new world and then load it. Will cause a scene change.
    /// </summary>
    /// <param name="name">The name of the world to create.</param>
    void CreateAndActivateWorld(String name);

    /// <summary>
    ///     Delete a world.
    ///     Only valid after a successful <see cref="Refresh" />.
    /// </summary>
    /// <param name="info">
    ///     The world to delete, must be an object from <see cref="Worlds" />, retrieved after a successful
    ///     <see cref="Refresh" />.
    /// </param>
    /// <returns>The operation to delete the world.</returns>
    Operation DeleteWorld(IWorldInfo info);

    /// <summary>
    ///     Duplicate a world, it will be added to the list of worlds.
    ///     Only valid after a successful <see cref="Refresh" />.
    /// </summary>
    /// <param name="info">
    ///     The world to duplicate.
    ///     Must be an object from <see cref="Worlds" />, retrieved after a successful <see cref="Refresh" />.
    /// </param>
    /// <param name="duplicateName">The name of the duplicated world. Must be a valid name.</param>
    /// <returns>The operation that duplicates the world.</returns>
    Operation DuplicateWorld(IWorldInfo info, String duplicateName);

    /// <summary>
    ///     Check if a name is valid for a world.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if the given name is valid.</returns>
    Boolean IsWorldNameValid(String name);

    /// <summary>
    ///     Rename a world.
    /// </summary>
    /// <param name="info">
    ///     The world to rename. Must be an object from <see cref="Worlds" />, retrieved after a successful
    ///     <see cref="Refresh" />.
    /// </param>
    /// <param name="newName">The new name of the world. Must be a valid name.</param>
    void RenameWorld(IWorldInfo info, String newName);

    /// <summary>
    ///     Set whether a world is a favorite.
    /// </summary>
    /// <param name="info">The world for which to set the favorite status.</param>
    /// <param name="isFavorite">Whether the world should be a favorite.</param>
    void SetFavorite(IWorldInfo info, Boolean isFavorite);

    /// <summary>
    ///     Information about a single world.
    /// </summary>
    interface IWorldInfo
    {
        /// <summary>
        ///     The name of the world.
        /// </summary>
        String Name { get; }

        /// <summary>
        ///     The version of the client in which the world was saved last.
        /// </summary>
        String Version { get; }

        /// <summary>
        ///     The directory where the world is stored.
        /// </summary>
        DirectoryInfo Directory { get; }

        /// <summary>
        ///     Date and time when the world was created.
        /// </summary>
        DateTime DateTimeOfCreation { get; }

        /// <summary>
        ///     Date and time when the world was last loaded, or null if it was never loaded.
        /// </summary>
        DateTime? DateTimeOfLastLoad { get; }

        /// <summary>
        ///     Whether the world is a favorite.
        /// </summary>
        Boolean IsFavorite { get; }
    }
}
