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
public class WindowSettings
{
    public string Title { get; init; } = "New Window";
    public Vector2i Size { get; init; } = Vector2i.Zero;
}

