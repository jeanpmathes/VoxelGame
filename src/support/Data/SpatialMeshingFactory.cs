// <copyright file="SpatialMeshingFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Visuals;

namespace VoxelGame.Support.Data;

/// <summary>
///     Creates instances of <see cref="SpatialMeshing" />.
/// </summary>
public class SpatialMeshingFactory : IMeshingFactory
{
    /// <summary>
    ///     A shared instance of the factory. Holds no state.
    /// </summary>
    public static SpatialMeshingFactory Shared { get; } = new();

    /// <inheritdoc />
    public IMeshing Create(int hint)
    {
        return new SpatialMeshing(hint);
    }
}
