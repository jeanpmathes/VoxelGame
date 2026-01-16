// <copyright file="UserInterfaceHide.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Input.Actions;
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
    public override void OnLogicUpdate(Delta delta, Timer? timer)
    {
        if (scene.CanHandleGameInput && button.Changed) ui.ToggleHidden();
    }
}
