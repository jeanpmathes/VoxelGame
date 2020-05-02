// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Input;
using Resources;
using System;
using VoxelGame.Logic;
using VoxelGame.Physics;
using VoxelGame.Rendering;

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

        public override Vector3 LookingDirection { get => camera.Front; }
        public override BlockSide TargetSide { get => selectedSide; }

        private readonly Camera camera;
        private readonly Vector3 cameraOffset = new Vector3(0f, 0.5f, 0f);

        private readonly float speed = 4f;
        private readonly float sprintSpeed = 6f;
        private readonly Vector3 maxForce = new Vector3(5000f, 0f, 5000f);
        private readonly float jumpForce = 25000f;

        private Vector2 lastMousePos;
        private bool firstMove = true;
        private readonly float mouseSensitivity = 0.2f;

        private int selectedX, selectedY, selectedZ;
        private BlockSide selectedSide;
        private readonly BoxRenderer selectionRenderer;

        private readonly float interactionCooldown = 0.25f;
        private float timer;

        private Block activeBlock;
        private bool hasPressedPlus = false;
        private bool hasPressedMinus = false;

        private static readonly int sectionSizeExp = (int)Math.Log(Section.SectionSize, 2);

        public Player(float mass, float drag, Vector3 startPosition, Camera camera, BoundingBox boundingBox) : base(mass, drag, boundingBox)
        {
            this.camera = camera ?? throw new ArgumentNullException(paramName: nameof(camera));

            Position = startPosition;
            camera.Position = startPosition;

            selectionRenderer = new BoxRenderer();

            activeBlock = Block.GLASS;

            // Request chunks around current position

            //int currentChunkX = (int)Math.Floor(Position.X) / Section.SectionSize;
            //int currentChunkZ = (int)Math.Floor(Position.Z) / Section.SectionSize;

            //for (int x = -RenderDistance; x <= RenderDistance; x++)
            //{
            //    for (int z = -RenderDistance; z <= RenderDistance; z++)
            //    {
            //        Game.World.RequestChunk(currentChunkX + x, currentChunkZ + z);
            //    }
            //}
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

        public override void Render()
        {
            if (selectedY >= 0)
            {
                Block selectedBlock = Game.World.GetBlock(selectedX, selectedY, selectedZ, out _);

                if (selectedBlock != Block.AIR)
                {
                    BoundingBox selectedBox = selectedBlock.GetBoundingBox(selectedX, selectedY, selectedZ);

                    Game.SelectionShader.SetVector3("color", new Vector3(1f, 0f, 0f));

                    selectionRenderer.SetBoundingBox(selectedBox);
                    selectionRenderer.Draw(selectedBox.Center);
                }
            }
        }

        protected override void Update(float deltaTime)
        {
            camera.Position = Position + cameraOffset;

            Ray ray = new Ray(camera.Position, camera.Front, 6f);

            Raycast.CastWorld(ray, out selectedX, out selectedY, out selectedZ, out selectedSide);

            if (Game.instance.Focused)
            {
                KeyboardState input = Keyboard.GetState();

                // Handling movement
                Vector3 movement = new Vector3();

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

                MouseState mouse = Mouse.GetState();

                if (firstMove)
                {
                    lastMousePos = new Vector2(mouse.X, mouse.Y);
                    firstMove = false;
                }
                else
                {
                    // Calculate the offset of the mouse position
                    var deltaX = mouse.X - lastMousePos.X;
                    var deltaY = mouse.Y - lastMousePos.Y;
                    lastMousePos = new Vector2(mouse.X, mouse.Y);

                    // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                    camera.Yaw += deltaX * mouseSensitivity;
                    camera.Pitch -= deltaY * mouseSensitivity;

                    Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
                }

                // Block selection
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

                // Handling world manipulation

                // Placement
                if (selectedY >= 0 && timer >= interactionCooldown && mouse.IsButtonDown(MouseButton.Right))
                {
                    int placePositionX = selectedX;
                    int placePositionY = selectedY;
                    int placePositionZ = selectedZ;

                    if (Game.World.GetBlock(placePositionX, placePositionY, placePositionZ, out _)?.IsReplaceable == false)
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

                    // Prevent block placement if the block would intersect the player
                    if (!BoundingBox.Intersects(activeBlock.GetBoundingBox(placePositionX, placePositionY, placePositionZ)))
                    {
                        activeBlock.Place(placePositionX, placePositionY, placePositionZ, this);

                        timer = 0;
                    }
                }

                // Destruction
                if (selectedY >= 0 && timer >= interactionCooldown && mouse.IsButtonDown(MouseButton.Left))
                {
                    Block selectedBlock = Game.World.GetBlock(selectedX, selectedY, selectedZ, out _);

                    if (selectedBlock != null)
                    {
                        selectedBlock.Destroy(selectedX, selectedY, selectedZ, this);

                        timer = 0;
                    }
                }
            }

            timer += deltaTime;

            // Check if the current chunk has changed and request new chunks if needed / release unneeded chunks
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
                        for (int z = 0; z < 2 * RenderDistance + 1; z++)
                        {
                            Game.World.ReleaseChunk(ChunkX + (RenderDistance - x) * -signX, ChunkZ + (RenderDistance - z) * -signZ);
                            Game.World.RequestChunk(currentChunkX + (RenderDistance - x) * signX, currentChunkZ + (RenderDistance - z) * signZ);
                        }
                    }

                    for (int z = 0; z < deltaZ; z++)
                    {
                        for (int x = 0; x < 2 * RenderDistance + 1; x++)
                        {
                            Game.World.ReleaseChunk(ChunkX + (RenderDistance - x) * -signX, ChunkZ + (RenderDistance - z) * -signZ);
                            Game.World.RequestChunk(currentChunkX + (RenderDistance - x) * signX, currentChunkZ + (RenderDistance - z) * signZ);
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
                selectionRenderer.Dispose();
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}