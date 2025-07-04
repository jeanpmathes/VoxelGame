﻿// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Players;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Actors;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed partial class Player : Core.Actors.Player, IPlayerDataProvider
{
    private readonly Camera camera;

    private readonly Input input;
    private readonly Targeting targeting;
    private readonly PlacementSelection selector;
    private readonly Interaction interaction;

    private readonly VisualInterface visualInterface;

    private readonly GameScene scene;

    private Vector3d movement;
    private MovementStrategy movementStrategy;

    /// <summary>
    ///     Create a client player.
    /// </summary>
    /// <param name="mass">The mass of the player.</param>
    /// <param name="camera">The camera to use for this player.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    /// <param name="visualInterface">The visual interface to use for this player.</param>
    /// <param name="scene">The scene in which the player is placed.</param>
    public Player(Single mass, Camera camera, BoundingVolume boundingVolume,
        VisualInterface visualInterface, GameScene scene) : base(mass, boundingVolume)
    {
        this.camera = camera;
        camera.Position = Position;

        Head = new Head(camera, this);

        this.scene = scene;
        this.visualInterface = visualInterface;

        input = new Input(scene.Client.Keybinds);
        targeting = new Targeting();
        selector = new PlacementSelection(input, () => targeting.Block?.Block);
        interaction = new Interaction(this, input, targeting, selector);

        movementStrategy = new DefaultMovement(input, flyingSpeed: 1.0);

        LogCreatedNewPlayer(logger);
    }

    /// <inheritdoc />
    public override IOrientable Head { get; }

    /// <summary>
    ///     The previous position before teleporting.
    /// </summary>
    public Vector3d PreviousPosition { get; private set; }

    /// <inheritdoc />
    public override Side TargetSide => targeting.Side;

    /// <inheritdoc cref="PhysicsActor" />
    public override Vector3i? TargetPosition => targeting.Position;

    /// <summary>
    ///     Get the view of this player.
    /// </summary>
    public IView View => camera;

    /// <inheritdoc />
    public override Vector3d Movement => movement;

    /// <inheritdoc />
    public Property DebugData => new DebugProperties(this, targeting);

    String IPlayerDataProvider.Selection => selector.SelectionName;

    String IPlayerDataProvider.Mode => selector.ModeName;

    /// <summary>
    ///     Set the flying speed of the player.
    /// </summary>
    /// <param name="speed">The new flying speed.</param>
    public void SetFlyingSpeed(Double speed)
    {
        Throw.IfDisposed(disposed);

        movementStrategy.FlyingSpeed = speed;

        LogSetFlyingSpeed(logger, speed);
    }

    /// <summary>
    ///     Set whether the player is in freecam mode.
    ///     Freecam mode means the camera can move freely without the player moving.
    /// </summary>
    /// <param name="freecam">True if the player is in freecam mode, false otherwise.</param>
    public void SetFreecam(Boolean freecam)
    {
        Throw.IfDisposed(disposed);

        movementStrategy = freecam
            ? new FreecamMovement(this, input, movementStrategy.FlyingSpeed)
            : new DefaultMovement(input, movementStrategy.FlyingSpeed);

        LogSetFreecam(logger, freecam);
    }

    /// <summary>
    ///     Set whether the overlay rendering is allowed.
    /// </summary>
    public void SetOverlayAllowed(Boolean allowed)
    {
        Throw.IfDisposed(disposed);

        visualInterface.IsOverlayAllowed = allowed;

        LogSetOverlayAllowed(logger, allowed);
    }

    /// <summary>
    ///     Teleport the player to a new position.
    /// </summary>
    /// <param name="position">The new position.</param>
    public void Teleport(Vector3d position)
    {
        Throw.IfDisposed(disposed);

        PreviousPosition = Position;
        Position = position;

        LogTeleport(logger, position);
    }

    /// <summary>
    ///     Called when the world activates.
    ///     After this updates will be called.
    /// </summary>
    public void OnActivate()
    {
        Throw.IfDisposed(disposed);

        visualInterface.Activate();
        visualInterface.UpdateData();
    }

    /// <summary>
    ///     Called when the world deactivates.
    /// </summary>
    public void OnDeactivate()
    {
        Throw.IfDisposed(disposed);

        visualInterface.Deactivate();
    }

    /// <inheritdoc />
    protected override void OnPlayerUpdate(Double deltaTime)
    {
        Throw.IfDisposed(disposed);

        movement = Vector3d.Zero;

        camera.Position = movementStrategy.GetCameraPosition(Head);

        targeting.LogicUpdate(Head, World);

        if (scene is {IsWindowFocused: true, IsOverlayOpen: false})
        {
            movement = movementStrategy.ApplyMovement(this, deltaTime);

            DoLookInput();
            DoBlockFluidSelection();

            interaction.Perform();
        }

        if (scene is {IsWindowFocused: true}) visualInterface.UpdateInput();

        SetBlockAndFluidOverlays();

        // Because interaction can change the target block or the bounding box,
        // we search again for the target and update the selection now.
        targeting.LogicUpdate(Head, World);

        visualInterface.SetSelectionBoxTarget(World, targeting.Block, targeting.Position);
        visualInterface.LogicUpdate();

        input.LogicUpdate(deltaTime);
    }

    private void DoBlockFluidSelection()
    {
        Boolean isUpdated = selector.DoBlockFluidSelection();
        if (isUpdated) visualInterface.UpdateData();
    }

    private void SetBlockAndFluidOverlays()
    {
        Vector3i center = camera.Position.Floor();
        Frustum frustum = camera.GetPartialFrustum(near: 0.0, camera.Definition.Clipping.near);

        visualInterface.BuildOverlay(this, Raycast.CastFrustum(World, center, range: 1, frustum));
    }

    private void DoLookInput()
    {
        // The pitch is clamped in the camera class.

        (Double yaw, Double pitch) = scene.Client.Keybinds.LookBind.Value;
        camera.Yaw += yaw;
        camera.Pitch += pitch;

        Rotation = Quaterniond.FromAxisAngle(Vector3d.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Player>();

    [LoggerMessage(EventId = LogID.Player + 0, Level = LogLevel.Debug, Message = "Created new player")]
    private static partial void LogCreatedNewPlayer(ILogger logger);

    [LoggerMessage(EventId = LogID.Player + 1, Level = LogLevel.Debug, Message = "Set flying speed to {Speed}")]
    private static partial void LogSetFlyingSpeed(ILogger logger, Double speed);

    [LoggerMessage(EventId = LogID.Player + 2, Level = LogLevel.Debug, Message = "Set freecam mode to {Freecam}")]
    private static partial void LogSetFreecam(ILogger logger, Boolean freecam);

    [LoggerMessage(EventId = LogID.Player + 3, Level = LogLevel.Debug, Message = "Set overlay rendering to {Allowed}")]
    private static partial void LogSetOverlayAllowed(ILogger logger, Boolean allowed);

    [LoggerMessage(EventId = LogID.Player + 4, Level = LogLevel.Debug, Message = "Teleported player to {Position}")]
    private static partial void LogTeleport(ILogger logger, Vector3d position);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed)
            return;

        if (disposing) visualInterface.Dispose();

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion DISPOSABLE
}
