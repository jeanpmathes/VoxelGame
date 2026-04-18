// <copyright file="TextTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Visuals;
using Xunit;
using Brush = VoxelGame.GUI.Graphics.Brush;
using Canvas = VoxelGame.GUI.Controls.Canvas;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.GUI.Tests.Visuals;

[TestSubject(typeof(Text))]
public class TextTests : VisualTestBase<Text>
{
    private readonly TrackingRenderer renderer = new();
    private new readonly Canvas canvas;

    public TextTests() : base(() => new Text())
    {
        canvas = Canvas.Create(renderer, new Theme());
    }

    [Fact]
    public void Text_ShouldCreateFormattedTextOnAttach()
    {
        canvas.Child = new GUI.Controls.Text();

        Assert.NotNull(renderer.LastCreatedText);
        Assert.False(renderer.LastCreatedText.IsDisposed);
    }

    [Fact]
    public void Text_ShouldDisposeFormattedTextOnDetach()
    {
        canvas.Child = new GUI.Controls.Text();

        TrackableFormattedText formattedText = renderer.LastCreatedText!;

        canvas.Child = null;

        Assert.True(formattedText.IsDisposed);
    }

    [Fact]
    public void Text_Content_ShouldDisposeFormattedTextOnContentChange()
    {
        GUI.Controls.Text control = new();
        canvas.Child = control;

        control.Content.Value = "Initial";

        TrackableFormattedText first = renderer.LastCreatedText!;

        control.Content.Value = "Updated";

        Assert.True(first.IsDisposed);
    }

    private sealed class TrackableFormattedText : IFormattedText
    {
        public Boolean IsDisposed { get; private set; }

        public SizeF Measure(SizeF availableSize)
        {
            return availableSize;
        }

        public void Draw(RectangleF rectangle, Brush brush) {}

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class TrackingRenderer : MockRenderer
    {
        public TrackableFormattedText? LastCreatedText { get; private set; }

        public override IFormattedText CreateFormattedText(String text, Font font, TextOptions options)
        {
            LastCreatedText = new TrackableFormattedText();

            return LastCreatedText;
        }
    }
}
