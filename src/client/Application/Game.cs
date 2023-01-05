// <copyright file="Game.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Client.Console;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Updates;

namespace VoxelGame.Client.Application;

/// <summary>
///     Represents a running game, could also be called a session.
/// </summary>
public sealed class Game : IDisposable
{
    /// <summary>
    ///     Create a new game instance.
    /// </summary>
    /// <param name="world">The world in which the game is played.</param>
    /// <param name="player">The player playing the game.</param>
    public Game(ClientWorld world, ClientPlayer player)
    {
        World = world;
        Player = player;
        Console = null!;
    }

    /// <summary>
    ///     The player of the game.
    /// </summary>
    public ClientPlayer Player { get; }

    /// <summary>
    ///     The game in which the player is playing.
    /// </summary>
    public ClientWorld World { get; }

    /// <summary>
    ///     Get the console used for the game.
    /// </summary>
    public ConsoleWrapper Console { get; private set; }

    /// <summary>
    ///     Get the update counter used for the game.
    /// </summary>
    public UpdateCounter UpdateCounter { get; } = new();

    /// <summary>
    ///     Set up the game.
    /// </summary>
    /// <param name="console">The console to use.</param>
    public void Initialize(ConsoleWrapper console)
    {
        Console = console;

        UpdateCounter.Reset();
    }

    /// <summary>
    ///     Perform one update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void Update(double deltaTime)
    {
        UpdateCounter.Increment();

        Console.Flush();

        World.Update(deltaTime);
    }

    /// <summary>
    ///     Perform one render cycle.
    /// </summary>
    public void Render()
    {
        World.Render();
    }

    #region IDisposable Support

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Game()
    {
        Dispose(disposing: false);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        World.Dispose();
        Player.Dispose();
    }

    #endregion IDisposable Support
}


