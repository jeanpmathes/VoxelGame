// <copyright file="IChunkMeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Sections;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Defines a meshing context.
/// </summary>
public interface IChunkMeshingContext
{
    /// <summary>
    ///     Get the meshing factory of the context.
    /// </summary>
    IMeshingFactory MeshingFactory { get; }

    /// <summary>
    ///     Gets the section with the given position, if it is part of the context.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The section, or null if it is not part of the context.</returns>
    Section? GetSection(SectionPosition position);

    /// <summary>
    ///     Get the tint for a position.
    /// </summary>
    /// <param name="position">The world/block position.</param>
    /// <returns>The tint colors.</returns>
    (ColorS block, ColorS fluid) GetPositionTint(Vector3i position);
}
