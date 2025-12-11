// <copyright file="IMeshable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals.Meshables;

/// <summary>
///     Marker interface for all meshable behaviors.
///     These behaviors support the actual meshing subclasses by defining aspects of the created meshes.
/// </summary>
public interface IMeshable
{
    /// <summary>
    ///     The type of meshable this behavior supports.
    /// </summary>
    Meshable Type { get; }
}
