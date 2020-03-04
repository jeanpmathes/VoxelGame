// <copyright file="Player.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Input;

using VoxelGame.Rendering;
using VoxelGame.Physics;

namespace VoxelGame.Entities
{
    public class Player : PhysicsEntity
    {
        private Camera camera;
        private Vector3 cameraOffset = new Vector3(0f, 0.5f, 0f);

        private float speed = 50f;
        private float jumpForce = 200f;

        private Vector2 lastMousePos;
        private bool firstMove = true;
        private float mouseSensitivity = 0.2f;

        public Player(float mass, float drag, Vector3 startPosition, Camera camera, BoundingBox boundingBox) : base(mass, drag, boundingBox)
        {
            this.camera = camera;

            Position = startPosition;            
            camera.Position = startPosition;
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

        protected override void Update()
        {
            camera.Position = Position + cameraOffset;

            if (Game.instance.Focused)
            {
                KeyboardState input = Keyboard.GetState();

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
                }
               
                if (input.IsKeyDown(Key.Space))
                    movement += new Vector3(0f, jumpForce, 0f); // Up

                AddForce(movement);

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
            }
        }
    }
}
