// <copyright file="MeshContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Provides the context required to generate meshes for blocks.
/// </summary>
/// <param name="State">The state of the block for which the mesh is generated.</param>
/// <param name="TextureIndexProvider">Provides texture indices used during mesh generation.</param>
/// <param name="ModelProvider">Provides models used during mesh generation.</param>
/// <param name="Validator">Validator to check for validity during mesh generation.</param>
public readonly record struct MeshContext(
    State State,
    ITextureIndexProvider TextureIndexProvider,
    IModelProvider ModelProvider,
    IValidator Validator);
