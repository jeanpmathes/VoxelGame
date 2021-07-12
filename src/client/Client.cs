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
using System.IO;
using VoxelGame.Client.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Logging;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
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

        public static ClientPlayer Player { get; private set; } = null!;

        public static double Fps => 1.0 / Instance.renderDeltaBuffer.Average;
        public static double Ups => 1.0 / Instance.updateDeltaBuffer.Average;

        #endregion STATIC PROPERTIES

        private readonly Graphics.Debug glDebug;
        private readonly SceneManager sceneManager;

        private double Time { get; set; }
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
            glDebug = new Graphics.Debug();

            this.AppDataDirectory = appDataDirectory;
            this.ScreenshotDirectory = screenshotDirectory;

            WorldsDirectory = Path.Combine(appDataDirectory, "Worlds");
            Directory.CreateDirectory(WorldsDirectory);

            sceneManager = new SceneManager();

            Load += OnLoad;

            RenderFrame += OnRenderFrame;
            UpdateFrame += OnUpdateFrame;
            UpdateFrame += MouseUpdate;

            Closed += OnClosed;

            MouseMove += OnMouseMove;
        }

        private new void OnLoad()
        {
            using (Logger.BeginScope("Client OnLoad"))
            {
                // GL debug setup.
                glDebug.Enable();

                // Screen setup.
                screen = new Screen(this);

                // Texture setup.
                BlockTextureArray = new ArrayTexture("Resources/Textures/Blocks", 16, true, TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3, TextureUnit.Texture4);
                Logger.LogInformation("All block textures loaded.");

                LiquidTextureArray = new ArrayTexture("Resources/Textures/Liquids", 16, false, TextureUnit.Texture5);
                Logger.LogInformation("All liquid textures loaded.");

                TextureLayout.SetProviders(BlockTextureArray, LiquidTextureArray);
                BlockModel.SetBlockTextureIndexProvider(BlockTextureArray);

                // Shader setup.
                Shaders.Load("Resources/Shaders");

                // Block setup.
                Block.LoadBlocks(BlockTextureArray);
                Logger.LogDebug("Texture/Block ratio: {ratio:F02}", BlockTextureArray.Count / (float)Block.Count);

                // Liquid setup.
                Liquid.LoadLiquids(LiquidTextureArray);

                // Scene setup.
                sceneManager.Load(new StartScene(this));

                Logger.LogInformation("Finished OnLoad");
            }
        }

        private new void OnRenderFrame(FrameEventArgs e)
        {
            using (Logger.BeginScope("RenderFrame"))
            {
                Time += e.Time;

                Shaders.SimpleSection.SetFloat("time", (float)Time);
                Shaders.ComplexSection.SetFloat("time", (float)Time);
                Shaders.OpaqueLiquidSection.SetFloat("time", (float)Time);
                Shaders.TransparentLiquidSection.SetFloat("time", (float)Time);

                screen.Clear();

                sceneManager.Render((float)e.Time);

                screen.Draw();

                SwapBuffers();

                renderDeltaBuffer.Write(e.Time);
            }
        }

        private bool hasReleasedFullscreenKey = true;

        private new void OnUpdateFrame(FrameEventArgs e)
        {
            using (Logger.BeginScope("UpdateFrame"))
            {
                var deltaTime = (float)MathHelper.Clamp(e.Time, 0f, 1f);

                sceneManager.Update(deltaTime);

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

        private new void OnClosed()
        {
            Logger.LogInformation("Closing window.");

            sceneManager.Unload();
        }

        #region SCENE MANAGEMENT

        public void LoadGameScene(ClientWorld world)
        {
            GameScene gameScene = new GameScene(Instance, world);

            sceneManager.Load(gameScene);

            Player = gameScene.Player;
        }

        public void LoadStartScene()
        {
            sceneManager.Load(new StartScene(Instance));

            Player = null!;
        }

        public void OnResize(Vector2i size)
        {
            sceneManager.OnResize(size);
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
    }
}