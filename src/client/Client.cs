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
using VoxelGame.Client.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Visuals;
using TextureLayout = VoxelGame.Core.Logic.TextureLayout;

namespace VoxelGame.Client
{
    internal class Client : GameWindow
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<Client>();
        private static Client Instance { get; set; } = null!;

        #region STATIC PROPERTIES

        public static KeyboardState Keyboard => Instance.KeyboardState;

        public static MouseState Mouse => Instance.MouseState;

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
        public static Shader VaryingHeightShader { get; private set; } = null!;
        public static Shader OpaqueLiquidSectionShader { get; private set; } = null!;
        public static Shader TransparentLiquidSectionShader { get; private set; } = null!;
        public static Shader OverlayShader { get; private set; } = null!;
        public static Shader SelectionShader { get; private set; } = null!;
        public static Shader ScreenElementShader { get; private set; } = null!;

        public static ClientWorld World { get; private set; } = null!;
        public static ClientPlayer Player { get; private set; } = null!;

        public static double Time { get; private set; }

        public static double Fps => 1.0 / Instance.renderDeltaBuffer.Average;
        public static double Ups => 1.0 / Instance.updateDeltaBuffer.Average;

        #endregion STATIC PROPERTIES

        public IScene Scene { get; private set; } = null!;
        private StartScene startScene = null!;

        public unsafe Window* WindowPointer { get; }

        public readonly string AppDataDirectory;
        public readonly string WorldsDirectory;
        public readonly string ScreenshotDirectory;

        private const int deltaBufferCapacity = 30;
        private readonly CircularTimeBuffer renderDeltaBuffer = new CircularTimeBuffer(deltaBufferCapacity);
        private readonly CircularTimeBuffer updateDeltaBuffer = new CircularTimeBuffer(deltaBufferCapacity);

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
            using (Logger.BeginScope("Client OnLoad"))
            {
                // GL version setup.
                int version = GL.GetInteger(GetPName.MajorVersion) * 10 + GL.GetInteger(GetPName.MinorVersion);
#if GL33
                version = 33;
#endif
                GLManager.Initialize(version);

                // GL debug setup.
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.Multisample);

                debugCallbackDelegate = new DebugProc(DebugCallback);
                GL.DebugMessageCallback(debugCallbackDelegate, IntPtr.Zero);

                // Screen setup.
                screen = GLManager.ScreenFactory.CreateScreen(this);

                // Texture setup.
                BlockTextureArray = GLManager.ArrayTextureFactory.CreateArrayTexture("Resources/Textures/Blocks", 16, true, TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3, TextureUnit.Texture4);
                Logger.LogInformation("All block textures loaded.");

                LiquidTextureArray = GLManager.ArrayTextureFactory.CreateArrayTexture("Resources/Textures/Liquids", 16, false, TextureUnit.Texture5);
                Logger.LogInformation("All liquid textures loaded.");

                TextureLayout.SetProviders(BlockTextureArray, LiquidTextureArray);
                BlockModel.SetBlockTextureIndexProvider(BlockTextureArray);

                // Shader setup.
                using (Logger.BeginScope("Shader setup"))
                {
                    SimpleSectionShader = new Shader("simplesection_shader.vert", "section_shader.frag");
                    ComplexSectionShader = new Shader("complexsection_shader.vert", "section_shader.frag");
                    VaryingHeightShader = new Shader("varyingheightsection_shader.vert", "section_shader.frag");
                    OpaqueLiquidSectionShader = new Shader("liquidsection_shader.vert", "opaqueliquidsection_shader.frag");
                    TransparentLiquidSectionShader = new Shader("liquidsection_shader.vert", "transparentliquidsection_shader.frag");
                    OverlayShader = new Shader("overlay_shader.vert", "overlay_shader.frag");
                    SelectionShader = new Shader("selection_shader.vert", "selection_shader.frag");
                    ScreenElementShader = new Shader("screenelement_shader.vert", "screenelement_shader.frag");

                    OverlayShader.SetMatrix4("projection", Matrix4.CreateOrthographic(1f, 1f / Screen.AspectRatio, 0f, 1f));
                    ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Size.X, Size.Y, 0f, 1f));

                    Logger.LogInformation("Shader setup complete.");
                }

                // Block setup.
                Block.LoadBlocks(BlockTextureArray);
                Logger.LogDebug("Texture/Block ratio: {ratio:F02}", BlockTextureArray.Count / (float)Block.Count);

                // Liquid setup.
                Liquid.LoadLiquids(LiquidTextureArray);

                // Scene setup.
                startScene = new StartScene(this);
                Scene = startScene;
                Scene.Load();

                Logger.LogInformation("Finished OnLoad");
            }
        }

        new protected void OnRenderFrame(FrameEventArgs e)
        {
            using (Logger.BeginScope("RenderFrame"))
            {
                Time += e.Time;

                SimpleSectionShader.SetFloat("time", (float)Time);
                ComplexSectionShader.SetFloat("time", (float)Time);
                OpaqueLiquidSectionShader.SetFloat("time", (float)Time);
                TransparentLiquidSectionShader.SetFloat("time", (float)Time);

                screen.Clear();

                Scene.Render((float)e.Time);

                screen.Draw();

                SwapBuffers();

                renderDeltaBuffer.Write(e.Time);
            }
        }

        private bool hasReleasedFullscreenKey = true;

        new protected void OnUpdateFrame(FrameEventArgs e)
        {
            using (Logger.BeginScope("UpdateFrame"))
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

                updateDeltaBuffer.Write(e.Time);
            }
        }

        new protected void OnClosed()
        {
            Logger.LogInformation("Closing window.");
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
        }

        public static void InvalidatePlayer()
        {
            Player = null!;
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
                    Logger.LogInformation(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityLow:
                    Logger.LogWarning(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityMedium:
                    Logger.LogError(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                case DebugSeverity.DebugSeverityHigh:
                    Logger.LogCritical(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;

                default:
                    Logger.LogInformation(eventId, "OpenGL Debug | Source: {source} | Type: {type} | Event: {event} | " +
                        "Message: {message}", sourceShort, typeShort, idResolved, System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length) ?? "NONE");
                    break;
            }
        }

        #endregion GL DEBUG
    }
}