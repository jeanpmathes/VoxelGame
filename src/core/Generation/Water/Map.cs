// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Water;

/// <summary>
///     The map for the water world.
/// </summary>
public class Map : IMap
{
    /// <inheritdoc />
    public string GetPositionDebugData(Vector3d position)
    {
        return "";
    }

    /// <inheritdoc />
    public (TintColor block, TintColor fluid) GetPositionTint(Vector3d position)
    {
        return (TintColor.Green, TintColor.Blue);
    }

    /// <inheritdoc />
    public double GetTemperature(Vector3d position)
    {
        return 20.0;
    }
}

