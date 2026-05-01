// <copyright file="Border.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls.Bases;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Utilities;
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Controls;

/// <summary>
///     A <see cref="Border" /> draws a border and background around its child control.
/// </summary>
/// <seealso cref="Visuals.Border" />
public class Border : BorderBase<Border>
{
    /// <summary>
    ///     Create a new instance of the <see cref="Border" /> class.
    /// </summary>
    public Border()
    {
        BorderWidth = Property.Create(this, WidthF.One);
        BorderRadius = Property.Create(this, Defaults.Radius);
        BorderStrokeStyle = Property.Create(this, StrokeStyle.Solid);
    }

    /// <inheritdoc />
    protected override ControlTemplate<Border> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<Border>(control => new Visuals.Border
        {
            BorderWidth = {Binding = Binding.To(control.BorderWidth)},
            BorderRadius = {Binding = Binding.To(control.BorderRadius)},
            BorderStrokeStyle = {Binding = Binding.To(control.BorderStrokeStyle)},

            Child = new ChildPresenter()
        });
    }

    #region PROPERTIES

    /// <summary>
    ///     The width of the border drawn around the child control.
    /// </summary>
    public Property<WidthF> BorderWidth { get; }

    /// <summary>
    ///     The radius of the corners of the drawn border.
    /// </summary>
    public Property<RadiusF> BorderRadius { get; }

    /// <summary>
    ///     The stroke style of the border drawn around the child control.
    /// </summary>
    public Property<StrokeStyle> BorderStrokeStyle { get; }

    #endregion PROPERTIES
}
