// <copyright file="LinearLayoutShowcase.cs" company="VoxelGame">
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
using VoxelGame.GUI;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Drawing.Brushes;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.Presentation.Demo.Showcases;

internal class LinearLayoutShowcase : Showcase
{
    public override String Name => "Linear Layout";

    public override ContentTemplate GetTemplate()
    {
        return ContentTemplate.Create<LinearLayoutShowcase>(nameof(LinearLayoutShowcase),
            _ => new LinearLayout
            {
                Orientation = {Value = Orientation.Vertical},

                Children =
                {
                    new Text {Content = {Value = "Horizontal:"}},
                    new LinearLayout
                    {
                        Orientation = {Value = Orientation.Horizontal},

                        Children =
                        {
                            CreateBlock(),
                            CreateBlock(),
                            CreateBlock()
                        }
                    },
                    new Text {Content = {Value = "Vertical:"}},
                    new LinearLayout
                    {
                        Orientation = {Value = Orientation.Vertical},

                        Children =
                        {
                            CreateBlock(),
                            CreateBlock(),
                            CreateBlock()
                        }
                    }
                }
            });
    }

    private static Border CreateBlock()
    {
        return new Border
        {
            Foreground = {Value = Brush.Black},
            Background = {Value = Brush.White},

            MinimumWidth = {Value = 25},
            MinimumHeight = {Value = 25},

            Margin = {Value = Thickness.One}
        };
    }
}
