// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using VoxelGame.Entities;
using VoxelGame.Logic;
using VoxelGame.Rendering;
using VoxelGame.Resources.Language;

namespace VoxelGame
{
    internal class Game : GameWindow
    {
        private static readonly ILogger logger = Program.CreateLogger<Game>();

        #region STATIC PROPERTIES

        public static Game Instance { get; private set; } = null!;

        /// <summary>
        /// The number of the current update cycle. It is incremented every time a new cycle begins.
        /// </summary>
        public static long CurrentUpdate { get; private set; }

        /// <summary>
        /// Gets the <see cref="ArrayTexture"/> that contains all block textures. It is bound to unit 1, 2, 3, and 4.
        /// </summary>
        public static ArrayTexture BlockTextureArray { get; private set; } = null!;

        /// <summary>
        /// Gets the <see cref="ArrayTexture"/> that contains all liquid textures. It is bound to unit 5.
        /// </summary>
        public static ArrayTexture LiquidTextureArray { get; private set; } = null!;

        public static Shader SimpleSectionShader { get; private set; } = null!;
        public static Shader ComplexSectionShader { get; private set; } = null!;
        public static Shader LiquidSectionShader { get; private set; } = null!;
        public static Shader SelectionShader { get; private set; } = null!;
        public static Shader ScreenElementShader { get; private set; } = null!;

        public static World World { get; set; } = null!;
        public static Player Player { get; private set; } = null!;

        public static Random Random { get; private set; } = null!;

        public static double Time { get; private set; }

        #endregion STATIC PROPERTIES

        public unsafe Window* WindowPointer { get; }

        private readonly string appDataDirectory;
        private readonly string screenshotDirectory;

        private Screen screen = null!;

        private bool wireframeMode;
        private bool hasReleasesWireframeKey = true;

        private bool hasReleasedScreenshotKey = true;

        private bool hasReleasedFullscreenKey = true;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, string appDataDirectory, string screenshotDirectory) : base(gameWindowSettings, nativeWindowSettings)
        {
            Instance = this;

            unsafe { WindowPointer = WindowPtr; }

            this.appDataDirectory = appDataDirectory;
            this.screenshotDirectory = screenshotDirectory;

            Load += OnLoad;

            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;
            UpdateFrame += MouseUpdate;

            Closed += OnClosed;

            MouseMove += OnMouseMove;

            CursorVisible = false;
        }

        new protected void OnLoad()
        {
            using (logger.BeginScope("Game OnLoad"))
            {
                // GL debug setup.
                GL.Enable(EnableCap.DebugOutput);

                debugCallbackDelegate = new DebugProc(DebugCallback);
                GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);

                // Screen setup.
                screen = new Screen();

                // Texture setup.
                BlockTextureArray = new ArrayTexture("Resources/Textures/Blocks", 16, true, TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3, TextureUnit.Texture4);
                logger.LogInformation("All block textures loaded.");

                LiquidTextureArray = new ArrayTexture("Resources/Textures/Liquids", 16, false, TextureUnit.Texture5);
                logger.LogInformation("All liquid textures loaded.");

                // Shader setup.
                using (logger.BeginScope("Shader setup"))
                {
                    SimpleSectionShader = new Shader("Resources/Shaders/simplesection_shader.vert", "Resources/Shaders/section_shader.frag");
                    ComplexSectionShader = new Shader("Resources/Shaders/complexsection_shader.vert", "Resources/Shaders/section_shader.frag");
                    LiquidSectionShader = new Shader("Resources/Shaders/liquidsection_shader.vert", "Resources/Shaders/liquidsection_shader.frag");
                    SelectionShader = new Shader("Resources/Shaders/selection_shader.vert", "Resources/Shaders/selection_shader.frag");
                    ScreenElementShader = new Shader("Resources/Shaders/screenelement_shader.vert", "Resources/Shaders/screenelement_shader.frag");

                    ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Size.X, Size.Y, 0f, 1f));

