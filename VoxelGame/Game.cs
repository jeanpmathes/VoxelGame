// <copyright file="Game.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;

using VoxelGame.Logic;
using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;

namespace VoxelGame
{
    internal class Game : GameWindow
    {
        public static Camera MainCamera { get; private set; }
        public static TextureAtlas Atlas { get; private set; }
        public static Shader Shader { get; private set; }
        public static World World { get; set; }

        public static Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();

        public static Block AIR;
        public static Block GRASS;
        public static Block TALL_GRASS;
        public static Block DIRT;
        public static Block STONE;
        public static Block COBBLESTONE;
        public static Block LOG;
        public static Block LEAVES;
        public static Block SAND;
        public static Block GLASS;
        public static Block ORE_COAL;
        public static Block ORE_IRON;
        public static Block ORE_GOLD;

        private const float cameraSpeed = 8f;
        private const float sensitivity = 0.2f;

        private bool firstMove = true;
        private Vector2 lastPos;

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            MainCamera = new Camera(new Vector3(-3f, 500f, 5f), Width / (float)Height);
            Atlas = new TextureAtlas("Resources/Textures");

            Shader = new Shader("Rendering/Shaders/shader.vert", "Rendering/Shaders/shader.frag");

            AIR = new AirBlock("air");
            GRASS = new BasicBlock("grass", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 2));
            TALL_GRASS = new CrossBlock("tall_grass");
            DIRT = new BasicBlock("dirt", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            STONE = new BasicBlock("stone", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            COBBLESTONE = new BasicBlock("cobblestone", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            LOG = new BasicBlock("log", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 1));
            SAND = new BasicBlock("sand", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            LEAVES = new BasicBlock("leaves", false, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            GLASS = new BasicBlock("glass", false, false, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            ORE_COAL = new BasicBlock("ore_coal", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            ORE_IRON = new BasicBlock("ore_iron", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            ORE_GOLD = new BasicBlock("ore_gold", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));

            Console.WriteLine($"Blocks loaded: {blockDictionary.Count} in total");

            //World = new World(new FlatGenerator(500, 490));
            //World = new World(new SineGenerator(20, 512, 0.05f, 0.05f));
            World = new World(new NoiseGenerator(2133));

            CursorVisible = false;

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error);
            }

            World.FrameUpdate();

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

            float cameraSpeed = Game.cameraSpeed;
            if (input.IsKeyDown(Key.ControlLeft))
            {
                cameraSpeed *= 5;
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

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }
    }
}