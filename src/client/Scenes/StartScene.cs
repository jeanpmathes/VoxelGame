// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Utilities;
using System;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Logic;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using VoxelGame.Client.Logic;
using VoxelGame.UI;
using VoxelGame.UI.Controls;

namespace VoxelGame.Client.Scenes
{
    public class StartScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<StartScene>();

        private readonly UserInterface ui;
        private StartControl control = null!;

        private List<(WorldInformation information, string path)> worlds;

        private readonly string worldsDirectory;

        public StartScene(string worldsDirectory)
        {
            ui = new UserInterface(Client.Instance, true);

            this.worldsDirectory = worldsDirectory;

            worlds = new List<(WorldInformation information, string path)>();
        }

        public void Load()
        {
            Screen.SetCursor(visible: true);

            ui.Load();
            ui.Resize(Client.Instance.Size);

            control = new StartControl(ui);
            control.Start += Control_Start;
            control.Exit += Control_Exit;

            LookupWorlds(worldsDirectory);
        }

        public void Update(float deltaTime)
        {
            // Method intentionally left empty.
        }

        public void OnResize(Vector2i size)
        {
            ui.Resize(size);
        }

        public void Render(float deltaTime)
        {
            ui.Render();
        }

        public void Unload()
        {
            // Method intentionally left empty.
        }

        #region EVENTS

        private void Control_Start()
        {
            ListWorlds(worlds);
            Client.LoadGameScene(WorldSetup(worldsDirectory));
        }

        private void Control_Exit()
        {
            Client.Instance.Close();
        }

        #endregion EVENTS

        #region WORLD SETUP

        private ClientWorld WorldSetup(string worldsDirectory)
        {
            using (logger.BeginScope("WorldSetup"))
            {
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
                    return CreateNewWorld(worldsDirectory);
                }
                else
                {
                    return LoadExistingWorld(worlds);
                }
            }
        }

        private void LookupWorlds(string worldsDirectory)
        {
            worlds = new List<(WorldInformation information, string path)>();

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

        private ClientWorld CreateNewWorld(string worldsDirectory)
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

            return new ClientWorld(name, path.ToString(), DateTime.Now.GetHashCode());
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

        private ClientWorld LoadExistingWorld(List<(WorldInformation information, string path)> worlds)
        {
            while (true)
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
                        return new ClientWorld(worlds[n].information, worlds[n].path);
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

        #region IDisposable Support

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

        #endregion IDisposable Support
    }
}