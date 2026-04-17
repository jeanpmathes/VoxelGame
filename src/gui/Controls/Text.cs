// <copyright file="Text.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Controls;

/// <summary>
///     Displays read-only text content.
/// </summary>
/// <seealso cref="Visuals.Text" />
public class Text : TextBase<Text>
{
    /// <inheritdoc />
    protected override ControlTemplate<Text> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<Text>(control => new Visuals.Text
        {
            FontFamily = {Binding = Binding.To(control.FontFamily)},
            FontSize = {Binding = Binding.To(control.FontSize)},
            FontStyle = {Binding = Binding.To(control.FontStyle)},
            FontWeight = {Binding = Binding.To(control.FontWeight)},
            FontStretch = {Binding = Binding.To(control.FontStretch)},

            Wrapping = {Binding = Binding.To(control.TextWrapping)},
            Alignment = {Binding = Binding.To(control.TextAlignment)},
            Trimming = {Binding = Binding.To(control.TextTrimming)},
            LineHeight = {Binding = Binding.To(control.LineHeight)},

            Content = {Binding = Binding.To(control.Content)}
        });
    }
}
