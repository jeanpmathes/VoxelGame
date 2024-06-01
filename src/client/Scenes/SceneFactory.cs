// <copyright file="SceneFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Create scenes.
/// </summary>
public partial class SceneFactory
{
    private readonly Application.Client client;

    /// <summary>
    ///     Create a new scene factory.
    /// </summary>
    internal SceneFactory(Application.Client client)
    {
        this.client = client;
    }

    /// <summary>
    ///     Create a new game scene.
    /// </summary>
    /// <param name="world">The world in which the game takes place.</param>
    /// <returns>The created game scene.</returns>
    public IScene CreateGameScene(World world)
    {
        LogCreatingGameScene(logger, world.Data.Information.Name);
        
        return new GameScene(client, world);
    }

    /// <summary>
    ///     Create a new start scene.
    /// </summary>
    /// <param name="resourceLoadingFailure">A resource loading failure that occurred during the game start, if any.</param>
    /// <param name="loadWorldDirectly">The index of the world to load directly, if any.</param>
    /// <returns>The created scene.</returns>
    public IScene CreateStartScene(ResourceLoadingFailure? resourceLoadingFailure, Int32? loadWorldDirectly)
    {
        LogCreatingStartScene(logger);
        
        return new StartScene(client, resourceLoadingFailure, loadWorldDirectly);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SceneFactory>();

    [LoggerMessage(EventId = Events.Scene, Level = LogLevel.Debug, Message = "Creating game scene for world {WorldName}")]
    private static partial void LogCreatingGameScene(ILogger logger, String worldName);

    [LoggerMessage(EventId = Events.Scene, Level = LogLevel.Debug, Message = "Creating start scene")]
    private static partial void LogCreatingStartScene(ILogger logger);

    #endregion LOGGING
}
