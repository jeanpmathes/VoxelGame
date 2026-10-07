// <copyright file="ButtonShowcase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Commands;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;

namespace VoxelGame.Presentation.Demo.Showcases;

internal class ButtonShowcase : Showcase
{
    public ButtonShowcase()
    {
        Counter = new Slot<Int32>(value: 0, this);
    }

    private Slot<Int32> Counter { get; }

    public override String Name => "Buttons";

    public override ContentTemplate GetTemplate()
    {
        return ContentTemplate.Create<ButtonShowcase>(nameof(ButtonShowcase),
            showcase => new LinearLayout
            {
                Children =
                {
                    new Button<String>
                    {
                        Content = {Value = "Click me!"},
                        Command = {Value = Command.FromAction(showcase.OnButtonClick)}
                    },
                    new Text
                    {
                        Content = {Binding = Binding.To(showcase.Counter).Compute(value => $"Counter: {value}")}
                    },
                    new Button<String>
                    {
                        Content = {Value = "You can't click me!"}
                    }

                    // todo: also needs a showcase that shows buttons can contain any content
                }
            });
    }

    private void OnButtonClick()
    {
        Counter.SetValue(Counter.GetValue() + 1);
    }
}
