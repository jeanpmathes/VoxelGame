// <copyright file="SingleChildControl.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Controls.Internals;

/// <summary>
///     A control which has at most one child control. Setting the <see cref="Child" /> property will replace any existing
///     child control.
/// </summary>
/// <typeparam name="TControl">The concrete type of the control.</typeparam>
public abstract class SingleChildControl<TControl> : Control<TControl> where TControl : SingleChildControl<TControl>
{
    /// <summary>
    ///     Gets or sets the single child control.
    /// </summary>
    public Control? Child
    {
        get => Children.Count.GetValue() > 0 ? Children[0] : null;
        set => SetChild(value);
    }
}
