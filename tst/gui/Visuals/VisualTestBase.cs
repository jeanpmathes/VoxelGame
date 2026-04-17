// <copyright file="VisualTestBase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Tests.Controls;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Visuals;
using Xunit;
using Canvas = VoxelGame.GUI.Controls.Canvas;

namespace VoxelGame.GUI.Tests.Visuals;

public abstract class VisualTestBase<TVisual> where TVisual : Visual
{
    private readonly Func<TVisual> factory;

    protected readonly Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());

    protected VisualTestBase(Func<TVisual> factory)
    {
        this.factory = factory;

        canvas.SetRenderingSize(new Size(width: 200, height: 200));
    }

    protected MockVisual? FindVisual(String tag)
    {
        Visual? root = canvas.Visualization.GetValue();

        return root == null ? null : Find(root);

        MockVisual? Find(Visual start)
        {
            foreach (Visual child in start.Children)
            {
                if (child is MockVisual target && target.Tag == tag)
                    return target;

                if (Find(child) is {} found)
                    return found;
            }

            return null;
        }
    }

    protected ControlTemplate<MockControl> CreateTemplate(String tag)
    {
        return ControlTemplate.Create<MockControl>(_ => new MockVisual(tag));
    }

    protected MockControl CreateControl(String tag)
    {
        return new MockControl(tag) {Template = {Value = CreateTemplate(tag)}};
    }

    [Fact]
    public void Visual_CanBeUsed()
    {
        Boolean isCreated = false;

        canvas.Child = new MockFactoryControl(() =>
        {
            isCreated = true;
            return factory();
        });

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

        canvas.SetDebugOutlines(true);
        canvas.Render();

        Assert.True(isCreated);
    }

    private class MockFactoryControl(Func<TVisual> factory) : Control<MockFactoryControl>
    {
        protected override ControlTemplate<MockFactoryControl> CreateDefaultTemplate()
        {
            return ControlTemplate.Create<MockFactoryControl>(_ => factory());
        }
    }
}
