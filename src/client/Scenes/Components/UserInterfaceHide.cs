// <copyright file="UserInterfaceHide.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Allows the user to hide or show the <see cref="InGameUserInterface" /> in a <see cref="SessionScene" />.
/// </summary>
public partial class UserInterfaceHide : SceneComponent
{
    private readonly ToggleButton button;
    private readonly SessionScene scene;
    private readonly InGameUserInterface ui;

    [Constructible]
    private UserInterfaceHide(SessionScene scene, InGameUserInterface ui) : base(scene)
    {
        this.scene = scene;
        this.ui = ui;

        button = scene.Client.Keybinds.GetToggle(scene.Client.Keybinds.UI);
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
