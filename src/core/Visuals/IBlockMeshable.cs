﻿// <copyright file="IBlockMeshable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     The base interface for all blocks, defining the meshing methods.
/// </summary>
public interface IBlockMeshable : IBlockBase
{
    /// <summary>
    ///     Create a mesh for this block and add it to the context.
    /// </summary>
    /// <param name="position">The position at which the block is meshed, in section-local coordinates.</param>
    /// <param name="info">Information about the block.</param>
    /// <param name="context">The current meshing context.</param>
    public void CreateMesh(Vector3i position, BlockMeshInfo info, MeshingContext context)
    {
        // Intentionally left empty.
    }

    /// <summary>
    ///     Validate if all block properties are valid for this meshable.
    /// </summary>
    public void Validate()
    {
        if (IsFull)
            throw Exceptions.InvalidOperation("Only special meshables accept full blocks.");
    }
}
