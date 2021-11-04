// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Entities
{
    public class ClientPlayer : Player
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

        private Vector3 movement;

        private bool renderOverlay;

        private Vector3i selectedPosition = new(x: 0, y: -1, z: 0);
        private BlockSide selectedSide;

        private float timer;

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
        }

        private bool IsInputLocked { get; set; }

        public override Vector3 LookingDirection => camera.Front;

        public override BlockSide TargetSide => selectedSide;

        /// <summary>
        ///     Gets the frustum of the player camera.
        /// </summary>
        public Frustum Frustum => camera.Frustum;

        public override Vector3 Movement => movement;

        public void LockInput()
        {
            IsInputLocked = true;
        }

        public void UnlockInput()
        {
            IsInputLocked = false;
        }

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

        public void Render()
        {
            if (selectedPosition.Y >= 0)
            {
                Block selectedBlock = World.GetBlock(selectedPosition, out _) ?? Block.Air;

#if DEBUG
                if (selectedBlock != Block.Air)
#else
                if (!selectedBlock.IsReplaceable)
#endif
                {
                    BoundingBox selectedBox = selectedBlock.GetBoundingBox(World, selectedPosition);

                    Shaders.Selection.SetVector3("color", new Vector3(x: 0.1f, y: 0.1f, z: 0.1f));

                    selectionRenderer.SetBoundingBox(selectedBox);
                    selectionRenderer.Draw(selectedBox.Center);
                }
            }

            if (renderOverlay) overlay.Draw();

            crosshairRenderer.Draw(crosshairPosition, crosshairScale);
        }

        protected override void OnUpdate(float deltaTime)
        {
            movement = Vector3.Zero;

            camera.Position = Position + cameraOffset;

            var ray = new Ray(camera.Position, camera.Front, length: 6f);
            Raycast.CastBlock(World, ray, out selectedPosition, out selectedSide);

            // Do input handling.
            if (Screen.IsFocused)
            {
                if (!IsInputLocked)
                {
                    HandleMovementInput();
                    HandleLookInput();
                }

                BlockLiquidSelection(firstUpdate);
                WorldInteraction();

                Vector3i headPosition = camera.Position.Floor();

                if (World.GetBlock(headPosition, out _) is IOverlayTextureProvider overlayBlockTextureProvider)
                {
                    overlay.SetBlockTexture(overlayBlockTextureProvider.TextureIdentifier);
                    renderOverlay = true;
                }
                else if (World.GetLiquid(headPosition, out _, out _) is IOverlayTextureProvider
                    overlayLiquidTextureProvider)
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

            timer += deltaTime;
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

        private void WorldInteraction()
        {
            if (IsInputLocked) return;

            Block? target = World.GetBlock(selectedPosition, out _);

            if (target == null) return;

            PlaceInteract(target);
            DestroyInteract(target);
        }

        private void PlaceInteract(Block target)
        {
            if (timer < interactionCooldown || interactOrPlaceButton.IsUp) return;

            Vector3i placePosition = selectedPosition;

            if (blockInteractButton.IsDown || !target.IsInteractable)
            {
                if (!target.IsReplaceable) placePosition = selectedSide.Offset(placePosition);

                // Prevent block placement if the block would intersect the player.
                if (!blockMode || !activeBlock.IsSolid || !BoundingBox.Intersects(
                    activeBlock.GetBoundingBox(World, placePosition)))
                {
                    if (blockMode) activeBlock.Place(World, placePosition, this);
                    else activeLiquid.Fill(World, placePosition, LiquidLevel.One, BlockSide.Top, out _);

                    timer = 0;
                }
            }
            else if (target.IsInteractable)
            {
                target.EntityInteract(this, selectedPosition);

                timer = 0;
            }
        }

        private void DestroyInteract(Block target)
        {
            if (timer >= interactionCooldown && destroyButton.IsDown)
            {
                if (blockMode) target.Destroy(World, selectedPosition, this);
                else TakeLiquid(selectedPosition);

                timer = 0;
            }

            void TakeLiquid(Vector3i position)
            {
                var level = LiquidLevel.One;

                if (!target.IsReplaceable)
                    position = selectedSide.Offset(position);

                World.GetLiquid(position, out _, out _)?.Take(World, position, ref level);
            }
        }

        private void BlockLiquidSelection(bool updateUI)
        {
            if (placementModeToggle.Changed)
            {
                blockMode = !blockMode;
                updateUI = true;
            }

            if (selectionAxis.Value != 0)
            {
                if (selectionAxis.Value > 0)
                {
                    if (blockMode)
                        activeBlock = activeBlock.Id != Block.Count - 1
                            ? Block.TranslateID(activeBlock.Id + 1)
                            : Block.TranslateID(id: 1);
                    else
                        activeLiquid = activeLiquid.Id != Liquid.Count - 1
                            ? Liquid.TranslateID(activeLiquid.Id + 1)
                            : Liquid.TranslateID(id: 1);

                    updateUI = true;
                }

                if (selectionAxis.Value < 0)
                {
                    if (blockMode)
                        activeBlock = activeBlock.Id != 1
                            ? Block.TranslateID(activeBlock.Id - 1)
                            : Block.TranslateID((uint) (Block.Count - 1));
                    else
                        activeLiquid = activeLiquid.Id != 1
                            ? Liquid.TranslateID(activeLiquid.Id - 1)
                            : Liquid.TranslateID((uint) (Liquid.Count - 1));

                    updateUI = true;
                }
            }

            if (updateUI)
            {
                if (blockMode) ui.SetPlayerSelection(Language.Block, activeBlock.Name);
                else ui.SetPlayerSelection(Language.Liquid, activeLiquid.Name);
            }
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

        #endregion INPUT ACTIONS

        #region IDisposable Support

        private bool disposed;

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