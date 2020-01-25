using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using VoxelGame.Logic;

namespace VoxelGame.Rendering
{
    class Game : GameWindow
    {
        public static Camera MainCamera { get; private set; }
        public static TextureAtlas Atlas { get; private set; }

        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;        

        private bool firstMove = true;
        private Vector2 lastPos;

        private double time;

        private Shader shader;

        private Block GRASS;
        private Block DIRT;
        private Block STONE;

        private Block[,,] blocks;

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            MainCamera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);
            Atlas = new TextureAtlas("Ressources/Textures");

            shader = new Shader("Rendering/Shaders/shader.vert", "Rendering/Shaders/shader.frag");

            GRASS = new Block("grass", shader, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 2));
            DIRT = new Block("dirt", shader, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            STONE = new Block("stone", shader, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));

            blocks = new Block[32, 32, 32];
            Block current;

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    if (y == 31)
                    {
                        current = GRASS;
                    }
                    else if (y > 25)
                    {
                        current = DIRT;
                    }
                    else
                    {
                        current = STONE;
                    }

                    for (int z = 0; z < 32; z++)
                    {
                        blocks[x, y, z] = current;
                    }
                }
            }

            CursorVisible = false;

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            time += e.Time;

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    for (int z = 0; z < 32; z++)
                    {
                        blocks[x, y, z].RenderBlock(new Vector3(x, y, z));
                    }
                }
            }

            SwapBuffers();

            base.OnRenderFrame(e);
        }
        
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused) // check to see if the window is focused
            {
                return;
            }

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            if (input.IsKeyDown(Key.W))
                MainCamera.Position += MainCamera.Front * cameraSpeed * (float)e.Time; // Forward 
            if (input.IsKeyDown(Key.S))
                MainCamera.Position -= MainCamera.Front * cameraSpeed * (float)e.Time; // Backwards
            if (input.IsKeyDown(Key.A))
                MainCamera.Position -= MainCamera.Right * cameraSpeed * (float)e.Time; // Left
            if (input.IsKeyDown(Key.D))
                MainCamera.Position += MainCamera.Right * cameraSpeed * (float)e.Time; // Right
            if (input.IsKeyDown(Key.Space))
                MainCamera.Position += MainCamera.Up * cameraSpeed * (float)e.Time; // Up 
            if (input.IsKeyDown(Key.LShift))
                MainCamera.Position -= MainCamera.Up * cameraSpeed * (float)e.Time; // Down

            MouseState mouse = Mouse.GetState();

            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                MainCamera.Yaw += deltaX * sensitivity;
                MainCamera.Pitch -= deltaY * sensitivity;
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused) // check to see if the window is focused
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            MainCamera.Fov -= e.DeltaPrecise;

            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            MainCamera.AspectRatio = Width / (float)Height;

            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }
    }
}
