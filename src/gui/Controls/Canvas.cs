// <copyright file="Canvas.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls.Internals;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Rendering;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.GUI.Controls;

/// <summary>
///     The root control for a user interface.
/// </summary>
/// <seealso cref="Visuals.Canvas" />
public sealed class Canvas : SingleChildControl<Canvas>, IDisposable
{
    private readonly IRenderer onlyRenderer;

    private SizeF viewportSize = SizeF.Empty;

    private Canvas(IRenderer renderer, Theme theme)
    {
        onlyRenderer = renderer;

        Input = Binding.To(Visualization).Cast<Visuals.Canvas>().Select(canvas => canvas?.Input, defaultValue: null);

        Context = new Context(theme, this);

        Background.OverrideDefault(Defaults.BackgroundBrush);
    }

    /// <summary>
    ///     Get the input handler for this canvas, or null if the canvas is not visualized.
    /// </summary>
    public IValueSource<InputRoot?> Input { get; }

    /// <summary>
    ///     Get the current scale of the canvas.
    ///     Use <see cref="SetScale" /> to change the scale.
    /// </summary>
    public Single Scale { get; private set; } = 1.0f;

    /// <inheritdoc />
    public void Dispose()
    {
        if (Child != null)
            RemoveChild(Child);
    }

    /// <summary>
    ///     Create a new canvas with the given renderer and registry.
    /// </summary>
    /// <param name="renderer">
    ///     The only renderer which will be used for rendering this canvas.
    ///     Will not be disposed by the canvas.
    /// </param>
    /// <param name="theme">
    ///     The theme to create the canvas context with.
    /// </param>
    /// <returns>A new canvas instance.</returns>
    public static Canvas Create(IRenderer renderer, Theme theme)
    {
        Canvas canvas = new(renderer, theme);

        canvas.SetAsRoot(renderer);
        canvas.Visualize();

        return canvas;
    }

    /// <summary>
    ///     Set the scale of the canvas.
    /// </summary>
    /// <param name="newScale">The scale factor, which must be greater than zero.</param>
    public void SetScale(Single newScale)
    {
        if (newScale <= 0)
            throw Exceptions.ArgumentOutOfRange(nameof(newScale), newScale);

        Scale = newScale;

        onlyRenderer.Scale(newScale);

        UpdateSize();

        Visualization.GetValue()?.InvalidateRender();
    }

    /// <summary>
    ///     Set the size of the rendering viewport.
    /// </summary>
    /// <param name="newSize">The size of the viewport.</param>
    public void SetRenderingSize(Size newSize)
    {
        viewportSize = newSize;

        onlyRenderer.Resize(newSize);

        UpdateSize();
    }

    /// <summary>
    ///     Whether to draw debug outlines for controls.
    /// </summary>
    /// <param name="enabled">True to draw debug outlines, false to disable them.</param>
    public void SetDebugOutlines(Boolean enabled)
    {
        Visualization.GetValue()?.DrawDebugOutlines = enabled;
    }

    private void UpdateSize()
    {
        Visualization.GetValue()?.SetSize(viewportSize / Scale);
    }

    /// <summary>
    ///     Render the canvas.
    /// </summary>
    public void Render()
    {
        Visualization.GetValue()?.Render();
    }

    /// <inheritdoc />
    protected override ControlTemplate<Canvas> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<Canvas>(_ => new Visuals.Canvas
        {
            Child = new ChildPresenter()
        });
    }
}
