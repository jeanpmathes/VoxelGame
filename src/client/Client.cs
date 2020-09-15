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
using System.IO;
using VoxelGame.Core.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Client.Scenes;

namespace VoxelGame.Client
{
    internal class Client : GameWindow
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();
        private static Client Instance { get; set; } = null!;

        #region STATIC PROPERTIES

        public static KeyboardState Keyboard { get => Instance.KeyboardState; }

        public static MouseState Mouse { get => Instance.MouseState; }

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

        public static ClientWorld World { get; private set; } = null!;
        public static ClientPlayer Player { get; private set; } = null!;

        public static Random Random { get; private set; } = null!;

        public static double Time { get; private set; }

        #endregion STATIC PROPERTIES

        public IScene Scene { get; private set; } = null!;
        private StartScene startScene = null!;

        public unsafe Window* WindowPointer { get; }

        public readonly string AppDataDirectory;
        public readonly string WorldsDirectory;
        public readonly string ScreenshotDirectory;

        private Screen screen = null!;

        public Client(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, string appDataDirectory, string screenshotDirectory) : base(gameWindowSettings, nativeWindowSettings)
        {
            Instance = this;

            unsafe { WindowPointer = WindowPtr; }

            this.AppDataDirectory = appDataDirectory;
            this.ScreenshotDirectory = screenshotDirectory;

            WorldsDirectory = Path.Combine(appDataDirectory, "Worlds");
            Directory.CreateDirectory(WorldsDirectory);

            Load += OnLoad;

            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;
            UpdateFrame += MouseUpdate;

            Closed += OnClosed;

            MouseMove += OnMouseMove;
        }

        new protected void OnLoad()
        {
            using (logger.BeginScope("Client OnLoad"))
            {
                // GL debug setup.
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.Multisample);

                debugCallbackDelegate = new DebugProc(DebugCallback);
                GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);

                // Screen setup.
                screen = new Screen(this);

                // Texture setup.
                BlockTextureArray = new ArrayTexture("Resources/Textures/Blocks", 16, true, TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3, TextureUnit.Texture4);
                Game.SetBlockTextures(BlockTextureArray);
                logger.LogInformation("All block textures loaded.");

                LiquidTextureArray = new ArrayTexture("Resources/Textures/Liquids", 16, false, TextureUnit.Texture5);
                Game.SetLiquidTextures(LiquidTextureArray);
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

                // Scene setup.
                startScene = new StartScene(this);
                Scene = startScene;
                Scene.Load();

                // Other object setup.
                Random = new Random();
                Game.SetRandom(Random);

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

                Scene.Render((float)e.Time);

                screen.Draw();

                SwapBuffers();
            }
        }

        private bool hasReleasedFullscreenKey = true;

        new protected void OnUpdateFrame(FrameEventArgs e)
        {
            using (logger.BeginScope("UpdateFrame"))
            {
                float deltaTime = (float)MathHelper.Clamp(e.Time, 0f, 1f);

                Scene.Update(deltaTime);

                if (IsFocused)
                {
                    KeyboardState input = Client.Instance.LastKeyboardState;

                    if (hasReleasedFullscreenKey && input.IsKeyDown(Key.F11))
                    {
                        hasReleasedFullscreenKey = false;

                        Screen.SetFullscreen(!Client.Instance.IsFullscreen);
                    }
                    else if (input.IsKeyUp(Key.F11))
                    {
                        hasReleasedFullscreenKey = true;
                    }
                }
            }
        }

        new protected void OnClosed()
        {
            logger.LogInformation("Closing window.");
        }

        #region SCENE MANAGEMENT

        public static void LoadGameScene(ClientWorld world)
        {
            Instance.Scene?.Unload();
            Instance.Scene?.Dispose();

            GameScene gameScene = new GameScene(Instance, world);
            Instance.Scene = gameScene;
            Instance.Scene.Load();

            World = gameScene.World;
            Player = gameScene.Player;
        }

        public static void LoadStartScene()
        {
            Instance.Scene?.Unload();
            Instance.Scene?.Dispose();

            Instance.Scene = Instance.startScene;
            Instance.Scene.Load();
        }

        public static void InvalidateWorld()
        {
            World = null!;
            Game.SetWorld(null!);
        }

        public static void InvalidatePlayer()
        {
            Player = null!;
            Game.SetPlayer(null!);
        }

        #endregion SCENE MANAGEMENT

        #region MOUSE MOVE

        public static Vector2 SmoothMouseDelta { get; private set; }
        public bool DoMouseTracking { get; set; }

        private Vector2 lastMouseDelta;
        private Vector2 rawMouseDelta;
        private Vector2 mouseDelta;

        private Vector2 mouseCorrection;
        private bool mouseHasMoved;

        new protected void OnMouseMove(MouseMoveEventArgs e)
        {
            if (!DoMouseTracking) return;

            mouseHasMoved = true;

            Vector2 center = new Vector2(Size.X / 2f, Size.Y / 2f);

            rawMouseDelta += e.Delta;
            mouseCorrection += center - MousePosition;

            MousePosition = center;
        }

        private void MouseUpdate(FrameEventArgs e)
        {
            if (!DoMouseTracking) return;

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