// <copyright file="ToggleButton.cs" company="VoxelGame">
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

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Inputs.Internals;

namespace VoxelGame.Client.Inputs.Actions;

/// <summary>
///     Represents an action that switches between enabled and disabled whenever its input combination is pressed.
///     Use this for actions that should remain enabled after the input combination is released, until the button is
///     toggled again.
/// </summary>
public partial class ToggleButton : Button
{
    [Constructible]
    private ToggleButton(Binding binding) : base(binding) {}

    /// <summary>
    ///     Get whether presses received for the most recent update changed <see cref="Button.IsActive" />.
    /// </summary>
    public Boolean Changed { get; private set; }

    /// <inheritdoc />
    internal override void Apply(ActionStateChange change)
    {
        Changed = (change.PressCount & 1) != 0;

        if (Changed)
            IsActive = !IsActive;
    }
}