                    logger.LogInformation("Shader setup complete.");
                }

                // Block setup.
                Block.LoadBlocks();
                logger.LogDebug("Texture/Block ratio: {ratio:F02}", BlockTextureArray.Count / (float)Block.Count);

                // Liquid setup.
                Liquid.LoadLiquids();

                // Create required folders in %appdata% directory
                string worldsDirectory = Path.Combine(appDataDirectory, "Worlds");
                Directory.CreateDirectory(worldsDirectory);

                WorldSetup(worldsDirectory);

                // Player setup.
                Camera camera = new Camera(new Vector3());
                Player = new Player(70f, 0.25f, camera, new Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.25f, 0.9f, 0.25f)));

                // Other object setup.
                Random = new Random();

                logger.LogInformation("Finished OnLoad");
            }
        }

        new protected void OnRenderFrame(FrameEventArgs e)
        {
            using (logger.BeginScope("RenderFrame"))
            {
                Time += e.Time;

                SimpleSectionShader.SetFloat("time", (float)Time);
                ComplexSectionShader.SetFloat("time", (float)Time);
                LiquidSectionShader.SetFloat("time", (float)Time);

                screen.Clear();

                World.Render();

                screen.Draw();

                SwapBuffers();
            }
        }

        new protected void OnUpdateFrame(FrameEventArgs e)
        {
            CurrentUpdate++;

            using (logger.BeginScope("UpdateFrame"))
            {
                float deltaTime = (float)MathHelper.Clamp(e.Time, 0f, 1f);

                World.Update(deltaTime);

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

                        logger.LogInformation("Disabled wireframe mode.");
                    }
                    else
                    {
                        GL.LineWidth(5f);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        wireframeMode = true;

                        logger.LogInformation("Enabled wireframe mode.");
                    }
                }
                else if (input.IsKeyUp(Key.K))
                {
                    hasReleasesWireframeKey = true;
                }

                if (hasReleasedScreenshotKey && input.IsKeyDown(Key.F12))
                {
                    hasReleasedScreenshotKey = false;

                    Screen.TakeScreenshot(screenshotDirectory);
                }
                else if (input.IsKeyUp(Key.F12))
                {
                    hasReleasedScreenshotKey = true;
                }

                if (hasReleasedFullscreenKey && input.IsKeyDown(Key.F11))
                {
                    hasReleasedFullscreenKey = false;

                    Screen.SetFullscreen(!IsFullscreen);
                }
                else if (input.IsKeyUp(Key.F11))
                {
                    hasReleasedFullscreenKey = true;
                }

                if (input.IsKeyDown(Key.Escape))
                {
                    Close();
                }
            }
        }

        new protected void OnClosed()
        {
            logger.LogInformation("{method} has been called.", nameof(OnClosed));

            try
            {
                World.Save().Wait();
            }
            catch (AggregateException exception)
            {
                logger.LogCritical(LoggingEvents.WorldSavingError, exception.GetBaseException(), "An exception was thrown when saving the world.");
            }

            World.Dispose();
            Player.Dispose();

            logger.LogInformation("Closing window.");
        }

        #region WORLD SETUP

        private static void WorldSetup(string worldsDirectory)
        {
            using (logger.BeginScope("WorldSetup"))
            {
                List<(WorldInformation information, string path)> worlds = WorldLookup(worldsDirectory);
                ListWorlds(worlds);

                bool newWorld;

                if (worlds.Count == 0)
                {
                    logger.LogInformation("Skipping new world prompt as no worlds are available to load.");

                    newWorld = true;
                }
                else
                {
                    newWorld = NewWorldPrompt();
                }

                if (newWorld)
                {
                    CreateNewWorld(worldsDirectory);
                }
                else
                {
                    LoadExistingWorld(worlds);
                }
            }
        }

        private static List<(WorldInformation information, string path)> WorldLookup(string worldsDirectory)
        {
            List<(WorldInformation information, string path)> worlds = new List<(WorldInformation information, string path)>();

            foreach (string directory in Directory.GetDirectories(worldsDirectory))
            {
                string meta = Path.Combine(directory, "meta.json");

                if (File.Exists(meta))
                {
                    WorldInformation information = WorldInformation.Load(meta);
                    worlds.Add((information, directory));

                    logger.LogDebug("Valid world directory found: {directory}", directory);
                }
                else
                {
                    logger.LogDebug("The directory has no meta file and is ignored: {directory}", directory);
                }
            }

            logger.LogInformation("Completed world lookup, {Count} valid directories have been found.", worlds.Count);

            return worlds;
        }

        private static void ListWorlds(List<(WorldInformation information, string path)> worlds)
        {
            Thread.Sleep(100);

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
                Console.WriteLine();
            }
        }

        private static bool NewWorldPrompt()
        {
            Console.WriteLine(Language.NewWorldPrompt + " [y|skip: n]");

            Console.ForegroundColor = ConsoleColor.White;
            string input = Console.ReadLine().ToUpperInvariant();
            Console.ResetColor();

            return input == "Y" || input == "YES";
        }

        private static void CreateNewWorld(string worldsDirectory)
        {
            Console.WriteLine(Language.EnterNameOfWorld);

            string name;

            do
            {
                Console.ForegroundColor = ConsoleColor.White;
                name = Console.ReadLine();
                Console.ResetColor();
            }
            while (!IsNameValid(name));

            StringBuilder path = new StringBuilder(Path.Combine(worldsDirectory, name));

            if (IsNameReserved(name))
            {
                path.Append('_');
            }

            while (Directory.Exists(path.ToString()))
            {
                path.Append('_');
            }

            World = new World(name, path.ToString(), DateTime.Now.GetHashCode());
        }

        private static bool IsNameValid(string name)
        {
            if (name.Length == 0)
            {
                logger.LogWarning("The input is too short.");

                Console.WriteLine(Language.InputNotValid);

                return false;
            }

            if (name[^1] == ' ')
            {
                logger.LogWarning("The input ends with a whitespace.");

                Console.WriteLine(Language.InputNotValid);

                return false;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (!CheckChar(c)) return false;
            }

            foreach (char c in new char[] { '.', ',', '{', '}' })
            {
                if (!CheckChar(c)) return false;
            }

            return true;

            bool CheckChar(char c)
            {
                if (name.Contains(c, StringComparison.Ordinal))
                {
                    logger.LogWarning("The input contains an invalid character.");

                    Console.WriteLine(Language.InputNotValid);

                    return false;
                }

                return true;
            }
        }

        private static bool IsNameReserved(string name)
        {
            switch (name)
            {
                case "CON":
                case "PRN":
                case "AUX":
                case "NUL":
                case "COM":
                case "COM0":
                case "COM1":
                case "COM2":
                case "COM3":
                case "COM4":
                case "COM5":
                case "COM6":
                case "COM7":
                case "COM8":
                case "COM9":
                case "LPT0":
                case "LPT1":
                case "LPT2":
                case "LPT3":
                case "LPT4":
                case "LPT5":
                case "LPT6":
                case "LPT7":
                case "LPT8":
                case "LPT9":
                    return true;

                default:
                    return false;
            }
        }

        private static void LoadExistingWorld(List<(WorldInformation information, string path)> worlds)
        {
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
                        logger.LogWarning("The index ({i}) is too high or too low.", n);

                        Console.WriteLine(Language.WorldNotFound);
                    }
                }
                else
                {
                    logger.LogWarning("The input ({input}) could not be parsed to an int value.", index);

                    Console.WriteLine(Language.InputNotValid);
                }
            }
        }

        #endregion WORLD SETUP

        #region MOUSE MOVE

        public static Vector2 SmoothMouseDelta { get; private set; }

        private Vector2 lastMouseDelta;
        private Vector2 rawMouseDelta;
        private Vector2 mouseDelta;

        private Vector2 mouseCorrection;
        private bool mouseHasMoved;

        new protected void OnMouseMove(MouseMoveEventArgs e)
        {
            mouseHasMoved = true;

            Vector2 center = new Vector2(Size.X / 2f, Size.Y / 2f);

            rawMouseDelta += e.Delta;
            mouseCorrection += center - MousePosition;

            MousePosition = center;
        }

        private void MouseUpdate(FrameEventArgs e)
        {
            if (!mouseHasMoved)
            {
                mouseDelta = Vector2.Zero;
            }
            else
            {
                const float a = 0.4f;

                mouseDelta = rawMouseDelta - mouseCorrection;
                mouseDelta = (lastMouseDelta * (1f - a)) + (mouseDelta * a);
            }

            SmoothMouseDelta = mouseDelta;
            mouseHasMoved = false;

            lastMouseDelta = mouseDelta;
            rawMouseDelta = Vector2.Zero;
            mouseCorrection = Vector2.Zero;
        }

        #endregion MOUSE MOVE

        #region GL DEBUG

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1450:Private fields only used as local variables in methods should become local variables", Justification = "Has to be field to prevent GC collection.")]
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
            int eventId = 0;
            switch (id)
            {
                case 0x500:
                    idResolved = "GL_INVALID_ENUM";
                    eventId = LoggingEvents.GlInvalidEnum;
                    break;

                case 0x501:
                    idResolved = "GL_INVALID_VALUE";
                    eventId = LoggingEvents.GlInvalidValue;
                    break;

                case 0x502:
                    idResolved = "GL_INVALID_OPERATION";
                    eventId = LoggingEvents.GlInvalidOperation;
                    break;

                case 0x503:
                    idResolved = "GL_STACK_OVERFLOW";
                    eventId = LoggingEvents.GlStackOverflow;
                    break;

                case 0x504:
                    idResolved = "GL_STACK_UNDERFLOW";
                    eventId = LoggingEvents.GlStackUnderflow;
                    break;

                case 0x505:
                    idResolved = "GL_OUT_OF_MEMORY";
                    eventId = LoggingEvents.GlOutOfMemory;
                    break;

                case 0x506:
                    idResolved = "GL_INVALID_FRAMEBUFFER_OPERATION";
                    eventId = LoggingEvents.GlInvalidFramebufferOperation;
                    break;

                case 0x507:
                    idResolved = "GL_CONTEXT_LOST";
                    eventId = LoggingEvents.GlContextLost;
                    break;
            }

            switch (severity)
            {
                case DebugSeverity.DebugSeverityNotification:
                    logger.LogInformation(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityLow:
                    logger.LogWarning(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityMedium:
                    logger.LogError(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityHigh:
                    logger.LogCritical(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                default:
                    logger.LogInformation(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;
            }
        }

        #endregion GL DEBUG
    }
}