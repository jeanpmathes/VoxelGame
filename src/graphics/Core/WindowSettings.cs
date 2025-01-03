﻿// <copyright file="WindowSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Core;

/// <summary>
///     The initial window settings.
/// </summary>
public record WindowSettings
{
    /// <summary>
    ///     The title of the window.
    /// </summary>
    public String Title { get; init; } = "New Window";

    /// <summary>
    ///     The initial size of the window.
    /// </summary>
    public Vector2i Size { get; init; } = Vector2i.One;

    /// <summary>
    ///     The scale at which the world space is rendered.
    /// </summary>
    public Single RenderScale { get; init; } = 1.0f;

    /// <summary>
    ///     Gets a value indicating whether to enable special PIX Graphics.
    /// </summary>
    public Boolean SupportPIX { get; init; }

    /// <summary>
    ///     Gets a value indicating whether to use GBV.
    /// </summary>
    public Boolean UseGBV { get; init; }

    /// <summary>
    ///     Get a version of these settings with corrected values that are safe to use.
    /// </summary>
    public WindowSettings Corrected
        => this with {Size = new Vector2i(Math.Max(val1: 1, Size.X), Math.Max(val1: 1, Size.Y))};
}
