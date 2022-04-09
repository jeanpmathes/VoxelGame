// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
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
    private Liquid activeLiquid;

    private bool blockMode = true;

    private bool firstUpdate = true;
    private Vector3i headPosition;

    private Vector3 movement;

    private BlockInstance? targetBlock;
    private LiquidInstance? targetLiquid;

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

        visualization = new PlayerVisualization(ui);
        input = new InputBehaviour(this);

        activeBlock = Block.Grass;
        activeLiquid = Liquid.Water;
    }

    /// <inheritdoc />
    public override Vector3 LookingDirection => camera.Front;

    /// <summary>
    ///     Get the looking position of the player.
    /// </summary>
    public Vector3 LookingPosition => camera.Position;

    /// <inheritdoc />
    public override BlockSide TargetSide => targetSide;

    /// <summary>
    ///     Gets the frustum of the player camera.
    /// </summary>
    public Frustum Frustum => camera.Frustum;

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

    LiquidInstance IPlayerDataProvider.TargetLiquid => targetLiquid ?? LiquidInstance.Default;

    string IPlayerDataProvider.Selection => blockMode ? activeBlock.Name : activeLiquid.Name;

    string IPlayerDataProvider.Mode => blockMode ? Language.Block : Language.Liquid;

 #pragma warning disable CA1822
    /// <summary>
    ///     Render the visual content of this player.
    /// </summary>
    public void Render()
 #pragma warning restore CA1822
    {
        // intentionally empty, as player has no mesh to render
        // this render method is for content that has to be rendered on every player
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

                DoBlockLiquidSelection();
                DoWorldInteraction();

                visualization.UpdateInput();
            }

            headPosition = camera.Position.Floor();

            (BlockInstance block, LiquidInstance liquid)? content = World.GetContent(headPosition);
            visualization.SetOverlay(content?.block.Block, content?.liquid.Liquid);

            firstUpdate = false;
        }

        visualization.Update();
        input.Update(deltaTime);
    }

    private void UpdateTargets()
    {
        var ray = new Ray(camera.Position, camera.Front, length: 6f);
        (Vector3i, BlockSide)? hit = Raycast.CastBlock(World, ray);

        if (hit is var (hitPosition, hitSide) && World.GetContent(hitPosition) is var (block, liquid))
        {
            targetPosition = hitPosition;
            targetSide = hitSide;

            (targetBlock, targetLiquid) = (block, liquid);
        }
        else
        {
            (targetBlock, targetLiquid) = (null, null);
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
        if (targetBlock == null || targetLiquid == null) return;

        PlaceInteract();
        DestroyInteract();
    }

    private void PlaceInteract()
    {
        Debug.Assert(targetBlock != null);
        Debug.Assert(targetLiquid != null);

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
                else activeLiquid.Fill(World, placePosition, LiquidLevel.One, BlockSide.Top, out _);

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
        Debug.Assert(targetLiquid != null);

        BlockInstance currentTarget = targetBlock.Value;

        if (input.ShouldDestroy)
        {
            if (blockMode) currentTarget.Block.Destroy(World, targetPosition, this);
            else TakeLiquid(targetPosition);

            input.RegisterInteraction();
        }

        void TakeLiquid(Vector3i position)
        {
            var level = LiquidLevel.One;

            if (!currentTarget.Block.IsReplaceable)
                position = targetSide.Offset(position);

            World.GetLiquid(position)?.Liquid.Take(World, position, ref level);
        }
    }

    private void DoBlockLiquidSelection()
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
            long nextLiquidId = activeLiquid.Id + change;
            nextLiquidId = VMath.ClampRotating(nextLiquidId, min: 1, Liquid.Count);
            activeLiquid = Liquid.TranslateID((uint) nextLiquidId);
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
