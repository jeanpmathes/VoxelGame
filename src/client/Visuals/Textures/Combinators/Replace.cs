// <copyright file="Blend.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using JetBrains.Annotations;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures.Combinators;

/// <summary>
///     Replaces the pixels of the lower sheet with those of the upper sheet.
///     If the upper pixel transparency is <c>0</c>, the lower pixel is kept.
/// </summary>
[UsedImplicitly]
public class Replace() : BasicCombinator("replace")
{
    /// <inheritdoc />
    protected override void Apply(Image back, Image front)
    {
        for (var x = 0; x < back.Width; x++)
        for (var y = 0; y < back.Height; y++)
        {
            Color frontColor = front.GetPixel(x, y);

            if (frontColor.A == 0)
                continue;

            back.SetPixel(x, y, frontColor);
        }
    }
}
