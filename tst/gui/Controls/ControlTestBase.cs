// <copyright file="ControlTestBase.cs" company="VoxelGame">
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
using System.Drawing;
using System.Linq;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Themes;
using Xunit;

namespace VoxelGame.GUI.Tests.Controls;

public abstract class ControlTestBase<TControl>(Func<TControl> factory) where TControl : Control
{
    [Fact]
    public void Control_ShouldNotThrowWhenUsed()
    {
        using Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());

        canvas.Child = factory();

        canvas.SetRenderingSize(new Size(width: 1000, height: 1000));
        canvas.Render();

        canvas.SetRenderingSize(new Size(width: 0, height: 0));
        canvas.Render();

        canvas.SetRenderingSize(new Size(width: 1, height: 1));
        canvas.Render();

        canvas.SetRenderingSize(new Size(width: 5000, height: 5000));
        canvas.Render();

        canvas.SetScale(0.5f);
        canvas.Render();

        canvas.SetScale(2.39f);
        canvas.Render();

        Assert.True(canvas.Children.OfType<TControl>().Any());
    }
}
