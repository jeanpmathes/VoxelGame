// <copyright file="TextShowcase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Texts;

namespace VoxelGame.Presentation.Demo.Showcases;

internal class TextShowcase : Showcase
{
    public override String Name => "Text";

    public override ContentTemplate GetTemplate()
    {
        return ContentTemplate.Create<TextShowcase>(nameof(TextShowcase),
            showcase => new LinearLayout
            {
                Children =
                {
                    new Border
                    {
                        Child = new Text {Content = {Value = "DEFAULT TEXT\nDEFAULT TEXT"}}
                    },
                    new Border
                    {
                        Child = new Text
                        {
                            Content = {Value = "Large BOLD Italic EXPANDED\nwith more line height"},

                            FontSize = {Value = 25},
                            FontWeight = {Value = Weight.Bold},
                            FontStyle = {Value = Style.Italic},
                            FontStretch = {Value = Stretch.Expanded},
                            LineHeight = {Value = 2.0f}
                        }
                    }
                }
            });
    }
}
