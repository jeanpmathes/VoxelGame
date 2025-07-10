// <copyright file="UserInterfaceHide.cs" company="VoxelGame">
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
/// Allows the user to hide or show the <see cref="InGameUserInterface"/> in a <see cref="SessionScene"/>.
/// </summary>
public class UserInterfaceHide : SceneComponent, IConstructible<SessionScene, InGameUserInterface, UserInterfaceHide>
{
    private readonly SessionScene scene;
    private readonly InGameUserInterface ui;

    private readonly ToggleButton button;

    private UserInterfaceHide(SessionScene scene, InGameUserInterface ui) : base(scene)
    {
        this.scene = scene;
        this.ui = ui;

        button = scene.Client.Keybinds.GetToggle(scene.Client.Keybinds.UI);
    }

    /// <inheritdoc />
    public static UserInterfaceHide Construct(SessionScene input1, InGameUserInterface input2)
    {
        return new UserInterfaceHide(input1, input2);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        if (scene.CanHandleGameInput && button.Changed)
        {
            ui.ToggleHidden();
        }
    }
}
