// <copyright file="FullscreenToggle.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.App;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Input.Actions;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Toggles the fullscreen mode of the client.
/// </summary>
public partial class FullscreenToggle : ApplicationComponent
{
    private readonly ToggleButton button;
    private readonly Client client;

    [Constructible]
    private FullscreenToggle(Client client) : base(client)
    {
        this.client = client;

        button = client.Keybinds.GetToggle(client.Keybinds.Fullscreen);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double delta, Timer? timer)
    {
        if (client.IsFocused && button.Changed) client.ToggleFullscreen();
    }
}
