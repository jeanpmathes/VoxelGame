// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Composite;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed class ClientPlayer : Player, IPlayerDataProvider
{
    private readonly Camera camera;
    private readonly Vector3 cameraOffset = new(x: 0f, y: 0.65f, z: 0f);

    private readonly InputBehaviour input;

    private readonly float jumpForce = 25000f;

    private readonly Vector3 maxForce = new(x: 500f, y: 0f, z: 500f);
    private readonly Vector3 maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

    private readonly float speed = 4f;
    private readonly float sprintSpeed = 6f;
    private readonly float swimSpeed = 4f;

    private readonly PlayerVisualization visualization;

    private Block activeBlock;
    private Fluid activeFluid;

    private bool blockMode = true;

    private bool firstUpdate = true;
    private Vector3i headPosition;

    private Vector3 movement;

    private BlockInstance? targetBlock;
    private FluidInstance? targetFluid;

    private Vector3i targetPosition = new(x: 0, y: -1, z: 0);
    private BlockSide targetSide;

    /// <summary>
    ///     Create a client player.
    /// </summary>
    /// <param name="world">The world in which the client player will be placed.</param>
    /// <param name="mass">The mass of the player.</param>
    /// <param name="drag">The drag affecting the player.</param>
    /// <param name="camera">The camera to use for this player.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    /// <param name="ui">The ui used to display player information.</param>
    public ClientPlayer(World world, float mass, float drag, Camera camera, BoundingVolume boundingVolume,
        GameUserInterface ui) : base(world, mass, drag, boundingVolume)
    {
        this.camera = camera;
        camera.Position = Position;

        visualization = new PlayerVisualization(this, ui);
        input = new InputBehaviour(this);

        activeBlock = Block.Grass;
        activeFluid = Fluid.Water;
    }

    /// <inheritdoc />
    public override Vector3 LookingDirection => camera.Front;

    /// <summary>
    ///     Get the looking position of the player, meaning the position of the camera.
    /// </summary>
    public Vector3 LookingPosition => camera.Position;

    /// <inheritdoc />
    public override BlockSide TargetSide => targetSide;

    /// <summary>
    ///     Gets the frustum of the player camera.
    /// </summary>
    public Frustum Frustum => camera.Frustum;

    /// <summary>
    ///     Get the dimensions of the near view plane.
    /// </summary>
    public (Vector3 a, Vector3 b) NearDimensions
    {
        get
        {
            (float width, float height) = camera.GetDimensionsAt(Camera.NearClipping);

            Vector3 position = camera.Position + camera.Front * Camera.NearClipping;

            Vector3 up = camera.Up * height * 0.5f;
            Vector3 right = camera.Right * width * 0.5f;

            return (position - up - right, position + up + right);
        }
    }

    /// <inheritdoc />
    public override Vector3 Movement => movement;

    /// <summary>
    ///     Gets the view matrix of the camera of this player.
    /// </summary>
    public Matrix4 ViewMatrix => camera.ViewMatrix;

    /// <summary>
    ///     Gets the projection matrix of the camera of this player.
    /// </summary>
    public Matrix4 ProjectionMatrix => camera.ProjectionMatrix;

    /// <inheritdoc cref="PhysicsEntity" />
    public override Vector3i TargetPosition => targetPosition;

    Vector3i IPlayerDataProvider.HeadPosition => headPosition;

    BlockInstance IPlayerDataProvider.TargetBlock => targetBlock ?? BlockInstance.Default;

    FluidInstance IPlayerDataProvider.TargetFluid => targetFluid ?? FluidInstance.Default;

    string IPlayerDataProvider.Selection => blockMode ? activeBlock.Name : activeFluid.Name;

    string IPlayerDataProvider.Mode => blockMode ? Language.Block : Language.Fluid;

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
        visualization.Draw();

        if (targetPosition.Y >= 0)
        {
            (Block selectedBlock, _) = World.GetBlock(targetPosition) ?? BlockInstance.Default;

#if DEBUG
            if (selectedBlock != Block.Air)
#else
            if (!selectedBlock.IsReplaceable)
#endif
            {
                Application.Client.Instance.Resources.Shaders.Selection.SetVector3(
                    "color",
                    new Vector3(x: 0.1f, y: 0.1f, z: 0.1f));

                visualization.DrawSelectionBox(selectedBlock.GetCollider(World, targetPosition));
            }
        }

        visualization.DrawOverlay();
    }

    /// <inheritdoc />
    protected override void OnUpdate(float deltaTime)
    {
        movement = Vector3.Zero;

        camera.Position = Position + cameraOffset;

        UpdateTargets();

        // Do input handling.
        if (Screen.IsFocused)
        {
            if (!Screen.IsOverlayLockActive)
            {
                HandleMovementInput();
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
        Vector3 center = camera.Position;

        const float distance = 0.1f;
        (float width, float height) = camera.GetDimensionsAt(distance);

        List<Vector3> samplePoints = new()
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

        foreach (Vector3 samplePoint in samplePoints)
        {
            Vector3i samplePosition = samplePoint.Floor();

            if (samplePositions.Contains(samplePosition)) continue;

            samplePositions.Add(samplePosition);
        }

        samplePositions.Sort((a, b) => Vector3.Distance(a, center).CompareTo(Vector3.Distance(b, center)));
        samplePositions.Reverse();

        visualization.ClearOverlay();

        foreach (Vector3 point in samplePoints)
        {
            (BlockInstance block, FluidInstance fluid)? sampledContent = World.GetContent(point.Floor());

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
            (targetBlock, targetFluid) = (null, null);
        }
    }

    private void HandleMovementInput()
    {
        Move(input.GetMovement(speed, sprintSpeed), maxForce);

        if (input.ShouldJump)
        {
            if (IsGrounded) AddForce(new Vector3(x: 0f, jumpForce, z: 0f));
            else if (IsSwimming) Move(Vector3.UnitY * swimSpeed, maxSwimForce);
        }
    }

    private void HandleLookInput()
    {
        // Apply the camera pitch and yaw (the pitch is clamped in the camera class)
        (float yaw, float pitch) = Application.Client.Instance.Keybinds.LookBind.Value;
        camera.Yaw += yaw;
        camera.Pitch += pitch;

        Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
    }

    private void DoWorldInteraction()
    {
        if (targetBlock == null || targetFluid == null) return;

        PlaceInteract();
        DestroyInteract();
    }

    private void PlaceInteract()
    {
        Debug.Assert(targetBlock != null);
        Debug.Assert(targetFluid != null);

        BlockInstance currentTarget = targetBlock.Value;

        if (!input.ShouldInteract) return;

        Vector3i placePosition = targetPosition;

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
            currentTarget.Block.EntityInteract(this, targetPosition);

            input.RegisterInteraction();
        }
    }

    private void DestroyInteract()
    {
        Debug.Assert(targetBlock != null);
        Debug.Assert(targetFluid != null);

        BlockInstance currentTarget = targetBlock.Value;

        if (input.ShouldDestroy)
        {
            if (blockMode) currentTarget.Block.Destroy(World, targetPosition, this);
            else TakeFluid(targetPosition);

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
            long nextBlockId = activeBlock.Id + change;
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

    private sealed class InputBehaviour
    {
        private readonly Button blockInteractButton;
        private readonly Button destroyButton;

        private readonly float interactionCooldown = 0.25f;

        private readonly Button interactOrPlaceButton;
        private readonly Button jumpButton;
        private readonly InputAxis2 movementInput;

        private readonly ToggleButton placementModeToggle;

        private readonly PhysicsEntity player;
        private readonly InputAxis selectionAxis;
        private readonly PushButton selectTargetedButton;
        private readonly Button sprintButton;

        private float timer;

        public InputBehaviour(PhysicsEntity player)
        {
            this.player = player;

            KeybindManager keybind = Application.Client.Instance.Keybinds;

            Button forwardsButton = keybind.GetButton(keybind.Forwards);
            Button backwardsButton = keybind.GetButton(keybind.Backwards);
            Button strafeRightButton = keybind.GetButton(keybind.StrafeRight);
            Button strafeLeftButton = keybind.GetButton(keybind.StrafeLeft);

            movementInput = new InputAxis2(
                new InputAxis(forwardsButton, backwardsButton),
                new InputAxis(strafeRightButton, strafeLeftButton));

            sprintButton = keybind.GetButton(keybind.Sprint);
            jumpButton = keybind.GetButton(keybind.Jump);

            interactOrPlaceButton = keybind.GetButton(keybind.InteractOrPlace);
            destroyButton = keybind.GetButton(keybind.Destroy);
            blockInteractButton = keybind.GetButton(keybind.BlockInteract);

            placementModeToggle = keybind.GetToggle(keybind.PlacementMode);
            placementModeToggle.Clear();

            selectTargetedButton = keybind.GetPushButton(keybind.SelectTargeted);

            Button nextButton = keybind.GetPushButton(keybind.NextPlacement);
            Button previousButton = keybind.GetPushButton(keybind.PreviousPlacement);
            selectionAxis = new InputAxis(nextButton, previousButton);
        }

        public bool ShouldJump => jumpButton.IsDown;

        private bool IsCooldownOver => timer >= interactionCooldown;

        public bool ShouldInteract => IsCooldownOver && interactOrPlaceButton.IsDown;

        public bool ShouldDestroy => IsCooldownOver && destroyButton.IsDown;

        public bool ShouldChangePlacementMode => placementModeToggle.Changed;

        public bool ShouldSelectTargeted => selectTargetedButton.IsDown;

        public bool IsInteractionBlocked => blockInteractButton.IsDown;

        public Vector3 GetMovement(float normalSpeed, float sprintSpeed)
        {
            (float x, float z) = movementInput.Value;
            Vector3 movement = x * player.Forward + z * player.Right;

            if (movement != Vector3.Zero)
                movement = sprintButton.IsDown
                    ? movement.Normalized() * sprintSpeed
                    : movement.Normalized() * normalSpeed;

            return movement;
        }

        public void Update(float deltaTime)
        {
            timer += deltaTime;
        }

        public void RegisterInteraction()
        {
            timer = 0;
        }

        public int GetSelectionChange()
        {
            return Math.Sign(selectionAxis.Value);
        }
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
