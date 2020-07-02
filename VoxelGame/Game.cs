// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
using VoxelGame.Entities;
using VoxelGame.Logic;
using VoxelGame.Rendering;
using VoxelGame.Resources.Language;

namespace VoxelGame
{
    internal class Game : GameWindow
    {
        public static Game instance = null!;
        public static Player Player { get; private set; } = null!;
        public static World World { get; set; } = null!;

        /// <summary>
        /// Gets the <see cref="ArrayTexture"/> that contains all block textures. It is bound to unit 1 and 2;
        /// </summary>
        public static ArrayTexture BlockTextureArray { get; private set; } = null!;

        public static Shader SimpleSectionShader { get; private set; } = null!;
        public static Shader ComplexSectionShader { get; private set; } = null!;
        public static Shader SelectionShader { get; private set; } = null!;

        public static Random Random { get; private set; } = null!;

        private bool wireframeMode = false;
        private bool hasReleasesWireframeKey = true;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            instance = this;
        }

        protected override void OnLoad()
        {
            // Rendering setup
            GL.Enable(EnableCap.DebugOutput);

            debugCallbackDelegate = new DebugProc(DebugCallback);
            GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);

            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            BlockTextureArray = new ArrayTexture("Resources/Textures", 16, true, TextureUnit.Texture1, TextureUnit.Texture2);
            Console.WriteLine(Language.BlockTexturesLoadedAmount + BlockTextureArray.Count);

            SimpleSectionShader = new Shader("Resources/Shaders/simplesection_shader.vert", "Resources/Shaders/section_shader.frag");
            ComplexSectionShader = new Shader("Resources/Shaders/complexsection_shader.vert", "Resources/Shaders/section_shader.frag");
            SelectionShader = new Shader("Resources/Shaders/selection_shader.vert", "Resources/Shaders/selection_shader.frag");

            // Block setup
            Block.LoadBlocks();
            Console.WriteLine(Language.BlocksLoadedAmount + Block.Count);

            Console.WriteLine(Language.TextureBlockRatio + $"{BlockTextureArray.Count * 1f / Block.Count:F02}");

            // Get the application data folder and set up all required folders there
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "voxel");
            string worldsDirectory = Path.Combine(appDataFolder, "Worlds");

            Directory.CreateDirectory(appDataFolder);
            Directory.CreateDirectory(worldsDirectory);

            WorldSetup(worldsDirectory);

            // Player setup
            Camera camera = new Camera(new Vector3(), Size.X / (float)Size.Y);
            Player = new Player(70f, 0.25f, camera, new Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.35f, 0.9f, 0.35f)));

            CursorVisible = false;

            // Other object setup
            Random = new Random();

            base.OnLoad();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "The characters '[' and ']' are not culture dependent.")]
        private static void WorldSetup(string worldsDirectory)
        {
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

                for (int n = 0; n < worlds.Count; n++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{n + 1}: ");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{worlds[n].information.Name} - {Language.CreatedOn}: {worlds[n].information.Creation}");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" [");

                    if (worlds[n].information.Version == Program.Version) Console.ForegroundColor = ConsoleColor.Green;
                    else Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(worlds[n].information.Version);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("]");
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

                Console.ForegroundColor = ConsoleColor.White;
                input = Console.ReadLine();
                Console.ResetColor();
            }

            if (input == "y" || input == "yes")
            {
                // Create a new world
                Console.WriteLine(Language.EnterNameOfWorld);

                Console.ForegroundColor = ConsoleColor.White;
                string name = Console.ReadLine();
                Console.ResetColor();

                // Validate name
                if (string.IsNullOrEmpty(name) ||
                    name.Contains("\"", StringComparison.Ordinal) ||
                    name.Contains("<", StringComparison.Ordinal) ||
                    name.Contains(">", StringComparison.Ordinal) ||
                    name.Contains("|", StringComparison.Ordinal) ||
                    name.Contains("\\", StringComparison.Ordinal) ||
                    name.Contains("/", StringComparison.Ordinal))
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

                    Console.ForegroundColor = ConsoleColor.White;
                    string index = Console.ReadLine();
                    Console.ResetColor();

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
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            World.FrameRender();

            SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float deltaTime = (float)MathHelper.Clamp(e.Time, 0f, 1f);

            World.FrameUpdate(deltaTime);

            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }

            KeyboardState input = LastKeyboardState;

            if (hasReleasesWireframeKey && input.IsKeyDown(Key.K))
            {
                hasReleasesWireframeKey = false;

                if (wireframeMode)
                {
                    GL.LineWidth(1f);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    wireframeMode = false;
                }
                else
                {
                    GL.LineWidth(5f);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    wireframeMode = true;
                }
            }
            else if (input.IsKeyUp(Key.K))
            {
                hasReleasesWireframeKey = true;
            }

            if (input.IsKeyDown(Key.Escape))
            {
                Close();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            base.OnResize(e);
        }

        protected override void OnClosed()
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
                     "The process will be terminated, but some data may be lost.\n");
                Console.ResetColor();
            }

            World.Dispose();
            Player.Dispose();

            base.OnClosed();
        }

        private DebugProc debugCallbackDelegate = null!;

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