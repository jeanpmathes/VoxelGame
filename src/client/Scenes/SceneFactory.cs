﻿// <copyright file="SceneFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Client.Application;
using VoxelGame.Client.Console;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Create scenes.
/// </summary>
public class SceneFactory
{
    private readonly Application.Client client;
    private readonly CommandInvoker commandInvoker;

    /// <summary>
    ///     Create a new scene factory.
    /// </summary>
    internal SceneFactory(Application.Client client)
    {
        this.client = client;

        commandInvoker = GameConsole.BuildInvoker();
    }

    /// <summary>
    ///     Create a new game scene.
    /// </summary>
    /// <param name="world">The world in which the game takes place.</param>
    /// <param name="game">This will be set to the newly created game.</param>
    /// <returns>The created game scene.</returns>
    public IScene CreateGameScene(World world, out Game game)
    {
        GameScene scene = new(client, world, new GameConsole(commandInvoker));
        game = scene.Game;

        return scene;
    }

    /// <summary>
    ///     Create a new start scene.
    /// </summary>
    /// <param name="resourceLoadingFailure">A resource loading failure that occurred during the game start, if any.</param>
    /// <returns>The created scene.</returns>
    public IScene CreateStartScene(ResourceLoadingFailure? resourceLoadingFailure)
    {
        return new StartScene(client, resourceLoadingFailure);
    }
}


