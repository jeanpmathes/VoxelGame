// <copyright file="Blend.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using Image = VoxelGame.Core.Visuals.Image;

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
        {
            Vector4d backColor = back.GetPixel(x, y).ToVector4();
            Vector4d frontColor = front.GetPixel(x, y).ToVector4();

            Vector4d result = backColor * (1 - frontColor.W) + frontColor * frontColor.W;

            back.SetPixel(x, y, result.ToColor());
        }
    }
}
