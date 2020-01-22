using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace VoxelGame.Rendering
{
    class Game : GameWindow
    {
        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        private readonly float[] cubeVertices =
        {
          // Position | UV Position
            // Front face
            0f, 0f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f, 1f,
            1f, 1f, 0f, 0.25f, 1f,
            1f, 0f, 0f, 0.25f, 0f,

            // Back face
            1f, 0f, 1f, 0f, 0f,
            1f, 1f, 1f, 0f, 1f,
            0f, 1f, 1f, 0.25f, 1f,
            0f, 0f, 1f, 0.25f, 0f,

            // Left face
            0f, 0f, 1f, 0f, 0f,
            0f, 1f, 1f, 0f, 1f,
            0f, 1f, 0f, 0.25f, 1f,
            0f, 0f, 0f, 0.25f, 0f,

            // Right face
            1f, 0f, 0f, 0f, 0f,
            1f, 1f, 0f, 0f, 1f,
            1f, 1f, 1f, 0.25f, 1f,
            1f, 0f, 1f, 0.25f, 0f,

            // Bottom face
            0f, 0f, 1f, 0.25f, 0f,
            0f, 0f, 0f, 0.25f, 1f,
            1f, 0f, 0f, 0.5f, 1f,
            1f, 0f, 1f, 0.5f, 0f,

            // Top face
            0f, 1f, 0f, 0.5f, 0f,
            0f, 1f, 1f, 0.5f, 1f,
            1f, 1f, 1f, 0.75f, 1f,
            1f, 1f, 0f, 0.75f, 0f
        };

        private readonly uint[] cubeIndices =
        {
            // Front face
            0, 2, 1,
            0, 3, 2,

            // Back face
            4, 6, 5,
            4, 7, 6,

            // Left face
            8, 10, 9,
            8, 11, 10,

            // Right face
            12, 14, 13,
            12, 15, 14,

            // Bottom face
            16, 18, 17,
            16, 19, 18,

            // Top face
            20, 22, 21,
            20, 23, 22
        };

        private int vertexBufferObject;
        private int elementBufferObject;
        private int vertexArrayObject;

        private Shader shader;
        private Texture texture;
        private Camera camera;

        private bool firstMove = true;
        private Vector2 lastPos;

        private double time;

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, cubeIndices.Length * sizeof(uint), cubeIndices, BufferUsageHint.StaticDraw);

            shader = new Shader("Rendering/Shaders/shader.vert", "Rendering/Shaders/shader.frag");
            shader.Use();

            texture = new Texture("Ressources/Textures/grass.png");
            texture.Use();

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);

            CursorVisible = false;

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            time += e.Time;

            GL.BindVertexArray(vertexArrayObject);

            shader.Use();

            Matrix4 model = Matrix4.Identity;
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, cubeIndices.Length, DrawElementsType.UnsignedInt, 0);

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
                camera.Position += camera.Front * cameraSpeed * (float)e.Time; // Forward 
            if (input.IsKeyDown(Key.S))
                camera.Position -= camera.Front * cameraSpeed * (float)e.Time; // Backwards
            if (input.IsKeyDown(Key.A))
                camera.Position -= camera.Right * cameraSpeed * (float)e.Time; // Left
            if (input.IsKeyDown(Key.D))
                camera.Position += camera.Right * cameraSpeed * (float)e.Time; // Right
            if (input.IsKeyDown(Key.Space))
                camera.Position += camera.Up * cameraSpeed * (float)e.Time; // Up 
            if (input.IsKeyDown(Key.LShift))
                camera.Position -= camera.Up * cameraSpeed * (float)e.Time; // Down

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
                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
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
            camera.Fov -= e.DeltaPrecise;

            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            camera.AspectRatio = Width / (float)Height;

            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }
    }
}
