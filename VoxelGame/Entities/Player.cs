// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;
using System;
using VoxelGame.Logic;
using VoxelGame.Physics;
using VoxelGame.Rendering;
using VoxelGame.Resources.Language;
using VoxelGame.Utilities;

namespace VoxelGame.Entities
{
    public class Player : PhysicsEntity
    {
        /// <summary>
        /// Gets the extents of how many chunks should be around this player.
        /// </summary>
        public int RenderDistance { get; } = 2;

        /// <summary>
        /// Gets whether this player has moved to a different chunk in the last frame.
        /// </summary>
        public bool ChunkHasChanged { get; private set; }

        /// <summary>
        /// The x coordinate of the current chunk this player is in.
        /// </summary>
        public int ChunkX { get; private set; }

        /// <summary>
        /// The z coordinate of the current chunk this player is in.
        /// </summary>
        public int ChunkZ { get; private set; }

        private Vector3 movement;

        public override Vector3 Movement { get => movement; }
        public override Vector3 LookingDirection { get => camera.Front; }
        public override BlockSide TargetSide { get => selectedSide; }

        private readonly Camera camera;
        private readonly Vector3 cameraOffset = new Vector3(0f, 0.65f, 0f);

        private readonly float mouseSensitivity = Config.GetFloat("mouseSensitivity", 0.3f);

        private static readonly int sectionSizeExp = (int)Math.Log(Section.SectionSize, 2);

        public Player(float mass, float drag, Camera camera, BoundingBox boundingBox) : base(mass, drag, boundingBox)
        {
            this.camera = camera ?? throw new ArgumentNullException(paramName: nameof(camera));

            Position = Game.World.Information.SpawnInformation.Position;
            camera.Position = Position;

            selectionRenderer = new BoxRenderer();

            crosshair = new Texture("Resources/Textures/UI/crosshair.png", fallbackResolution: 32);
            crosshair.Use(OpenToolkit.Graphics.OpenGL4.TextureUnit.Texture6);

            crosshairRenderer = new ScreenElementRenderer();
            crosshairRenderer.SetTexture(crosshair);
            crosshairRenderer.SetColor(crosshairColor);

            activeBlock = Block.Grass;

            // Request chunks around current position
            ChunkX = (int)Math.Floor(Position.X) >> sectionSizeExp;
            ChunkZ = (int)Math.Floor(Position.Z) >> sectionSizeExp;

            for (int x = -RenderDistance; x <= RenderDistance; x++)
            {
                for (int z = -RenderDistance; z <= RenderDistance; z++)
                {
                    Game.World.RequestChunk(ChunkX + x, ChunkZ + z);
                }
            }
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

        private readonly BoxRenderer selectionRenderer;

        private readonly Texture crosshair;
        private readonly ScreenElementRenderer crosshairRenderer;

        private readonly Vector3 crosshairPositionScale = new Vector3(0.5f, 0.5f, Config.GetFloat("crosshairScale", 0.0225f));
        private readonly Vector3 crosshairColor = Config.GetVector3("crosshairColor", Vector3.One);

        public override void Render()
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

                    Game.SelectionShader.SetVector3("color", new Vector3(0.1f, 0.1f, 0.1f));

                    selectionRenderer.SetBoundingBox(selectedBox);
                    selectionRenderer.Draw(selectedBox.Center);
                }
            }

