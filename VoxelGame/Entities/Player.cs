// <copyright file="Player.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Input;
using VoxelGame.Logic;
using VoxelGame.Physics;
using VoxelGame.Rendering;
using Resources;

namespace VoxelGame.Entities
{
    public class Player : PhysicsEntity
    {
        private Camera camera;
        private Vector3 cameraOffset = new Vector3(0f, 0.5f, 0f);

        private float speed = 5f;
        private float jumpForce = 550f;

        private Vector2 lastMousePos;
        private bool firstMove = true;
        private float mouseSensitivity = 0.2f;

        private int selectedX, selectedY, selectedZ;
        private BlockSide selectedSide;
        private BoxRenderer selectionRenderer;

        private readonly float interactionCooldown = 0.25f;
        private float timer;

        private Block activeBlock;
        private bool hasPressedPlus = false;
        private bool hasPressedMinus = false;

        public Player(float mass, float drag, Vector3 startPosition, Camera camera, BoundingBox boundingBox) : base(mass, drag, boundingBox)
        {
            this.camera = camera;

            Position = startPosition;
            camera.Position = startPosition;

            selectionRenderer = new BoxRenderer();

            activeBlock = Block.GLASS;
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
                Block selectedBlock = Game.World.GetBlock(selectedX, selectedY, selectedZ);

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
                    movement = movement.Normalized() * speed;

                    Move(movement);
                }

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
                    activeBlock = (activeBlock.Id != Block.blockDictionary.Count - 1) ? Block.blockDictionary[(ushort)(activeBlock.Id + 1)] : Block.blockDictionary[1];
                    hasPressedPlus = true;

                    System.Console.WriteLine(Language.CurrentBlockIs + activeBlock.Name);
                }
                else if (input.IsKeyUp(Key.KeypadPlus))
                {
                    hasPressedPlus = false;
                }

                if (input.IsKeyDown(Key.KeypadMinus) && !hasPressedMinus)
                {
                    activeBlock = (activeBlock.Id != 1) ? Block.blockDictionary[(ushort)(activeBlock.Id - 1)] : Block.blockDictionary[(ushort)(Block.blockDictionary.Count - 1)];
                    hasPressedMinus = true;

                    System.Console.WriteLine(Language.CurrentBlockIs + activeBlock.Name);
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
                    Block selectedBlock = Game.World.GetBlock(selectedX, selectedY, selectedZ);

                    if (selectedBlock != null)
                    {
                        selectedBlock.Destroy(selectedX, selectedY, selectedZ, this);

                        timer = 0;
                    }
                }
            }

            timer += deltaTime;
        }
    }
}