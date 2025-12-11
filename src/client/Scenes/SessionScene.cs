// <copyright file="SessionScene.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors;
using VoxelGame.Client.Application.Components;
using VoxelGame.Client.Console;
using VoxelGame.Client.Inputs;
using VoxelGame.Client.Logic;
using VoxelGame.Client.Scenes.Components;
using VoxelGame.Client.Sessions;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Objects;
using VoxelGame.UI;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     The scene that is active when a session is played.
/// </summary>
public sealed class SessionScene : Scene, IInputControl
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly MetaController meta;

    private readonly EventHandler<FocusChangeEventArgs> onFocusChange;

    private Session? session;

    internal SessionScene(Application.Client client, World world, CommandInvoker commands, UserInterfaceResources uiResources, Engine engine) : base(client)
    {
        InGameUserInterface ui = CreateUI(client, uiResources);
        session = CreateSession(client.Space.Camera, world, ui, engine);

        SessionConsole console = session.AddComponent<SessionConsole, CommandInvoker>(commands);

        world.State.Activating += (_, _) =>
        {
            console.OnWorldReady();
        };

        AddComponent<SessionHook, Session>(session);
        AddComponent<UpdateInGamePerformanceData, InGameUserInterface>(ui);
        AddComponent<UserInterfaceHook, UserInterface>(ui);

        AddComponent<ScreenshotController, SessionScene>();
        AddComponent<UserInterfaceHide, InGameUserInterface, SessionScene>(ui);

        meta = AddComponent<MetaController, InGameUserInterface, SessionScene>(ui);

        SetUpUI(ui, world, console);

        onFocusChange = (_, _) =>
        {
            if (!Client.IsFocused) ui.HandleLossOfFocus();
        };
    }

    /// <summary>
    ///     Whether it is OK to handle game input currently.
    /// </summary>
    public Boolean CanHandleGameInput => !meta.IsSidelined && Client.IsFocused;

    /// <summary>
    ///     Whether it is OK to handle meta input currently.
    /// </summary>
    public Boolean CanHandleMetaInput => Client.IsFocused;

    /// <inheritdoc />
    public KeybindManager Keybinds => Client.Keybinds;

    /// <inheritdoc />
    protected override void OnLoad()
    {
        Client.FocusChanged += onFocusChange;
    }

    /// <inheritdoc />
    protected override void OnUnload()
    {
        Client.FocusChanged -= onFocusChange;

        session?.Dispose();
        session = null;
    }

    /// <inheritdoc />
    public override Boolean CanCloseWindow()
    {
        return false;
    }

    private static InGameUserInterface CreateUI(Application.Client client, UserInterfaceResources uiResources)
    {
        return new InGameUserInterface(
            client.Input,
            client.Settings,
            uiResources,
            drawBackground: false);
    }

    private Session CreateSession(Camera camera, World world, InGameUserInterface ui, Engine engine)
    {
        Player player = new(
            mass: 70.0,
            new BoundingVolume(new Vector3d(x: 0.25f, y: 0.9f, z: 0.25f)),
            camera,
            ui,
            engine,
            this);

        world.AddComponent<LocalPlayerHook, Player>(player);

        return new Session(world, player);
    }

    private void SetUpUI(InGameUserInterface ui, Core.Logic.World world, IConsoleProvider console)
    {
        Debug.Assert(session != null);

        List<SettingsProvider> settingsProviders =
        [
            SettingsProvider.Wrap(Client.Settings),
            SettingsProvider.Wrap(Client.Keybinds)
        ];

        ui.SetSettingsProviders(settingsProviders);
        ui.SetConsoleProvider(console);
        ui.SetPerformanceProvider(Client.GetRequiredComponent<CycleTracker>());
        ui.SetPlayerDataProvider(session.Player);

        ui.WorldSave += (_, _) =>
        {
            if (!world.State.IsActive) return;

            world.State.BeginSaving();
        };

        ui.WorldExit += (_, args) =>
        {
            if (!world.State.IsActive) return;

            world.State.BeginTerminating()?.Then(() => Client.ExitGame(args.ExitToOS));
        };
    }

    #region DISPOSABLE

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        session?.Dispose();
        session = null;
    }

    #endregion DISPOSABLE
}
