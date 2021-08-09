// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Objects;
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
                KeyboardState input = Client.Keyboard;
                MouseState mouse = Client.Mouse;

                MovementInput(input);
                MouseChange();

                BlockLiquidSelection(input, firstUpdate);

                WorldInteraction(input, mouse);

                int headX = (int)Math.Floor(camera.Position.X);
                int headY = (int)Math.Floor(camera.Position.Y);
                int headZ = (int)Math.Floor(camera.Position.Z);

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

        private void MovementInput(KeyboardState input)
        {
            movement = new Vector3();

            if (input.IsKeyDown(Key.W))
                movement += Forward; // Forward

            if (input.IsKeyDown(Key.S))
                movement -= Forward; // Backwards

            if (input.IsKeyDown(Key.A))
                movement -= Right; // Left

            if (input.IsKeyDown(Key.D))
                movement += Right; // Right

            if (movement != Vector3.Zero)
            {
                if (input.IsKeyDown(Key.ShiftLeft))
                {
                    movement = movement.Normalized() * sprintSpeed;
                }
                else
                {
                    movement = movement.Normalized() * speed;
                }
            }

            Move(movement, maxForce);

            if (input.IsKeyDown(Key.Space))
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
            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            camera.Yaw += Client.SmoothMouseDelta.X * mouseSensitivity;
            camera.Pitch -= Client.SmoothMouseDelta.Y * mouseSensitivity;

            Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
        }

        private readonly float interactionCooldown = 0.25f;

        private int selectedX, selectedY = -1, selectedZ;
        private BlockSide selectedSide;

        private float timer;

        private void WorldInteraction(KeyboardState input, MouseState mouse)
        {
            Block? target = World.GetBlock(selectedX, selectedY, selectedZ, out _);

            if (target == null)
            {
                return;
            }

            PlaceInteract(input, mouse, target);
            DestroyInteract(mouse, target);
        }

        private void PlaceInteract(KeyboardState input, MouseState mouse, Block target)
        {
            if (timer < interactionCooldown || mouse.IsButtonUp(MouseButton.Right)) return;

            int placePositionX = selectedX;
            int placePositionY = selectedY;
            int placePositionZ = selectedZ;

            if (input.IsKeyDown(Key.ControlLeft) || !target.IsInteractable)
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

        private void DestroyInteract(MouseState mouse, Block target)
        {
            if (timer >= interactionCooldown && mouse.IsButtonDown(MouseButton.Left))
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

        private bool hasPressedPlus;
        private bool hasPressedMinus;
        private bool hasSwitchedMode;

        private void BlockLiquidSelection(KeyboardState input, bool updateUI)
        {
            if (input.IsKeyDown(Key.R) && !hasSwitchedMode)
            {
                blockMode = !blockMode;
                hasSwitchedMode = true;

                updateUI = true;
            }
            else if (input.IsKeyUp(Key.R))
            {
                hasSwitchedMode = false;
            }

            if (input.IsKeyDown(Key.KeypadPlus) && !hasPressedPlus)
            {
                if (blockMode) activeBlock = (activeBlock.Id != Block.Count - 1) ? Block.TranslateID(activeBlock.Id + 1) : Block.TranslateID(1);
                else activeLiquid = (activeLiquid.Id != Liquid.Count - 1) ? Liquid.TranslateID(activeLiquid.Id + 1) : Liquid.TranslateID(1);

                hasPressedPlus = true;

                updateUI = true;
            }
            else if (input.IsKeyUp(Key.KeypadPlus))
            {
                hasPressedPlus = false;
            }

            if (input.IsKeyDown(Key.KeypadMinus) && !hasPressedMinus)
            {
                if (blockMode) activeBlock = (activeBlock.Id != 1) ? Block.TranslateID(activeBlock.Id - 1) : Block.TranslateID((uint)(Block.Count - 1));
                else activeLiquid = (activeLiquid.Id != 1) ? Liquid.TranslateID(activeLiquid.Id - 1) : Liquid.TranslateID((uint)(Liquid.Count - 1));

                hasPressedMinus = true;

                updateUI = true;
            }
            else if (input.IsKeyUp(Key.KeypadMinus))
            {
                hasPressedMinus = false;
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