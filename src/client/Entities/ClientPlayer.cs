// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed class ClientPlayer : Player, IPlayerDataProvider
{
    private const float FlyingSpeedFactor = 5f;
    private const float FlyingSprintSpeedFactor = 25f;
    private readonly Camera camera;
    private readonly Vector3d cameraOffset = new(x: 0f, y: 0.65f, z: 0f);
    private readonly float diveSpeed = 8f;

    private readonly PlayerInput input;

    private readonly float jumpForce = 25000f;

    private readonly Vector3d maxForce = new(x: 500f, y: 0f, z: 500f);
    private readonly Vector3d maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

    private readonly float speed = 4f;
    private readonly float sprintSpeed = 6f;
    private readonly float swimSpeed = 4f;

    private readonly PlayerVisualization visualization;

    private Block activeBlock;
    private Fluid activeFluid;

    private bool blockMode = true;

    private bool firstUpdate = true;
    private Vector3i headPosition;

    private Vector3d movement;

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
    public ClientPlayer(World world, float mass, Camera camera, BoundingVolume boundingVolume,
        GameUserInterface ui) : base(world, mass, boundingVolume)
    {
        this.camera = camera;
        camera.Position = Position;

        visualization = new PlayerVisualization(this, ui);
        input = new PlayerInput(this);

        activeBlock = Block.Grass;
        activeFluid = Fluid.Water;
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
    /// Get the view of this player.
    /// </summary>
    public IView View => camera;

    /// <summary>
    ///     Get or set whether any overlay rendering is enabled.
    /// </summary>
    public bool OverlayEnabled { get; set; } = true;

    /// <summary>
    ///     Get the dimensions of the near view plane.
    /// </summary>
    public (Vector3d a, Vector3d b) NearDimensions
    {
        get
        {
            (double width, double height) = camera.GetDimensionsAt(camera.NearClipping);

            Vector3d position = camera.Position + camera.Front * camera.NearClipping;

            Vector3d up = camera.Up * height * 0.5f;
            Vector3d right = camera.Right * width * 0.5f;

            return (position - up - right, position + up + right);
        }
    }

    /// <inheritdoc />
    public override Vector3d Movement => movement;

    /// <inheritdoc cref="PhysicsEntity" />
    public override Vector3i? TargetPosition => targetPosition;

    Vector3i IPlayerDataProvider.HeadPosition => headPosition;

    BlockInstance IPlayerDataProvider.TargetBlock => targetBlock ?? BlockInstance.Default;

    FluidInstance IPlayerDataProvider.TargetFluid => targetFluid ?? FluidInstance.Default;

    string IPlayerDataProvider.WorldDebugData => World.Map.GetPositionDebugData(Position);

    double IPlayerDataProvider.Temperature => World.Map.GetTemperature(Position);

    string IPlayerDataProvider.Selection => blockMode ? activeBlock.Name : activeFluid.Name;

    string IPlayerDataProvider.Mode => blockMode ? Language.Block : Language.Fluid;

    /// <summary>
    ///     Teleport the player to a new position.
    /// </summary>
    /// <param name="position">The new position.</param>
    public void Teleport(Vector3d position)
    {
        PreviousPosition = Position;
        Position = position;
    }

 #pragma warning disable CA1822
    /// <summary>
    ///     Render the visual content of this player.
    /// </summary>
    public void Render()
 #pragma warning restore CA1822
    {
        // Intentionally empty, as player has no mesh to render.
        // This render method is for content that has to be rendered on every player.
    }

    /// <summary>
    ///     Render content that is specific to the local player.
    /// </summary>
    public void RenderOverlays()
    {
        if (targetPosition is {} position)
        {
            (Block selectedBlock, _) = World.GetBlock(position) ?? BlockInstance.Default;

#if DEBUG
            if (selectedBlock != Block.Air)
#else
            if (!selectedBlock.IsReplaceable)
#endif
            {
                Application.Client.Instance.Resources.Shaders.Selection.SetVector3(
                    "color",
                    new Vector3(x: 0.1f, y: 0.1f, z: 0.1f));

                visualization.DrawSelectionBox(selectedBlock.GetCollider(World, position));
            }
        }

        visualization.Draw();

        if (OverlayEnabled) visualization.DrawOverlay();
    }

    /// <inheritdoc />
    protected override void OnUpdate(double deltaTime)
    {
        movement = Vector3d.Zero;

        camera.Position = Position + cameraOffset;

        UpdateTargets();

        // Do input handling.
        if (Screen.IsFocused)
        {
            if (!Screen.IsOverlayLockActive)
            {
                HandleMovementInput(deltaTime);
                HandleLookInput();

                DoBlockFluidSelection();
                DoWorldInteraction();

                visualization.UpdateInput();
            }

            headPosition = camera.Position.Floor();

            SetBlockAndFluidOverlays();

            firstUpdate = false;
        }

        visualization.Update();
        input.Update(deltaTime);
    }

    private void SetBlockAndFluidOverlays()
    {
        Vector3d center = camera.Position;

        const double distance = 0.1;
        (double width, double height) = camera.GetDimensionsAt(distance);

        List<Vector3d> samplePoints = new()
        {
            center,
            center + camera.Up * height,
            center - camera.Up * height,
            center + camera.Right * width,
            center - camera.Right * width,
            center + camera.Front * distance,
            center - camera.Front * distance
        };

        List<Vector3i> samplePositions = new();

        foreach (Vector3d samplePoint in samplePoints)
        {
            Vector3i samplePosition = samplePoint.Floor();

            if (samplePositions.Contains(samplePosition)) continue;

            samplePositions.Add(samplePosition);
        }

        samplePositions.Sort((a, b) => Vector3d.Distance(a, center).CompareTo(Vector3d.Distance(b, center)));
        samplePositions.Reverse();

        visualization.ClearOverlay();

        foreach (Vector3d point in samplePoints)
        {
            Content? sampledContent = World.GetContent(point.Floor());

            if (sampledContent is not var (block, fluid)) continue;

            visualization.AddOverlay(block, fluid, point.Floor());
        }

        visualization.FinalizeOverlay();
    }

    private void UpdateTargets()
    {
        var ray = new Ray(camera.Position, camera.Front, length: 6f);
        (Vector3i, BlockSide)? hit = Raycast.CastBlock(World, ray);

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
        movement = input.GetMovement(speed, sprintSpeed);
        Move(movement, maxForce);

        if (!(input.ShouldJump ^ input.ShouldCrouch)) return;

        if (input.ShouldJump)
        {
            if (IsGrounded) AddForce(new Vector3d(x: 0, jumpForce, z: 0));
            else if (IsSwimming) Move(Vector3d.UnitY * swimSpeed, maxSwimForce);
        }
        else
        {
            if (IsSwimming) Move(Vector3d.UnitY * -diveSpeed, maxSwimForce);
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
            if (!blockMode || !activeBlock.IsSolid || !Collider.Intersects(
                    activeBlock.GetCollider(World, placePosition)))
            {
                if (blockMode) activeBlock.Place(World, placePosition, this);
                else activeFluid.Fill(World, placePosition, FluidLevel.One, BlockSide.Top, out _);

                input.RegisterInteraction();
            }
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
            if (blockMode) currentTarget.Block.Destroy(World, targetedPosition, this);
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

    private void DoBlockFluidSelection()
    {
        var updateData = false;

        updateData |= SelectMode();
        updateData |= SelectFromList();
        updateData |= SelectTargeted();

        if (updateData || firstUpdate) visualization.UpdateData();
    }

    private bool SelectMode()
    {
        if (!input.ShouldChangePlacementMode) return false;

        blockMode = !blockMode;

        return true;
    }

    private bool SelectFromList()
    {
        int change = input.GetSelectionChange();

        if (change == 0) return false;

        if (blockMode)
        {
            long nextBlockId = activeBlock.ID + change;
            nextBlockId = VMath.ClampRotating(nextBlockId, min: 1, Block.Count);
            activeBlock = Block.TranslateID((uint) nextBlockId);
        }
        else
        {
            long nextFluidId = activeFluid.Id + change;
            nextFluidId = VMath.ClampRotating(nextFluidId, min: 1, Fluid.Count);
            activeFluid = Fluid.TranslateID((uint) nextFluidId);
        }

        return true;
    }

    private bool SelectTargeted()
    {
        if (!input.ShouldSelectTargeted || !blockMode) return false;

        activeBlock = targetBlock?.Block ?? activeBlock;

        return true;
    }

    #region IDisposable Support

    private bool disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing) visualization.Dispose();

        disposed = true;
    }

    #endregion IDisposable Support
}
