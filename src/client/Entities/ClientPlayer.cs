// <copyright file="ClientPlayer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;
using System;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Entities
{
    public class ClientPlayer : Core.Entities.Player
    {
        public override Vector3 LookingDirection { get => camera.Front; }
        public override BlockSide TargetSide { get => selectedSide; }

        private readonly Camera camera;
        private readonly Vector3 cameraOffset = new Vector3(0f, 0.65f, 0f);

        private readonly float mouseSensitivity = Properties.client.Default.MouseSensitivity;

        public ClientPlayer(float mass, float drag, Camera camera, BoundingBox boundingBox) : base(mass, drag, boundingBox)
        {
            this.camera = camera;
            camera.Position = Position;

            selectionRenderer = new BoxRenderer();

            crosshair = new Texture("Resources/Textures/UI/crosshair.png", fallbackResolution: 32);
            crosshair.Use(OpenToolkit.Graphics.OpenGL4.TextureUnit.Texture6);

            crosshairRenderer = new ScreenElementRenderer();
            crosshairRenderer.SetTexture(crosshair);
            crosshairRenderer.SetColor(crosshairColor);

            activeBlock = Block.Grass;
            activeLiquid = Liquid.Water;
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

        private readonly Texture crosshair;
        private readonly ScreenElementRenderer crosshairRenderer;

        private readonly Vector3 crosshairPositionScale = new Vector3(0.5f, 0.5f, Properties.client.Default.CrosshairScale);
        private readonly Vector3 crosshairColor = Properties.client.Default.CrosshairColor.ToVector3();

        public void Render()
        {
            if (selectedY >= 0)
            {
                Block selectedBlock = Game.World.GetBlock(selectedX, selectedY, selectedZ, out _) ?? Block.Air;

#if DEBUG
                if (selectedBlock != Block.Air)
#else
                if (!selectedBlock.IsReplaceable)
#endif
                {
                    BoundingBox selectedBox = selectedBlock.GetBoundingBox(selectedX, selectedY, selectedZ);

                    Client.SelectionShader.SetVector3("color", new Vector3(0.1f, 0.1f, 0.1f));

                    selectionRenderer.SetBoundingBox(selectedBox);
                    selectionRenderer.Draw(selectedBox.Center);
                }
            }

            crosshairRenderer.Draw(crosshairPositionScale);
        }

        private Vector3 movement;

        protected override void OnUpdate(float deltaTime)
        {
            movement = Vector3.Zero;

            camera.Position = Position + cameraOffset;

            Ray ray = new Ray(camera.Position, camera.Front, 6f);
            Raycast.CastWorld(ray, out selectedX, out selectedY, out selectedZ, out selectedSide);

            // Do input handling.
            if (Client.Instance.IsFocused)
            {
                KeyboardState input = Client.Instance.KeyboardState;
                MouseState mouse = Client.Instance.MouseState;

                MovementInput(input);
                MouseChange();

                BlockLiquidSelection(input);

                WorldInteraction(input, mouse);
            }

            timer += deltaTime;
        }

        private readonly float speed = 4f;
        private readonly float sprintSpeed = 6f;
        private readonly Vector3 maxForce = new Vector3(5000f, 0f, 5000f);
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

            if (input.IsKeyDown(Key.Space) && IsGrounded) // Jump
            {
                AddForce(new Vector3(0f, jumpForce, 0f));
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

        private int selectedX, selectedY, selectedZ;
        private BlockSide selectedSide;

        private float timer;

        private void WorldInteraction(KeyboardState input, MouseState mouse)
        {
            Block? target = Game.World.GetBlock(selectedX, selectedY, selectedZ, out _);

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
                if (!blockMode || !activeBlock.IsSolid || !BoundingBox.Intersects(activeBlock.GetBoundingBox(placePositionX, placePositionY, placePositionZ)))
                {
                    if (blockMode) activeBlock.Place(placePositionX, placePositionY, placePositionZ, this);
                    else activeLiquid.Fill(placePositionX, placePositionY, placePositionZ, LiquidLevel.One, out _);

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
                if (blockMode) target.Destroy(selectedX, selectedY, selectedZ, this);
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

                Game.World.GetLiquid(x, y, z, out _, out _)?.Take(x, y, z, ref level);
            }
        }

        private Block activeBlock;
        private Liquid activeLiquid;

        private bool blockMode = true;

        private bool hasPressedPlus;
        private bool hasPressedMinus;
        private bool hasSwitchedMode;

        private void BlockLiquidSelection(KeyboardState input)
        {
            if (input.IsKeyDown(Key.R) && !hasSwitchedMode)
            {
                blockMode = !blockMode;
                hasSwitchedMode = true;

                Console.WriteLine(blockMode ? Language.CurrentBlockIs + activeBlock.Name : Language.CurrentLiquidIs + activeLiquid.Name);
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

                Console.WriteLine(blockMode ? Language.CurrentBlockIs + activeBlock.Name : Language.CurrentLiquidIs + activeLiquid.Name);
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

                Console.WriteLine(blockMode ? Language.CurrentBlockIs + activeBlock.Name : Language.CurrentLiquidIs + activeLiquid.Name);
            }
            else if (input.IsKeyUp(Key.KeypadMinus))
            {
                hasPressedMinus = false;
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
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}