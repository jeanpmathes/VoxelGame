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
using System.ComponentModel;
using System.IO;
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

        private string worldsDirectory;

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DebugOutput);

            debugCallbackDelegate = new DebugProc(DebugCallback);
            GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);

            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            Atlas = new TextureAtlas("Resources/Textures");

            SectionShader = new Shader("Resources/Shaders/section_shader.vert", "Resources/Shaders/section_shader.frag");
            SelectionShader = new Shader("Resources/Shaders/selection_shader.vert", "Resources/Shaders/selection_shader.frag");

            Block.LoadBlocks();
            Console.WriteLine(Language.BlocksLoadedAmount + Block.Count);

            worldsDirectory = Directory.GetCurrentDirectory() + @"\Worlds";
            Console.WriteLine("Listing all worlds");
            foreach (string world in Directory.GetDirectories(worldsDirectory))
            {
                Console.WriteLine(Path.GetFileName(world));
            }

            Console.WriteLine("Please enter the name of the world to load");
            string newWorld = Console.ReadLine();
            World = new World(Path.Combine(worldsDirectory, newWorld), new NoiseGenerator(2133));

            Camera camera = new Camera(new Vector3(), Width / (float)Height);
            Player = new Player(70f, 0.03f, new Vector3(0f, 1000f, 0f), camera, new Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.45f, 0.9f, 0.45f)));

            CursorVisible = false;

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
                Mouse.SetPosition(X + (Width / 2f), Y + (Height / 2f));
            }

            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                World.Save().Wait();
            }
            catch (AggregateException exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(
                    $"{DateTime.Now} | ---- WORLD SAVING ERROR ------------- \n" +
                    $"An exception was thrown when saving the world. Exception: ({exception.GetBaseException().GetType()})\n" +
                    $"{exception.GetBaseException().Message}\n" +
                    $"The process will be terminated, but some data may be lost.\n");
                Console.ResetColor();
            }

            World.Dispose();
            Player.Dispose();

            base.OnClosing(e);
        }

        private DebugProc debugCallbackDelegate;

        private void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (id == 131169 || id == 131185 || id == 131218 || id == 131204) return;

            string sourceShort = "NONE";
            switch (source)
            {
                case DebugSource.DebugSourceApi:
                    sourceShort = "API";
                    break;
                case DebugSource.DebugSourceApplication:
                    sourceShort = "APPLICATION";
                    break;
                case DebugSource.DebugSourceOther:
                    sourceShort = "OTHER";
                    break;
                case DebugSource.DebugSourceShaderCompiler:
                    sourceShort = "SHADER COMPILER";
                    break;
                case DebugSource.DebugSourceThirdParty:
                    sourceShort = "THIRD PARTY";
                    break;
                case DebugSource.DebugSourceWindowSystem:
                    sourceShort = "WINDOWS SYSTEM";
                    break;
            }

            string typeShort = "NONE";
            switch (type)
            {
                case DebugType.DebugTypeDeprecatedBehavior:
                    typeShort = "DEPRECATED BEHAVIOR";
                    break;
                case DebugType.DebugTypeError:
                    typeShort = "ERROR";
                    break;
                case DebugType.DebugTypeMarker:
                    typeShort = "MARKER";
                    break;
                case DebugType.DebugTypeOther:
                    typeShort = "OTHER";
                    break;
                case DebugType.DebugTypePerformance:
                    typeShort = "PERFORMANCE";
                    break;
                case DebugType.DebugTypePopGroup:
                    typeShort = "POP GROUP";
                    break;
                case DebugType.DebugTypePortability:
                    typeShort = "PORTABILITY";
                    break;
                case DebugType.DebugTypePushGroup:
                    typeShort = "PUSH GROUP";
                    break;
                case DebugType.DebugTypeUndefinedBehavior:
                    typeShort = "UNDEFINED BEHAVIOR";
                    break;
            }

            string idResolved = "-";
            switch (id)
            {
                case 0x500:
                    idResolved = "GL_INVALID_ENUM";
                    break;
                case 0x501:
                    idResolved = "GL_INVALID_VALUE";
                    break;
                case 0x502:
                    idResolved = "GL_INVALID_OPERATION";
                    break;
                case 0x503:
                    idResolved = "GL_STACK_OVERFLOW";
                    break;
                case 0x504:
                    idResolved = "GL_STACK_UNDERFLOW";
                    break;
                case 0x505:
                    idResolved = "GL_OUT_OF_MEMORY";
                    break;
                case 0x506:
                    idResolved = "GL_INVALID_FRAMEBUFFER_OPERATION";
                    break;
                case 0x507:
                    idResolved = "GL_CONTEXT_LOST";
                    break;
            }

            string severityShort = "NONE";
            switch (severity)
            {
                case DebugSeverity.DebugSeverityHigh:
                    severityShort = "HIGH";
                    break;
                case DebugSeverity.DebugSeverityLow:
                    severityShort = "LOW";
                    break;
                case DebugSeverity.DebugSeverityMedium:
                    severityShort = "MEDIUM";
                    break;
                case DebugSeverity.DebugSeverityNotification:
                    severityShort = "NOTIFICATION";
                    break;
            }

            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write(
                $"{DateTime.Now} | ---- GL DEBUG MESSAGE ------------- \n" +
                $"SOURCE: {sourceShort} | TYPE: {typeShort} \n" +
                $"ID: {id} ({idResolved}) | SEVERITY: {severityShort} \n" +
                $"MESSAGE: {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE"}\n");

            Console.ResetColor();
        }
    }
}