// <copyright file="IMeshingFactory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals;

/// <summary>
/// </summary>
public interface IMeshingFactory
{
    /// <summary>
    ///     Create a new meshing instance.
    /// </summary>
    /// <param name="hint">A hint for the size of the mesh that will be created.</param>
    /// <returns>The new meshing instance.</returns>
    public IMeshing Create(int hint);
}
