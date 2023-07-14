// <copyright file="WindowSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Support;

/// <summary>
///     The initial window settings.
/// </summary>
public record WindowSettings
{
    /// <summary>
    ///     The title of the window.
    /// </summary>
    public string Title { get; init; } = "New Window";

    /// <summary>
    ///     The initial size of the window.
    /// </summary>
    public Vector2i Size { get; init; } = Vector2i.One;

    /// <summary>
    ///     The scale at which the world space is rendered.
    /// </summary>
    public float RenderScale { get; init; } = 1.0f;

    /// <summary>
    ///     Get a version of these settings with corrected values that are safe to use.
    /// </summary>
    public WindowSettings Corrected
        => this with {Size = new Vector2i(Math.Max(val1: 1, Size.X), Math.Max(val1: 1, Size.Y))};
}
