// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Actors.Components;
using VoxelGame.Client.Scenes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Actors;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed partial class Player : Core.Actors.Player, IPlayerDataProvider
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlacementSelection selector;

    /// <summary>
    ///     Create a client player.
    /// </summary>
    /// <param name="mass">The mass of the player.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    /// <param name="camera">The camera to use for this player.</param>
    /// <param name="ui">The user interface used for the game.</param>
    /// <param name="engine">The graphics engine to use for rendering.</param>
    /// <param name="scene">The scene in which the player is placed.</param>
    public Player(Double mass, BoundingVolume boundingVolume, Camera camera,
        GameUserInterface ui, Engine engine, SessionScene scene) : base(mass, boundingVolume)
    {
        Camera = camera;
        
        Head = new PlayerHead(camera, Body.Transform);
        camera.Position = Head.Position;

        Scene = scene;

        AddComponent<PlayerInput, Player>();
        AddComponent<PlayerMovement, Player>(); // Also updates the targeter.
        AddComponent<PlayerRotator, Player>();
        selector = AddComponent<PlacementSelection, Player>();
        AddComponent<Interaction, Player>();
        AddComponent<OverlayDisplay, Engine, Player>(engine);
        AddComponent<Targeter>();
        AddComponent<PlayerUI, GameUserInterface, Player>(ui);
        AddComponent<TargetingDisplay, Engine, Player>(engine);
        AddComponent<CrosshairDisplay, Engine, Player>(engine);
        
        LogCreatedNewPlayer(logger);
    }

    /// <inheritdoc />
    public override IOrientable Head { get; }
    
    /// <summary>
    ///     Get the view of this player.
    /// </summary>
    public IView View => Camera;

    /// <inheritdoc />
    public Property DebugData => new PlayerDebugProperties(this);

    String IPlayerDataProvider.Selection => selector.SelectionName;

    String IPlayerDataProvider.Mode => selector.ModeName;

    /// <summary>
    /// Get access to the camera of the player.
    /// </summary>
    internal Camera Camera { get; }

    /// <summary>
    /// Get access to the current scene in which the player is placed.
    /// </summary>
    internal SessionScene Scene { get; }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Player>();

    [LoggerMessage(EventId = LogID.Player + 0, Level = LogLevel.Debug, Message = "Created new player")]
    private static partial void LogCreatedNewPlayer(ILogger logger);

    #endregion LOGGING
}
