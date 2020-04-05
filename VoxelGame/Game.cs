// <copyright file="Game.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Resources;
using System;
using VoxelGame.Entities;
using VoxelGame.Logic;
using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;

namespace VoxelGame
{
    internal class Game : GameWindow
    {
        public static Game instance;
        public static Player Player { get; private set; }
        public static TextureAtlas Atlas { get; private set; }
        public static Shader SectionShader { get; private set; }
        public static Shader SelectionShader { get; private set; }
        public static World World { get; set; }

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            Atlas = new TextureAtlas("Resources/Textures");

            SectionShader = new Shader("Resources/Shaders/section_shader.vert", "Resources/Shaders/section_shader.frag");
            SelectionShader = new Shader("Resources/Shaders/selection_shader.vert", "Resources/Shaders/selection_shader.frag");

            Block.LoadBlocks();
            Console.WriteLine(Language.BlocksLoadedAmount + Block.blockDictionary.Count);

            Camera camera = new Camera(new Vector3(), Width / (float)Height);
            Player = new Player(70f, 0.03f, new Vector3(0f, 1000f, 0f), camera, new Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.45f, 0.9f, 0.45f)));

            //World = new World(new FlatGenerator(520, 500));
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

            World.FrameRender();

            SwapBuffers();

            //Console.WriteLine(1f / e.Time);

            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            World.FrameUpdate((float)e.Time);

            if (!Focused) // check to see if the window is focused
            {
                return;
            }

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
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