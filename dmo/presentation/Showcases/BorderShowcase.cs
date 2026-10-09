// <copyright file="BorderShowcase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Drawing;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.Presentation.Demo.Showcases;

internal class BorderShowcase : Showcase
{
    public override String Name => "Border";

    public override ContentTemplate GetTemplate()
    {
        return ContentTemplate.Create<BorderShowcase>(nameof(BorderShowcase),
            _ => new LinearLayout
            {
                Children =
                {
                    new Border
                    {
                        Margin = {Value = Thickness.One},
                        Child = new Text {Content = {Value = "Normal"}}
                    },
                    new Border
                    {
                        Margin = {Value = Thickness.One},
                        BorderRadius = {Value = Radius.Zero},
                        BorderWidth = {Value = new Width(20.0f)},
                        Child = new Text {Content = {Value = "Sharp & Thick"}}
                    },
                    new Border
                    {
                        Margin = {Value = Thickness.One},
                        BorderStrokeStyle = {Value = StrokeStyle.Dashed},
                        BorderWidth = {Value = new Width(5.0f)},
                        Child = new Text {Content = {Value = "Dashed"}}
                    },
                    new Border
                    {
                        Margin = {Value = Thickness.One},
                        BorderStrokeStyle = {Value = StrokeStyle.Squared},
                        BorderWidth = {Value = new Width(5.0f)},
                        Child = new Text {Content = {Value = "Squared"}}
                    },
                    new Border
                    {
                        Margin = {Value = Thickness.One},
                        BorderStrokeStyle = {Value = StrokeStyle.Dotted},
                        BorderWidth = {Value = new Width(5.0f)},
                        Child = new Text {Content = {Value = "Dotted"}}
                    }
                }
            });
    }
}
