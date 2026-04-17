// <copyright file="LinearLayoutBase.cs" company="VoxelGame">
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

using VoxelGame.GUI.Bindings;

namespace VoxelGame.GUI.Controls.Bases;

/// <summary>
///     Abstract base class for linear layout controls, which arrange their children in a single line, either horizontally
///     or vertically.
/// </summary>
/// <typeparam name="TControl">The concrete type of the control.</typeparam>
public abstract class LinearLayoutBase<TControl> : LayoutBase<TControl> where TControl : LinearLayoutBase<TControl>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="LinearLayoutBase{TControl}" /> class.
    /// </summary>
    protected LinearLayoutBase()
    {
        Orientation = Property.Create(this, GUI.Orientation.Horizontal);
    }

    #region PROPERTIES

    /// <summary>
    ///     The orientation of the layout, which determines whether the children are arranged horizontally or vertically.
    /// </summary>
    public Property<Orientation> Orientation { get; }

    #endregion PROPERTIES
}
