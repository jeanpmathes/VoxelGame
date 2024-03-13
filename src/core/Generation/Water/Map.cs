// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Water;

/// <summary>
///     The map for the water world.
/// </summary>
public class Map : IMap
{
    /// <inheritdoc />
    public Property GetPositionDebugData(Vector3d position)
    {
        return new Message(nameof(Water), "");
    }

    /// <inheritdoc />
    public (TintColor block, TintColor fluid) GetPositionTint(Vector3d position)
    {
        return (TintColor.Green, TintColor.Blue);
    }

    /// <inheritdoc />
    public Temperature GetTemperature(Vector3d position)
    {
        return new Temperature {DegreesCelsius = 20};
    }
}
