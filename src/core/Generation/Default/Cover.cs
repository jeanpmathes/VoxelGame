// <copyright file="Cover.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Cover is generated on top of the terrain. It can be used for one-block sized elements.
/// </summary>
public class Cover
{
    /// <summary>
    ///     Create a default cover.
    /// </summary>
    public static Cover Default => new();


    /// <summary>
    ///     Get the cover for a given block.
    /// </summary>
    public Content GetContent(Vector3i position, bool isFilled, in Map.Sample sample)
    {
        if (isFilled) return Content.Default;

        double temperature = sample.GetTemperatureInCelsius(position.Y);

        if (temperature < 0) return new Content(Block.Specials.Snow.GetInstance(height: 1), FluidInstance.Default);

        return new Content(Block.TallGrass);
    }
}
