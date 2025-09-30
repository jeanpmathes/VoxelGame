// <copyright file="MetaController.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Responsible for handling all meta input in a <see cref="SessionScene" />.
/// </summary>
public class MetaController : SceneComponent, IConstructible<SessionScene, InGameUserInterface, MetaController>
{
    private readonly ToggleButton consoleToggle;
    private readonly PushButton escapeButton;
    private readonly SessionScene scene;
    private readonly InGameUserInterface ui;
    private readonly PushButton unlockMouseButton;

    private Boolean isMouseUnlockedByUserRequest;

    private MetaController(SessionScene scene, InGameUserInterface ui) : base(scene)
    {
        this.scene = scene;
        this.ui = ui;

        consoleToggle = scene.Client.Keybinds.GetToggle(scene.Client.Keybinds.Console);
        escapeButton = scene.Client.Keybinds.GetPushButton(scene.Client.Keybinds.Escape);
        unlockMouseButton = scene.Client.Keybinds.GetPushButton(scene.Client.Keybinds.UnlockMouse);

        OnSideliningEnd();

        ui.AnyMetaControlOpened += (_, _) => OnSideliningStart();
        ui.AnyMetaControlClosed += (_, _) => OnSideliningEnd();
    }

    /// <summary>
    ///     Get whether the actual game content is currently sidelined, meaning it is not the main focus of the user.
    ///     One reason for this could be that meta UI is shown on top of the game.
    ///     Note that this does not relate to whether the window is focused or not.
    ///     If sidelined, in-game input should not be handled.
    /// </summary>
    public Boolean IsSidelined { get; private set; }

    /// <inheritdoc />
    public static MetaController Construct(SessionScene input1, InGameUserInterface input2)
    {
        return new MetaController(input1, input2);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        if (!scene.Client.IsFocused)
            return;

        if (unlockMouseButton.Pushed)
        {
            if (isMouseUnlockedByUserRequest)
            {
                OnSideliningEnd();
            }
            else if (!IsSidelined)
            {
                OnSideliningStart();
                isMouseUnlockedByUserRequest = true;
            }
        }

        if (escapeButton.Pushed)
            ui.HandleEscape();

        if (consoleToggle.Changed)
            ui.ToggleConsole();
    }

    private void OnSideliningEnd()
    {
        IsSidelined = false;
        scene.Client.Input.Mouse.SetCursorLock(locked: true);

        isMouseUnlockedByUserRequest = false;
    }

    private void OnSideliningStart()
    {
        IsSidelined = true;
        scene.Client.Input.Mouse.SetCursorLock(locked: false);

        // The mouse was unlocked, but the user did not necessarily explicitly request it.
        isMouseUnlockedByUserRequest = false;
    }
}
