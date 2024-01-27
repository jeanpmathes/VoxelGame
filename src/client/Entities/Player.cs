// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Entities.Players;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed class Player : Core.Entities.Player, IPlayerDataProvider
{
    private const float FlyingSpeedFactor = 5f;
    private const float FlyingSprintSpeedFactor = 25f;

    private const float DiveSpeed = 8f;
    private const float JumpForce = 25000f;
    private const float Speed = 4f;
    private const float SprintSpeed = 6f;
    private const float SwimSpeed = 4f;

    private readonly Camera camera;
    private readonly Vector3d cameraOffset = new(x: 0f, y: 0.65f, z: 0f);

    private readonly Input input;

    private readonly GameScene scene;

    private readonly Vector3d maxForce = new(x: 500f, y: 0f, z: 500f);
    private readonly Vector3d maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

    private readonly PlacementSelection selector;
    private readonly VisualInterface visualInterface;

    private bool isFirstUpdate = true;

    private Vector3d movement;

    private Vector3i headPosition;

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
    /// <param name="ui">The ui used to display player information.</param>
    /// <param name="resources">The resources used to render the player.</param>
    /// <param name="scene">The scene in which the player is placed.</param>
    public Player(World world, float mass, Camera camera, BoundingVolume boundingVolume,
        GameUserInterface ui, GameResources resources, GameScene scene) : base(world, mass, boundingVolume)
    {
        this.camera = camera;
        camera.Position = Position;

        this.scene = scene;

        visualInterface = new VisualInterface(this, ui, resources);
        input = new Input(this);

        selector = new PlacementSelection(input, () => targetBlock?.Block);
    }

    /// <summary>
    ///     Get or set the flying state of the player.
    /// </summary>
    public double FlyingSpeed { get; set; } = 1f;

    /// <inheritdoc />
    public override Vector3d LookingDirection => camera.Front;

    /// <summary>
    ///     Get the up vector of the player camera.
    /// </summary>
    public Vector3d CameraUp => camera.Up;

    /// <summary>
    ///     Get the right vector of the player camera.
    /// </summary>
    public Vector3d CameraRight => camera.Right;

    /// <summary>
    ///     Get the looking position of the player, meaning the position of the camera.
    /// </summary>
    public Vector3d LookingPosition => camera.Position;

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

    /// <inheritdoc cref="PhysicsEntity" />
    public override Vector3i? TargetPosition => targetPosition;

    Vector3i IPlayerDataProvider.HeadPosition => headPosition;

    BlockInstance IPlayerDataProvider.TargetBlock => targetBlock ?? BlockInstance.Default;

    FluidInstance IPlayerDataProvider.TargetFluid => targetFluid ?? FluidInstance.Default;

    string IPlayerDataProvider.WorldDebugData => World.Map.GetPositionDebugData(Position);

    double IPlayerDataProvider.Temperature => World.Map.GetTemperature(Position);

    string IPlayerDataProvider.Selection => selector.SelectionName;

    string IPlayerDataProvider.Mode => selector.ModeName;

    /// <summary>
    ///     Set whether the overlay rendering is allowed.
    /// </summary>
    public void SetOverlayAllowed(bool allowed)
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
    /// Called when the world activates.
    /// After this updates will be called.
    /// </summary>
    public void OnActivate()
    {
        Throw.IfDisposed(disposed);

        visualInterface.Activate();
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
        bool visualized = !block.IsReplaceable;

        if (Program.IsDebug)
            visualized |= block != Blocks.Instance.Air;

        return visualized ? block.GetCollider(world, position) : null;
    }

    /// <inheritdoc />
    protected override void OnUpdate(double deltaTime)
    {
        Throw.IfDisposed(disposed);

        movement = Vector3d.Zero;

        camera.Position = Position + cameraOffset;

        UpdateTargets();

        if (scene.IsWindowFocused)
        {
            if (!scene.IsOverlayOpen)
            {
                HandleMovementInput(deltaTime);
                HandleLookInput();

                DoBlockFluidSelection();
                DoWorldInteraction();

                visualInterface.UpdateInput();
            }

            headPosition = camera.Position.Floor();

            SetBlockAndFluidOverlays();

            isFirstUpdate = false;
        }

        // Because interaction can change the target block or the bounding box,
        // we search again for the target and update the selection now.

        UpdateTargets();
        UpdateSelection();

        visualInterface.Update();
        input.Update(deltaTime);
    }

    private void DoBlockFluidSelection()
    {
        bool isUpdated = selector.DoBlockFluidSelection();
        if (isUpdated || isFirstUpdate) visualInterface.UpdateData();
    }

    private void SetBlockAndFluidOverlays()
    {
        Vector3i center = camera.Position.Floor();
        Frustum frustum = camera.GetPartialFrustum(near: 0.0, camera.NearClipping);

        IEnumerable<(Content content, Vector3i position)> positions = Raycast.CastFrustum(World, center, range: 1, frustum);

        visualInterface.BuildOverlay(positions);
    }

    private void UpdateTargets()
    {
        var ray = new Ray(camera.Position, camera.Front, length: 6f);
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

    private void HandleMovementInput(double deltaTime)
    {
        if (DoPhysics)
        {
            DoNormalMovement();
        }
        else
        {
            Vector3d offset = input.GetFlyingMovement(FlyingSpeed * FlyingSpeedFactor, FlyingSpeed * FlyingSprintSpeedFactor);
            Position += offset * deltaTime;
        }
    }

    private void DoNormalMovement()
    {
        movement = input.GetMovement(Speed, SprintSpeed);
        Move(movement, maxForce);

        if (!(input.ShouldJump ^ input.ShouldCrouch)) return;

        if (input.ShouldJump)
        {
            if (IsGrounded) AddForce(new Vector3d(x: 0, JumpForce, z: 0));
            else if (IsSwimming) Move(Vector3d.UnitY * SwimSpeed, maxSwimForce);
        }
        else
        {
            if (IsSwimming) Move(Vector3d.UnitY * -DiveSpeed, maxSwimForce);
        }
    }

    private void HandleLookInput()
    {
        // Apply the camera pitch and yaw (the pitch is clamped in the camera class)
        (double yaw, double pitch) = Application.Client.Instance.Keybinds.LookBind.Value;
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
        BlockInstance currentTarget = targetedBlock;

        if (!input.ShouldInteract) return;

        Vector3i placePosition = targetedPosition;

        if (input.IsInteractionBlocked || !currentTarget.Block.IsInteractable)
        {
            if (!currentTarget.Block.IsReplaceable) placePosition = targetSide.Offset(placePosition);

            // Prevent block placement if the block would intersect the player.
            if (selector is {IsBlockMode: true, ActiveBlock.IsSolid: true} && Collider.Intersects(
                    selector.ActiveBlock.GetCollider(World, placePosition))) return;

            if (selector.IsBlockMode) selector.ActiveBlock.Place(World, placePosition, this);
            else selector.ActiveFluid.Fill(World, placePosition, FluidLevel.One, BlockSide.Top, out _);

            input.RegisterInteraction();
        }
        else if (currentTarget.Block.IsInteractable)
        {
            currentTarget.Block.EntityInteract(this, targetedPosition);

            input.RegisterInteraction();
        }
    }

    private void DestroyInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        BlockInstance currentTarget = targetedBlock;

        if (input.ShouldDestroy)
        {
            if (selector.IsBlockMode) currentTarget.Block.Destroy(World, targetedPosition, this);
            else TakeFluid(targetedPosition);

            input.RegisterInteraction();
        }

        void TakeFluid(Vector3i position)
        {
            var level = FluidLevel.One;

            if (!currentTarget.Block.IsReplaceable)
                position = targetSide.Offset(position);

            World.GetFluid(position)?.Fluid.Take(World, position, ref level);
        }
    }

    #region IDisposable Support

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing) visualInterface.Dispose();

        disposed = true;
    }

    #endregion IDisposable Support
}
