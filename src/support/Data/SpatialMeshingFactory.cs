// <copyright file="SpatialMeshingFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation.")]
    public IMeshing Create(Int32 hint)
    {
        return new SpatialMeshing(hint);
    }
}
