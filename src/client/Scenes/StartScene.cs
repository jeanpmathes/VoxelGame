// <copyright file="StartScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Logging;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes
{
    public class StartScene : IScene
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<StartScene>();

        private readonly Application.Client client;
        private readonly StartUserInterface ui;

        private List<(WorldInformation information, string path)> worlds;

        internal StartScene(Application.Client client)
        {
            this.client = client;

            ui = new StartUserInterface(client, drawBackground: true);

            worlds = new List<(WorldInformation information, string path)>();
        }

        public void Load()
        {
            Screen.SetCursor(visible: true);
            Screen.SetWireFrame(wireframe: false);

            ui.Load();
            ui.Resize(Screen.Size);

            ui.CreateControl();
            ui.SetActions(Action_Start, Action_Exit);

            LookupWorlds(client.worldsDirectory);
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

        #region ACTIONS

        private void Action_Start()
        {
            ListWorlds(worlds);
            client.LoadGameScene(WorldSetup(client.worldsDirectory));
        }

        private void Action_Exit()
        {
            client.Close();
        }

        #endregion ACTIONS

        #region WORLD SETUP

        private ClientWorld WorldSetup(string worldsDirectory)
        {
            using (logger.BeginScope("WorldSetup"))
            {
                bool newWorld = worlds.Count == 0 || NewWorldPrompt();

                return newWorld ? CreateNewWorld(worldsDirectory) : LoadExistingWorld(worlds);
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

                    logger.LogDebug(Events.WorldIO, "Valid world directory found: {Directory}", directory);
                }
                else
                {
                    logger.LogDebug(
                        Events.WorldIO,
                        "Directory has no meta file and is ignored: {Directory}",
                        directory);
                }
            }

            logger.LogInformation(
                Events.WorldIO,
                "Completed world lookup, found {Count} valid directories",
                worlds.Count);
        }

        private static void ListWorlds(List<(WorldInformation information, string path)> worlds)
        {
            Thread.Sleep(millisecondsTimeout: 100);

            if (worlds.Count > 0)
            {
                Console.WriteLine(Language.ListingWorlds);

                for (var n = 0; n < worlds.Count; n++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($@"{n + 1}: ");

                    Console.ForegroundColor = ConsoleColor.Cyan;

                    Console.Write(
                        $@"{worlds[n].information.Name} - {Language.CreatedOn}: {worlds[n].information.Creation}");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(@" [");

                    Console.ForegroundColor = worlds[n].information.Version == Program.Version
                        ? ConsoleColor.Green
                        : ConsoleColor.Red;

                    Console.Write(worlds[n].information.Version);

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(@"]");
                }

                Console.ResetColor();
                Console.WriteLine();
            }
        }

        private static bool NewWorldPrompt()
        {
            Console.WriteLine(Language.NewWorldPrompt + @" [y|skip: n]");

            Console.ForegroundColor = ConsoleColor.White;
            string input = Console.ReadLine()?.ToUpperInvariant() ?? "";
            Console.ResetColor();

            return input is "Y" or "YES";
        }

        private static ClientWorld CreateNewWorld(string worldsDirectory)
        {
            Console.WriteLine(Language.EnterNameOfWorld);

            string name;

            do
            {
                Console.ForegroundColor = ConsoleColor.White;
                name = Console.ReadLine() ?? "";
                Console.ResetColor();
            } while (!IsNameValid(name));

            StringBuilder path = new(Path.Combine(worldsDirectory, name));

            if (IsNameReserved(name)) path.Append(value: '_');

            while (Directory.Exists(path.ToString())) path.Append(value: '_');

            return new ClientWorld(name, path.ToString(), DateTime.Now.GetHashCode());
        }

        private static bool IsNameValid(string name)
        {
            if (name.Length == 0)
            {
                logger.LogWarning(Events.UserInteraction, "World name '{Name}' too short", name);
                Console.WriteLine(Language.InputNotValid);

                return false;
            }

            if (name[^1] == ' ')
            {
                logger.LogWarning(Events.UserInteraction, "World name '{Name}' ends with whitespace", name);
                Console.WriteLine(Language.InputNotValid);

                return false;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
                if (!CheckChar(c))
                    return false;

            foreach (char c in new[] {'.', ',', '{', '}'})
                if (!CheckChar(c))
                    return false;

            return true;

            bool CheckChar(char c)
            {
                if (name.Contains(c, StringComparison.Ordinal))
                {
                    logger.LogWarning(Events.UserInteraction, "World name '{Name}' contains invalid characters", name);
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

        private static ClientWorld LoadExistingWorld(List<(WorldInformation information, string path)> worlds)
        {
            while (true)
            {
                Console.WriteLine(Language.EnterIndexOfWorld);

                Console.ForegroundColor = ConsoleColor.White;
                string index = Console.ReadLine() ?? "";
                Console.ResetColor();

                if (int.TryParse(index, out int n))
                {
                    n--;

                    if (n >= 0 && n < worlds.Count) return new ClientWorld(worlds[n].information, worlds[n].path);

                    logger.LogWarning(Events.UserInteraction, "World index ({I}) too high or too low", n);
                    Console.WriteLine(Language.WorldNotFound);
                }
                else
                {
                    logger.LogWarning(
                        Events.UserInteraction,
                        "Input '{Input}' could not be parsed to an int value",
                        index);

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
                if (disposing) ui.Dispose();

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}