// <copyright file="Blend.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Combinators;

/// <summary>
///     Blends two sheets together using alpha blending.
/// </summary>
[UsedImplicitly]
public class Blend() : BasicCombinator("blend")
{
    /// <inheritdoc />
    protected override void Apply(Image back, Image front)
    {
        for (var x = 0; x < back.Width; x++)
        for (var y = 0; y < back.Height; y++)
            back.SetPixel(x,
                y,
                ColorS.Blend(back.GetPixel(x, y).ToColorS(), front.GetPixel(x, y).ToColorS()).ToColor32());
    }
}
