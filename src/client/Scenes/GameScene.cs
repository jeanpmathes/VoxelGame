// <copyright file="GameScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Logic;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using VoxelGame.UI;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Client.Rendering;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Logic;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace VoxelGame.Client.Scenes
{
    public class GameScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameScene>();

        private readonly GameUI ui;
        private readonly string screenshotDirectory;
        private readonly string worldsDirectory;

        public ClientWorld World { get; private set; } = null!;
        public ClientPlayer Player { get; private set; } = null!;

        private bool wireframeMode;
        private bool hasReleasesWireframeKey = true;

        private bool hasReleasedScreenshotKey = true;

        private bool hasReleasedFullscreenKey = true;

        public GameScene(string worldsDirectory, string screenshotDirectory)
        {
            ui = new GameUI(Client.Instance);
            this.worldsDirectory = worldsDirectory;
            this.screenshotDirectory = screenshotDirectory;
        }

        public void Load()
        {
            // World setup.
            WorldSetup(worldsDirectory);
            Game.SetWorld(World);

            // Player setup.
            Camera camera = new Camera(new Vector3());
            Player = new ClientPlayer(70f, 0.25f, camera, new Core.Physics.BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.25f, 0.9f, 0.25f)));
            Game.SetPlayer(Player);

            ui.Load();

            Game.ResetUpdate();

            logger.LogInformation("Loaded GameScene");
        }

        public void OnResize(Vector2i size)
        {
            ui.Resize(size);
        }

        public void Render(float deltaTime)
        {
            using (logger.BeginScope("GameScene Render"))
            {
                World.Render();

                ui.Render();
            }
        }

        public void Update(float deltaTime)
        {
            using (logger.BeginScope("GameScene Update"))
            {
                Game.IncrementUpdate();

                World.Update(deltaTime);

                if (!Client.Instance.IsFocused) // check to see if the window is focused
                {
                    return;
                }

                KeyboardState input = Client.Instance.LastKeyboardState;

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

                    Screen.SetFullscreen(!Client.Instance.IsFullscreen);
                }
                else if (input.IsKeyUp(Key.F11))
                {
                    hasReleasedFullscreenKey = true;
                }
            }
        }

        #region WORLD SETUP

        private void WorldSetup(string worldsDirectory)
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

        private void CreateNewWorld(string worldsDirectory)
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

            World = new ClientWorld(name, path.ToString(), DateTime.Now.GetHashCode());
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

        private void LoadExistingWorld(List<(WorldInformation information, string path)> worlds)
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
                        World = new ClientWorld(worlds[n].information, worlds[n].path);
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

        #region IDisposable Support.

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ui.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support.
    }
}