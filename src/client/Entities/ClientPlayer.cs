// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;
using System;
using VoxelGame.Client.Rendering;
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
    public class ClientPlayer : Core.Entities.Player
    {
        public override Vector3 LookingDirection { get => camera.Front; }
        public override BlockSide TargetSide { get => selectedSide; }

        private readonly Camera camera;
        private readonly Vector3 cameraOffset = new Vector3(0f, 0.65f, 0f);

        private readonly float mouseSensitivity = Properties.client.Default.MouseSensitivity;

        private readonly GameUserInterface ui;

        private readonly Axis2 movementInput;
        private readonly Button sprintButton;
        private readonly Button jumpButton;

        private readonly Button interactOrPlaceButton;
        private readonly Button destroyButton;
        private readonly Button blockInteractButton;

        private readonly Toggle placementModeToggle;
        private readonly PushButton previousButton;
        private readonly PushButton nextButton;

        public ClientPlayer(World world, float mass, float drag, Camera camera, BoundingBox boundingBox, GameUserInterface ui) : base(world, mass, drag, boundingBox)
        {
            this.camera = camera;
            camera.Position = Position;

            selectionRenderer = new BoxRenderer();

            overlay = new OverlayRenderer();

            crosshair = new Texture("Resources/Textures/UI/crosshair.png", OpenToolkit.Graphics.OpenGL4.TextureUnit.Texture10, fallbackResolution: 32);

            crosshairRenderer = new ScreenElementRenderer();
            crosshairRenderer.SetTexture(crosshair);
            crosshairRenderer.SetColor(crosshairColor);

            activeBlock = Block.Grass;
            activeLiquid = Liquid.Water;

            this.ui = ui;

            Button forwardsButton = Application.Client.Instance.Keybinds.GetButton("forwards", Key.W);
            Button backwardsButton = Application.Client.Instance.Keybinds.GetButton("backwards", Key.S);
            Button strafeRightButton = Application.Client.Instance.Keybinds.GetButton("strafe_right", Key.D);
            Button strafeLeftButton = Application.Client.Instance.Keybinds.GetButton("strafe_left", Key.A);

            movementInput = new Axis2(
                new Axis(forwardsButton, backwardsButton),
                new Axis(strafeRightButton, strafeLeftButton));

            sprintButton = Application.Client.Instance.Keybinds.GetButton("sprint", Key.ShiftLeft);

            jumpButton = Application.Client.Instance.Keybinds.GetButton("jump", Key.Space);

            interactOrPlaceButton = Application.Client.Instance.Keybinds.GetButton("interact_or_place", MouseButton.Right);
            destroyButton = Application.Client.Instance.Keybinds.GetButton("destroy", MouseButton.Left);
            blockInteractButton = Application.Client.Instance.Keybinds.GetButton("strafe_left", Key.ControlLeft);

            placementModeToggle = Application.Client.Instance.Keybinds.GetToggle("placement_mode", Key.R);
            nextButton = Application.Client.Instance.Keybinds.GetPushButton("select_next_placement", Key.KeypadPlus);
            previousButton = Application.Client.Instance.Keybinds.GetPushButton("select_previous_placement", Key.KeypadMinus);
        }

        /// <summary>
        /// Gets the view matrix of the camera of this player.
        /// </summary>
        /// <returns>The view matrix.</returns>
        public Matrix4 GetViewMatrix()
        {
            return camera.GetViewMatrix();
        }

        /// <summary>
        /// Gets the projection matrix of the camera of this player.
        /// </summary>
        /// <returns>The projection matrix.</returns>
        public Matrix4 GetProjectionMatrix()
        {
            return camera.GetProjectionMatrix();
        }

        /// <summary>
        /// Gets the frustum of the player camera.
        /// </summary>
        public Frustum Frustum { get => camera.Frustum; }

        public override Vector3 Movement { get => movement; }

        private readonly BoxRenderer selectionRenderer;

        private readonly OverlayRenderer overlay;

        private bool renderOverlay;

        private readonly Texture crosshair;
        private readonly ScreenElementRenderer crosshairRenderer;

        private readonly Vector2 crosshairPosition = new Vector2(0.5f, 0.5f);
        private readonly float crosshairScale = Properties.client.Default.CrosshairScale;
        private readonly Vector3 crosshairColor = Properties.client.Default.CrosshairColor.ToVector3();

        public void Render()
        {
            if (selectedY >= 0)
            {
                Block selectedBlock = World.GetBlock(selectedX, selectedY, selectedZ, out _) ?? Block.Air;

#if DEBUG
                if (selectedBlock != Block.Air)
#else
                if (!selectedBlock.IsReplaceable)
#endif
                {
                    BoundingBox selectedBox = selectedBlock.GetBoundingBox(World, selectedX, selectedY, selectedZ);

                    Shaders.Selection.SetVector3("color", new Vector3(0.1f, 0.1f, 0.1f));

                    selectionRenderer.SetBoundingBox(selectedBox);
                    selectionRenderer.Draw(selectedBox.Center);
                }
            }

            if (renderOverlay)
            {
                overlay.Draw();
            }

            crosshairRenderer.Draw(crosshairPosition, crosshairScale);
        }

        private Vector3 movement;

        private bool firstUpdate = true;

        protected override void OnUpdate(float deltaTime)
        {
            movement = Vector3.Zero;

            camera.Position = Position + cameraOffset;

            var ray = new Ray(camera.Position, camera.Front, 6f);
            Raycast.CastBlock(World, ray, out selectedX, out selectedY, out selectedZ, out selectedSide);

            // Do input handling.
            if (Screen.IsFocused)
            {
                MovementInput();
                MouseChange();

                BlockLiquidSelection(firstUpdate);

                WorldInteraction();

                int headX = (int) Math.Floor(camera.Position.X);
                int headY = (int) Math.Floor(camera.Position.Y);
                int headZ = (int) Math.Floor(camera.Position.Z);

                if (World.GetBlock(headX, headY, headZ, out _) is IOverlayTextureProvider overlayBlockTextureProvider)
                {
                    overlay.SetBlockTexture(overlayBlockTextureProvider.TextureIdentifier);
                    renderOverlay = true;
                }
                else if (World.GetLiquid(headX, headY, headZ, out _, out _) is IOverlayTextureProvider overlayLiquidTextureProvider)
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

        private readonly float speed = 4f;
        private readonly float sprintSpeed = 6f;
        private readonly Vector3 maxForce = new Vector3(500f, 0f, 500f);
        private readonly float swimSpeed = 4f;
        private readonly Vector3 maxSwimForce = new Vector3(0f, 2500f, 0f);
        private readonly float jumpForce = 25000f;

        private void MovementInput()
        {
            (float x, float z) = movementInput.Value;
            movement = (x * Forward) + (z * Right);

            if (movement != Vector3.Zero)
            {
                if (sprintButton.IsDown)
                {
                    movement = movement.Normalized() * sprintSpeed;
                }
                else
                {
                    movement = movement.Normalized() * speed;
                }
            }

            Move(movement, maxForce);

            if (jumpButton.IsDown)
            {
                if (IsGrounded)
                {
                    AddForce(new Vector3(0f, jumpForce, 0f));
                }
                else if (IsSwimming)
                {
                    Move(Vector3.UnitY * swimSpeed, maxSwimForce);
                }
            }
        }

        private void MouseChange()
        {
            // Apply the camera pitch and yaw (the pitch is clamped in the camera class)
            camera.Yaw += Application.Client.Instance.Mouse.Delta.X * mouseSensitivity;
            camera.Pitch += Application.Client.Instance.Mouse.Delta.Y * mouseSensitivity;

            Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
        }

        private readonly float interactionCooldown = 0.25f;

        private int selectedX, selectedY = -1, selectedZ;
        private BlockSide selectedSide;

        private float timer;

        private void WorldInteraction()
        {
            Block? target = World.GetBlock(selectedX, selectedY, selectedZ, out _);

            if (target == null)
            {
                return;
            }

            PlaceInteract(target);
            DestroyInteract(target);
        }

        private void PlaceInteract(Block target)
        {
            if (timer < interactionCooldown || interactOrPlaceButton.IsUp) return;

            int placePositionX = selectedX;
            int placePositionY = selectedY;
            int placePositionZ = selectedZ;

            if (blockInteractButton.IsDown || !target.IsInteractable)
            {
                if (!target.IsReplaceable)
                {
                    OffsetSelection();
                }

                // Prevent block placement if the block would intersect the player.
                if (!blockMode || !activeBlock.IsSolid || !BoundingBox.Intersects(activeBlock.GetBoundingBox(World, placePositionX, placePositionY, placePositionZ)))
                {
                    if (blockMode) activeBlock.Place(World, placePositionX, placePositionY, placePositionZ, this);
                    else activeLiquid.Fill(World, placePositionX, placePositionY, placePositionZ, LiquidLevel.One, out _);

                    timer = 0;
                }
            }
            else if (target.IsInteractable)
            {
                target.EntityInteract(this, selectedX, selectedY, selectedZ);

                timer = 0;
            }

            void OffsetSelection()
            {
                switch (selectedSide)
                {
                    case BlockSide.Front:
                        placePositionZ++;
                        break;

                    case BlockSide.Back:
                        placePositionZ--;
                        break;

                    case BlockSide.Left:
                        placePositionX--;
                        break;

                    case BlockSide.Right:
                        placePositionX++;
                        break;

                    case BlockSide.Bottom:
                        placePositionY--;
                        break;

                    case BlockSide.Top:
                        placePositionY++;
                        break;
                }
            }
        }

        private void DestroyInteract(Block target)
        {
            if (timer >= interactionCooldown && destroyButton.IsDown)
            {
                if (blockMode) target.Destroy(World, selectedX, selectedY, selectedZ, this);
                else TakeLiquid(selectedX, selectedY, selectedZ);

                timer = 0;
            }

            void TakeLiquid(int x, int y, int z)
            {
                LiquidLevel level = LiquidLevel.One;

                if (!target.IsReplaceable)
                {
                    switch (selectedSide)
                    {
                        case BlockSide.Front:
                            z++;
                            break;

                        case BlockSide.Back:
                            z--;
                            break;

                        case BlockSide.Left:
                            x--;
                            break;

                        case BlockSide.Right:
                            x++;
                            break;

                        case BlockSide.Bottom:
                            y--;
                            break;

                        case BlockSide.Top:
                            y++;
                            break;
                    }
                }

                World.GetLiquid(x, y, z, out _, out _)?.Take(World, x, y, z, ref level);
            }
        }

        private Block activeBlock;
        private Liquid activeLiquid;

        private bool blockMode = true;

        private void BlockLiquidSelection(bool updateUI)
        {
            if (placementModeToggle.Changed)
            {
                blockMode = !blockMode;
                updateUI = true;
            }

            if (nextButton.Pushed)
            {
                if (blockMode) activeBlock = (activeBlock.Id != Block.Count - 1) ? Block.TranslateID(activeBlock.Id + 1) : Block.TranslateID(1);
                else activeLiquid = (activeLiquid.Id != Liquid.Count - 1) ? Liquid.TranslateID(activeLiquid.Id + 1) : Liquid.TranslateID(1);

                updateUI = true;
            }

            if (previousButton.Pushed)
            {
                if (blockMode) activeBlock = (activeBlock.Id != 1) ? Block.TranslateID(activeBlock.Id - 1) : Block.TranslateID((uint) (Block.Count - 1));
                else activeLiquid = (activeLiquid.Id != 1) ? Liquid.TranslateID(activeLiquid.Id - 1) : Liquid.TranslateID((uint) (Liquid.Count - 1));

                updateUI = true;
            }

            if (updateUI)
            {
                if (blockMode)
                {
                    ui.SetPlayerSelection(Language.Block, activeBlock.Name);
                }
                else
                {
                    ui.SetPlayerSelection(Language.Liquid, activeLiquid.Name);
                }
            }
        }

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
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}