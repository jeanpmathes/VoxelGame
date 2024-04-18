// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Players;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Actors;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed class Player : Core.Actors.Player, IPlayerDataProvider
{
    private readonly Camera camera;

    private readonly Input input;

    private readonly GameScene scene;

    private readonly PlacementSelection selector;
    private readonly VisualInterface visualInterface;

    private Vector3d movement;
    private MovementStrategy movementStrategy;

    private BlockInstance? targetBlock;
    private FluidInstance? targetFluid;

    private Vector3i? targetPosition;
    private BlockSide targetSide;

    /// <summary>
    ///     Create a client player.
    /// </summary>
    /// <param name="world">The world in which the client player will be placed.</param>
    /// <param name="mass">The mass of the player.</param>
    /// <param name="camera">The camera to use for this player.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    /// <param name="visualInterface">The visual interface to use for this player.</param>
    /// <param name="scene">The scene in which the player is placed.</param>
    public Player(World world, Single mass, Camera camera, BoundingVolume boundingVolume,
        VisualInterface visualInterface, GameScene scene) : base(world, mass, boundingVolume)
    {
        this.camera = camera;
        camera.Position = Position;

        Head = new Head(camera, this);

        this.scene = scene;
        this.visualInterface = visualInterface;

        input = new Input();
        selector = new PlacementSelection(input, () => targetBlock?.Block);

        movementStrategy = new DefaultMovement(input, flyingSpeed: 1.0);
    }

    /// <inheritdoc />
    public override IOrientable Head { get; }

    /// <summary>
    ///     The previous position before teleporting.
    /// </summary>
    public Vector3d PreviousPosition { get; private set; }

    /// <inheritdoc />
    public override BlockSide TargetSide => targetSide;

    /// <summary>
    ///     Get the view of this player.
    /// </summary>
    public IView View => camera;

    /// <inheritdoc />
    public override Vector3d Movement => movement;

    /// <summary>
    ///     The targeted block, or a default block if no block is targeted.
    /// </summary>
    public BlockInstance TargetBlock => targetBlock ?? BlockInstance.Default;

    /// <summary>
    ///     The targeted fluid, or a default fluid if no fluid is targeted.
    /// </summary>
    public FluidInstance TargetFluid => targetFluid ?? FluidInstance.Default;

    /// <inheritdoc cref="PhysicsActor" />
    public override Vector3i? TargetPosition => targetPosition;

    /// <inheritdoc />
    public Property DebugData => new DebugProperties(this);

    String IPlayerDataProvider.Selection => selector.SelectionName;

    String IPlayerDataProvider.Mode => selector.ModeName;

    /// <summary>
    /// Set the flying speed of the player.
    /// </summary>
    /// <param name="speed">The new flying speed.</param>
    public void SetFlyingSpeed(Double speed)
    {
        movementStrategy.FlyingSpeed = speed;
    }

    /// <summary>
    ///     Set whether the player is in freecam mode.
    ///     Freecam mode means the camera can move freely without the player moving.
    /// </summary>
    /// <param name="freecam">True if the player is in freecam mode, false otherwise.</param>
    public void SetFreecam(Boolean freecam)
    {
        movementStrategy = freecam
            ? new FreecamMovement(this, input, movementStrategy.FlyingSpeed)
            : new DefaultMovement(input, movementStrategy.FlyingSpeed);
    }

    /// <summary>
    ///     Set whether the overlay rendering is allowed.
    /// </summary>
    public void SetOverlayAllowed(Boolean allowed)
    {
        Throw.IfDisposed(disposed);

        visualInterface.IsOverlayAllowed = allowed;
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
    ///     After this no more updates will be called.
    /// </summary>
    public void OnDeactivate()
    {
        Throw.IfDisposed(disposed);

        visualInterface.Deactivate();
    }

    private static BoxCollider? GetBlockBoundsIfVisualized(World world, Block block, Vector3i position)
    {
        Boolean visualized = !block.IsReplaceable;

        if (Program.IsDebug)
            visualized |= block != Blocks.Instance.Air;

        return visualized ? block.GetCollider(world, position) : null;
    }

    /// <inheritdoc />
    protected override void OnUpdate(Double deltaTime)
    {
        Throw.IfDisposed(disposed);

        movement = Vector3d.Zero;

        camera.Position = movementStrategy.GetCameraPosition(Head);

        UpdateTargets();

        if (scene is {IsWindowFocused: true, IsOverlayOpen: false})
        {
            movementStrategy.ApplyMovement(this, deltaTime);

            DoLookInput();

            DoBlockFluidSelection();
            DoWorldInteraction();
        }

        if (scene is {IsWindowFocused: true}) visualInterface.UpdateInput();

        SetBlockAndFluidOverlays();

        // Because interaction can change the target block or the bounding box,
        // we search again for the target and update the selection now.

        UpdateTargets();
        UpdateSelection();

        visualInterface.Update();
        input.Update(deltaTime);
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

    private void UpdateTargets()
    {
        var ray = new Ray(Head.Position, Head.Forward, length: 6f);
        (Vector3i, BlockSide)? hit = Raycast.CastBlockRay(World, ray);

        if (hit is var (hitPosition, hitSide) && World.GetContent(hitPosition) is var (block, fluid))
        {
            targetPosition = hitPosition;
            targetSide = hitSide;

            (targetBlock, targetFluid) = (block, fluid);
        }
        else
        {
            targetPosition = null;
            targetSide = BlockSide.All;

            (targetBlock, targetFluid) = (null, null);
        }
    }

    private void UpdateSelection()
    {
        if (targetPosition is {} position && targetBlock is {} block) visualInterface.SetSelectionBox(GetBlockBoundsIfVisualized(World, block.Block, position));
        else visualInterface.SetSelectionBox(collider: null);
    }

    private void DoLookInput()
    {
        // The pitch is clamped in the camera class.

        (Double yaw, Double pitch) = Application.Client.Instance.Keybinds.LookBind.Value;
        camera.Yaw += yaw;
        camera.Pitch += pitch;

        Rotation = Quaterniond.FromAxisAngle(Vector3d.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
    }

    private void DoWorldInteraction()
    {
        if (targetBlock == null || targetFluid == null || targetPosition == null) return;

        PlaceInteract(targetBlock.Value, targetPosition.Value);
        DestroyInteract(targetBlock.Value, targetPosition.Value);
    }

    private void PlaceInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        if (!input.ShouldInteract) return;

        Vector3i placePosition = targetedPosition;

        if (input.IsInteractionBlocked || !targetedBlock.Block.IsInteractable)
        {
            if (!targetedBlock.Block.IsReplaceable) placePosition = targetSide.Offset(placePosition);

            // Prevent block placement if the block would intersect the player.
            if (selector is {IsBlockMode: true, ActiveBlock.IsSolid: true} && Collider.Intersects(
                    selector.ActiveBlock.GetCollider(World, placePosition))) return;

            if (selector.IsBlockMode) selector.ActiveBlock.Place(World, placePosition, this);
            else selector.ActiveFluid.Fill(World, placePosition, FluidLevel.One, BlockSide.Top, out _);

            input.RegisterInteraction();
        }
        else if (targetedBlock.Block.IsInteractable)
        {
            targetedBlock.Block.ActorInteract(this, targetedPosition);

            input.RegisterInteraction();
        }
    }

    private void DestroyInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        if (input.ShouldDestroy)
        {
            if (selector.IsBlockMode) targetedBlock.Block.Destroy(World, targetedPosition, this);
            else TakeFluid(targetedPosition);

            input.RegisterInteraction();
        }

        void TakeFluid(Vector3i position)
        {
            var level = FluidLevel.One;

            if (!targetedBlock.Block.IsReplaceable)
                position = targetSide.Offset(position);

            World.GetFluid(position)?.Fluid.Take(World, position, ref level);
        }
    }

    #region IDisposable Support

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

    #endregion IDisposable Support
}