            crosshairRenderer.Draw(crosshairPositionScale);
        }

        protected override void Update(float deltaTime)
        {
            camera.Position = Position + cameraOffset;
            this.movement = Vector3.Zero;

            Ray ray = new Ray(camera.Position, camera.Front, 6f);
            Raycast.CastWorld(ray, out selectedX, out selectedY, out selectedZ, out selectedSide);

            // Do input handling.
            if (Game.Instance.IsFocused)
            {
                KeyboardState input = Game.Instance.KeyboardState;
                MouseState mouse = Game.Instance.MouseState;

                MovementInput(input);
                MouseChange();

                BlockSelection(input);

                WorldInteraction(input, mouse);
            }

            timer += deltaTime;

            // Check if the current chunk has changed and request new chunks if needed / release unneeded chunks.
            ChunkChange();
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
            camera.Yaw += Game.SmoothMouseDelta.X * mouseSensitivity;
            camera.Pitch -= Game.SmoothMouseDelta.Y * mouseSensitivity;

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

            // Right mouse button.
            if (selectedY >= 0 && timer >= interactionCooldown && mouse.IsButtonDown(MouseButton.Right))
            {
                if (input.IsKeyDown(Key.ControlLeft) || !target.IsInteractable)
                {
                    int placePositionX = selectedX;
                    int placePositionY = selectedY;
                    int placePositionZ = selectedZ;

                    if (!target.IsReplaceable)
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

                    // Prevent block placement if the block would intersect the player.
                    if (!activeBlock.IsSolid || !BoundingBox.Intersects(activeBlock.GetBoundingBox(placePositionX, placePositionY, placePositionZ)))
                    {
                        activeBlock.Place(placePositionX, placePositionY, placePositionZ, this);

                        timer = 0;
                    }
                }
                else if (target.IsInteractable)
                {
                    target.EntityInteract(this, selectedX, selectedY, selectedZ);

                    timer = 0;
                }
            }

            // Left mouse button.
            if (selectedY >= 0 && timer >= interactionCooldown && mouse.IsButtonDown(MouseButton.Left))
            {
                target.Destroy(selectedX, selectedY, selectedZ, this);

                timer = 0;
            }
        }

        private Block activeBlock;
        private bool hasPressedPlus;
        private bool hasPressedMinus;

        private void BlockSelection(KeyboardState input)
        {
            if (input.IsKeyDown(Key.KeypadPlus) && !hasPressedPlus)
            {
                activeBlock = (activeBlock.Id != Block.Count - 1) ? Block.TranslateID((ushort)(activeBlock.Id + 1)) : Block.TranslateID(1);
                hasPressedPlus = true;

                Console.WriteLine(Language.CurrentBlockIs + activeBlock.Name);
            }
            else if (input.IsKeyUp(Key.KeypadPlus))
            {
                hasPressedPlus = false;
            }

            if (input.IsKeyDown(Key.KeypadMinus) && !hasPressedMinus)
            {
                activeBlock = (activeBlock.Id != 1) ? Block.TranslateID((ushort)(activeBlock.Id - 1)) : Block.TranslateID((ushort)(Block.Count - 1));
                hasPressedMinus = true;

                Console.WriteLine(Language.CurrentBlockIs + activeBlock.Name);
            }
            else if (input.IsKeyUp(Key.KeypadMinus))
            {
                hasPressedMinus = false;
            }
        }

        private void ChunkChange()
        {
            int currentChunkX = (int)Math.Floor(Position.X) >> sectionSizeExp;
            int currentChunkZ = (int)Math.Floor(Position.Z) >> sectionSizeExp;

            if (currentChunkX != ChunkX || currentChunkZ != ChunkZ)
            {
                ChunkHasChanged = true;

                int deltaX = Math.Abs(currentChunkX - ChunkX);
                int deltaZ = Math.Abs(currentChunkZ - ChunkZ);

                int signX = (currentChunkX - ChunkX >= 0) ? 1 : -1;
                int signZ = (currentChunkZ - ChunkZ >= 0) ? 1 : -1;

                // Check if player moved completely out of claimed chunks
                if (deltaX > 2 * RenderDistance || deltaZ > 2 * RenderDistance)
                {
                    for (int x = -RenderDistance; x <= RenderDistance; x++)
                    {
                        for (int z = -RenderDistance; z <= RenderDistance; z++)
                        {
                            Game.World.ReleaseChunk(ChunkX + x, ChunkZ + z);
                            Game.World.RequestChunk(currentChunkX + x, currentChunkZ + z);
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < deltaX; x++)
                    {
                        for (int z = 0; z < (2 * RenderDistance) + 1; z++)
                        {
                            Game.World.ReleaseChunk(ChunkX + ((RenderDistance - x) * -signX), ChunkZ + ((RenderDistance - z) * -signZ));
                            Game.World.RequestChunk(currentChunkX + ((RenderDistance - x) * signX), currentChunkZ + ((RenderDistance - z) * signZ));
                        }
                    }

                    for (int z = 0; z < deltaZ; z++)
                    {
                        for (int x = 0; x < (2 * RenderDistance) + 1; x++)
                        {
                            Game.World.ReleaseChunk(ChunkX + ((RenderDistance - x) * -signX), ChunkZ + ((RenderDistance - z) * -signZ));
                            Game.World.RequestChunk(currentChunkX + ((RenderDistance - x) * signX), currentChunkZ + ((RenderDistance - z) * signZ));
                        }
                    }
                }

                ChunkX = currentChunkX;
                ChunkZ = currentChunkZ;
            }
        }

        #region IDisposable Support

        private bool disposed = false;

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