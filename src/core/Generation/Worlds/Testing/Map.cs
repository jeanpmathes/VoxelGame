// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Worlds.Testing;

/// <summary>
///     The map for the testing world.
/// </summary>
public class Map : IMap
{
    /// <inheritdoc />
    public Property GetPositionDebugData(Vector3d position)
    {
        return new Message(nameof(Testing), "No special information.");
    }

    /// <inheritdoc />
    public (ColorS block, ColorS fluid) GetPositionTint(Vector3d position)
    {
        return (ColorS.Green, ColorS.Blue);
    }

    /// <inheritdoc />
    public Temperature GetTemperature(Vector3d position)
    {
        return new Temperature {DegreesCelsius = 20};
    }
}
