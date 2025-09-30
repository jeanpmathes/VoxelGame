// <copyright file="SceneFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Console;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;
using VoxelGame.UI;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Create scenes.
/// </summary>
public partial class SceneFactory
{
    private readonly Application.Client client;
    private CommandInvoker? commands;

    private Engine? engine;

    private UserInterfaceResources? uiResources;

    /// <summary>
    ///     Create a new scene factory.
    /// </summary>
    internal SceneFactory(Application.Client client)
    {
        this.client = client;
    }

    /// <summary>
    ///     Initialize all resources after loading.
    /// </summary>
    /// <param name="context">The context in which the resources are loaded.</param>
    public void InitializeResources(IResourceContext context)
    {
        engine = context.Get<Engine>();
        commands = context.Get<CommandInvoker>();

        uiResources = UserInterfaceResources.Retrieve(context);
    }

    /// <summary>
    ///     Create a new session scene.
    /// </summary>
    /// <param name="world">The world in which the session takes place.</param>
    /// <returns>The created session scene, or <c>null</c> if required resources are not loaded.</returns>
    public Scene? CreateSessionScene(World world)
    {
        if (engine == null || commands == null || uiResources == null)
        {
            LogMissingResources(logger);

            return null;
        }

        LogCreatingGameScene(logger, world.Data.Information.Name);

        return new SessionScene(client, world, commands, uiResources, engine);
    }

    /// <summary>
    ///     Create a new start scene.
    /// </summary>
    /// <param name="resourceLoadingIssueReport">A report of loading issues that occurred when loading the resources.</param>
    /// <param name="loadWorldDirectly">The index of the world to load directly, if any.</param>
    /// <returns>The created scene, or <c>null</c> if required resources are not loaded.</returns>
    public Scene? CreateStartScene(ResourceLoadingIssueReport? resourceLoadingIssueReport, Int32? loadWorldDirectly)
    {
        if (uiResources == null)
        {
            LogMissingResources(logger);

            return null;
        }

        LogCreatingStartScene(logger);

        return new StartScene(client, uiResources, resourceLoadingIssueReport, loadWorldDirectly);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SceneFactory>();

    [LoggerMessage(EventId = LogID.SceneFactory + 0, Level = LogLevel.Debug, Message = "Creating session scene for world {WorldName}")]
    private static partial void LogCreatingGameScene(ILogger logger, String worldName);

    [LoggerMessage(EventId = LogID.SceneFactory + 1, Level = LogLevel.Debug, Message = "Creating start scene")]
    private static partial void LogCreatingStartScene(ILogger logger);

    [LoggerMessage(EventId = LogID.SceneFactory + 2, Level = LogLevel.Warning, Message = "Missing resources to create scene")]
    private static partial void LogMissingResources(ILogger logger);

    #endregion LOGGING
}
