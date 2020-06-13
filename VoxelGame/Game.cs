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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using VoxelGame.Entities;
using VoxelGame.Logic;
using VoxelGame.Rendering;

namespace VoxelGame
{
    internal class Game : GameWindow
    {
        public static Game instance;
        public static Player Player { get; private set; }
        public static World World { get; set; }

        /// <summary>
        /// Gets the <see cref="ArrayTexture"/> that contains all block textures. It is bound to unit 1 and 2;
        /// </summary>
        public static ArrayTexture BlockTextureArray { get; private set; }

        public static Shader SimpleSectionShader { get; private set; }
        public static Shader ComplexSectionShader { get; private set; }
        public static Shader SelectionShader { get; private set; }

        public static Random Random { get; private set; }

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Rendering setup
            GL.Enable(EnableCap.DebugOutput);

            debugCallbackDelegate = new DebugProc(DebugCallback);
            GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);

            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

#if WIREFRAME

            GL.LineWidth(10f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

#endif

            BlockTextureArray = new ArrayTexture("Resources/Textures", 16, true, TextureUnit.Texture1, TextureUnit.Texture2);
            Console.WriteLine(Language.BlockTexturesLoadedAmount + BlockTextureArray.Count);

            SimpleSectionShader = new Shader("Resources/Shaders/simplesection_shader.vert", "Resources/Shaders/section_shader.frag");
            ComplexSectionShader = new Shader("Resources/Shaders/complexsection_shader.vert", "Resources/Shaders/section_shader.frag");
            SelectionShader = new Shader("Resources/Shaders/selection_shader.vert", "Resources/Shaders/selection_shader.frag");

            // Block setup
            Block.LoadBlocks();
            Console.WriteLine(Language.BlocksLoadedAmount + Block.Count);

            // Get the application data folder and set up all required folders there
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "voxel");
            string worldsDirectory = Path.Combine(appDataFolder, "Worlds");

            Directory.CreateDirectory(appDataFolder);
            Directory.CreateDirectory(worldsDirectory);

            // Finding of worlds and letting the user choose a world
            List<(WorldInformation information, string path)> worlds = new List<(WorldInformation information, string path)>();

            foreach (string directory in Directory.GetDirectories(worldsDirectory))
            {
                string meta = Path.Combine(directory, "meta.json");

                if (File.Exists(meta))
                {
                    WorldInformation information = WorldInformation.Load(meta);
                    worlds.Add((information, directory));
                }
            }

            if (worlds.Count > 0)
            {
                Console.WriteLine(Language.ListingWorlds);
                Console.ForegroundColor = ConsoleColor.Cyan;

                for (int n = 0; n < worlds.Count; n++)
                {
                    Console.WriteLine($"{n + 1}: {worlds[n].information.Name} - {Language.CreatedOn}: {worlds[n].information.Creation}");
                }

                Console.ResetColor();
            }

            string input;
            if (worlds.Count == 0)
            {
                input = "y";
            }
            else
            {
                Console.WriteLine(Language.NewWorldPrompt + " [y|skip: n]");
                input = Console.ReadLine();
            }

            if (input == "y" || input == "yes")
            {
                // Create a new world
                Console.WriteLine(Language.EnterNameOfWorld);

                string name = Console.ReadLine();

                // Validate name
                if (string.IsNullOrEmpty(name) || name.Contains("\"") || name.Contains("<") || name.Contains(">") || name.Contains("|") || name.Contains("\\") || name.Contains("/"))
                {
                    name = "New World";
                }

                string path = Path.Combine(worldsDirectory, name);

                while (Directory.Exists(path))
                {
                    path += "_";
                }

                World = new World(name, path, DateTime.Now.GetHashCode());
            }
            else
            {
                // Load an existing world
                while (World == null)
                {
                    Console.WriteLine(Language.EnterIndexOfWorld);
                    string index = Console.ReadLine();

                    if (int.TryParse(index, out int n))
                    {
                        n--;

                        if (n >= 0 && n < worlds.Count)
                        {
                            World = new World(worlds[n].information, worlds[n].path);
                        }
                        else
                        {
                            Console.WriteLine(Language.WorldNotFound);
                        }
                    }
                    else
                    {
                        Console.WriteLine(Language.InputNotValid);
                    }
                }
            }

            // Player setup
            Camera camera = new Camera(new Vector3(), Width / (float)Height);
            Player = new Player(70f, 0.25f, new Vector3(0f, 1000f, 0f), camera, new Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.45f, 0.9f, 0.45f)));

            CursorVisible = false;

            // Other object setup
            Random = new Random();

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
            float deltaTime = (float)MathHelper.Clamp(e.Time, 0f, 1f);

            World.FrameUpdate(deltaTime);

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