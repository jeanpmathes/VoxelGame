// <copyright file="IMap.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation;

/// <summary>
///     A map defines global attributes of the entire world.
/// </summary>
public interface IMap
{
    /// <summary>
    ///     Get debug data for a given position, which is shown to the player in the debug view.
    /// </summary>
    /// <param name="position">The world position of the player.</param>
    /// <returns>A string containing debug data.</returns>
    string GetPositionDebugData(Vector3d position);

    /// <summary>
    ///     Get the tint for a position.
    /// </summary>
    (TintColor block, TintColor fluid) GetPositionTint(Vector3d position);
}
