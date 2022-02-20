﻿// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using System.Drawing;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Objects;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Composite;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities
{
    /// <summary>
    ///     The client player, controlled by the user. There can only be one client player.
    /// </summary>
    public class ClientPlayer : Player, IPlayerDataProvider
    {
        private readonly Camera camera;
        private readonly Vector3 cameraOffset = new(x: 0f, y: 0.65f, z: 0f);

        private readonly Texture crosshair;

        private readonly Vector2 crosshairPosition = new(x: 0.5f, y: 0.5f);
        private readonly ScreenElementRenderer crosshairRenderer;

        private readonly float interactionCooldown = 0.25f;
        private readonly float jumpForce = 25000f;

        private readonly Vector3 maxForce = new(x: 500f, y: 0f, z: 500f);
        private readonly Vector3 maxSwimForce = new(x: 0f, y: 2500f, z: 0f);

        private readonly OverlayRenderer overlay;

        private readonly BoxRenderer selectionRenderer;

        private readonly float speed = 4f;
        private readonly float sprintSpeed = 6f;
        private readonly float swimSpeed = 4f;

        private readonly GameUserInterface ui;

        private Block activeBlock;
        private Liquid activeLiquid;

        private bool blockMode = true;
        private float crosshairScale = Application.Client.Instance.Settings.CrosshairScale;

        private bool firstUpdate = true;
        private Vector3i headPosition;

        private Vector3 movement;

        private bool renderOverlay;
        private BlockInstance? targetBlock;
        private LiquidInstance? targetLiquid;

        private Vector3i targetPosition = new(x: 0, y: -1, z: 0);
        private BlockSide targetSide;

        private float timer;

        /// <summary>
        ///     Create a client player.
        /// </summary>
        /// <param name="world">The world in which the client player will be placed.</param>
        /// <param name="mass">The mass of the player.</param>
        /// <param name="drag">The drag affecting the player.</param>
        /// <param name="camera">The camera to use for this player.</param>
        /// <param name="boundingBox">The bounding box of the player.</param>
        /// <param name="ui">The ui used to display player information.</param>
        public ClientPlayer(World world, float mass, float drag, Camera camera, BoundingBox boundingBox,
            GameUserInterface ui) : base(world, mass, drag, boundingBox)
        {
            this.camera = camera;
            camera.Position = Position;

            selectionRenderer = new BoxRenderer();

            overlay = new OverlayRenderer();

            crosshair = new Texture(
                "Resources/Textures/UI/crosshair.png",
                TextureUnit.Texture10,
                fallbackResolution: 32);

            crosshairRenderer = new ScreenElementRenderer();
            crosshairRenderer.SetTexture(crosshair);
            crosshairRenderer.SetColor(Application.Client.Instance.Settings.CrosshairColor.ToVector3());

            Application.Client.Instance.Settings.CrosshairColorChanged += UpdateCrosshairColor;
            Application.Client.Instance.Settings.CrosshairScaleChanged += SettingsOnCrosshairScaleChanged;

            activeBlock = Block.Grass;
            activeLiquid = Liquid.Water;

            this.ui = ui;

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

            Button nextButton = keybind.GetPushButton(keybind.NextPlacement);
            Button previousButton = keybind.GetPushButton(keybind.PreviousPlacement);
            selectionAxis = new InputAxis(nextButton, previousButton);

            debugViewButton = keybind.GetPushButton(keybind.DebugView);
        }

        /// <inheritdoc />
        public override Vector3 LookingDirection => camera.Front;

        /// <inheritdoc />
        public override BlockSide TargetSide => targetSide;

        /// <summary>
        ///     Gets the frustum of the player camera.
        /// </summary>
        public Frustum Frustum => camera.Frustum;

        /// <inheritdoc />
        public override Vector3 Movement => movement;

        /// <inheritdoc cref="PhysicsEntity" />
        public override Vector3i TargetPosition => targetPosition;

        Vector3i IPlayerDataProvider.HeadPosition => headPosition;

        BlockInstance IPlayerDataProvider.TargetBlock => targetBlock ?? BlockInstance.Default;

        LiquidInstance IPlayerDataProvider.TargetLiquid => targetLiquid ?? LiquidInstance.Default;

        string IPlayerDataProvider.Selection => blockMode ? activeBlock.Name : activeLiquid.Name;

        string IPlayerDataProvider.Mode => blockMode ? Language.Block : Language.Liquid;

        private void UpdateCrosshairColor(GeneralSettings settings, SettingChangedArgs<Color> args)
        {
            crosshairRenderer.SetColor(settings.CrosshairColor.ToVector3());
        }

        private void SettingsOnCrosshairScaleChanged(GeneralSettings settings, SettingChangedArgs<float> args)
        {
            crosshairScale = args.NewValue;
        }

        /// <summary>
        ///     Gets the view matrix of the camera of this player.
        /// </summary>
        /// <returns>The view matrix.</returns>
        public Matrix4 GetViewMatrix()
        {
            return camera.GetViewMatrix();
        }

        /// <summary>
        ///     Gets the projection matrix of the camera of this player.
        /// </summary>
        /// <returns>The projection matrix.</returns>
        public Matrix4 GetProjectionMatrix()
        {
            return camera.GetProjectionMatrix();
        }

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
            crosshairRenderer.Draw(crosshairPosition, crosshairScale);

            if (targetPosition.Y >= 0)
            {
                var (selectedBlock, _) = World.GetBlock(targetPosition) ?? BlockInstance.Default;

#if DEBUG
                if (selectedBlock != Block.Air)
#else
                if (!selectedBlock.IsReplaceable)
#endif
                {
                    BoundingBox selectedBox = selectedBlock.GetBoundingBox(World, targetPosition);

                    Shaders.Selection.SetVector3("color", new Vector3(x: 0.1f, y: 0.1f, z: 0.1f));

                    selectionRenderer.SetBoundingBox(selectedBox);
                    selectionRenderer.Draw(selectedBox.Center);
                }
            }

            if (renderOverlay) overlay.Draw();
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

                    BlockLiquidSelection();
                    DoWorldInteraction();

                    SelectDebugView();
                }

                headPosition = camera.Position.Floor();

                if (World.GetBlock(headPosition)?.Block is IOverlayTextureProvider overlayBlockTextureProvider)
                {
                    overlay.SetBlockTexture(overlayBlockTextureProvider.TextureIdentifier);
                    renderOverlay = true;
                }
                else if (World.GetLiquid(headPosition)?.Liquid is IOverlayTextureProvider overlayLiquidTextureProvider)
                {
                    overlay.SetLiquidTexture(overlayLiquidTextureProvider.TextureIdentifier);
                    renderOverlay = true;
                }
                else
                {
                    renderOverlay = false;
                }

                firstUpdate = false;
            }

            ui.UpdatePlayerDebugData();

            timer += deltaTime;
        }

        private void SelectDebugView()
        {
            if (debugViewButton.IsDown) ui.ToggleDebugDataView();
        }

        private void UpdateTargets()
        {
            var ray = new Ray(camera.Position, camera.Front, length: 6f);
            bool hit = Raycast.CastBlock(World, ray, out targetPosition, out targetSide);

            if (hit) (targetBlock, targetLiquid) = World.GetContent(targetPosition);
            else (targetBlock, targetLiquid) = (null, null);
        }

        private void HandleMovementInput()
        {
            (float x, float z) = movementInput.Value;
            movement = x * Forward + z * Right;

            if (movement != Vector3.Zero)
            {
                if (sprintButton.IsDown) movement = movement.Normalized() * sprintSpeed;
                else movement = movement.Normalized() * speed;
            }

            Move(movement, maxForce);

            if (jumpButton.IsDown)
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

            if (timer < interactionCooldown || interactOrPlaceButton.IsUp) return;

            Vector3i placePosition = targetPosition;

            if (blockInteractButton.IsDown || !targetBlock.Block.IsInteractable)
            {
                if (!targetBlock.Block.IsReplaceable) placePosition = targetSide.Offset(placePosition);

                // Prevent block placement if the block would intersect the player.
                if (!blockMode || !activeBlock.IsSolid || !BoundingBox.Intersects(
                        activeBlock.GetBoundingBox(World, placePosition)))
                {
                    if (blockMode) activeBlock.Place(World, placePosition, this);
                    else activeLiquid.Fill(World, placePosition, LiquidLevel.One, BlockSide.Top, out _);

                    timer = 0;
                }
            }
            else if (targetBlock.Block.IsInteractable)
            {
                targetBlock.Block.EntityInteract(this, targetPosition);

                timer = 0;
            }
        }

        private void DestroyInteract()
        {
            Debug.Assert(targetBlock != null);
            Debug.Assert(targetLiquid != null);

            if (timer >= interactionCooldown && destroyButton.IsDown)
            {
                if (blockMode) targetBlock.Block.Destroy(World, targetPosition, this);
                else TakeLiquid(targetPosition);

                timer = 0;
            }

            void TakeLiquid(Vector3i position)
            {
                var level = LiquidLevel.One;

                if (!targetBlock.Block.IsReplaceable)
                    position = targetSide.Offset(position);

                World.GetLiquid(position)?.Liquid.Take(World, position, ref level);
            }
        }

        private void BlockLiquidSelection()
        {
            var updateUI = false;

            if (placementModeToggle.Changed)
            {
                blockMode = !blockMode;
                updateUI = true;
            }

            if (!VMath.NearlyZero(selectionAxis.Value))
            {
                int change = selectionAxis.Value > 0 ? 1 : -1;

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

                updateUI = true;
            }

            if (updateUI || firstUpdate) ui.UpdatePlayerData();
        }

        #region INPUT ACTIONS

        private readonly InputAxis2 movementInput;
        private readonly Button sprintButton;
        private readonly Button jumpButton;

        private readonly Button interactOrPlaceButton;
        private readonly Button destroyButton;
        private readonly Button blockInteractButton;

        private readonly ToggleButton placementModeToggle;
        private readonly InputAxis selectionAxis;

        private readonly Button debugViewButton;

        #endregion INPUT ACTIONS

        #region IDisposable Support

        private bool disposed;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                crosshair.Dispose();

                selectionRenderer.Dispose();
                crosshairRenderer.Dispose();
                overlay.Dispose();

                Application.Client.Instance.Settings.CrosshairColorChanged -= UpdateCrosshairColor;
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}
