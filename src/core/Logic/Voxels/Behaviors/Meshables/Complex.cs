// <copyright file="Complex.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.Complex" />.
/// </summary>
public partial class Complex : BlockBehavior, IBehavior<Complex, BlockBehavior, Block>, IMeshable
{
    private readonly Meshed meshed;

    [Constructible]
    private Complex(Block subject) : base(subject)
    {
        meshed = subject.Require<Meshed>();

        Mesh = Aspect<Mesh, MeshContext>.New<Exclusive<Mesh, MeshContext>>(nameof(Mesh), this);
    }

    /// <summary>
    ///     Get the state dependent mesh for the block.
    /// </summary>
    public Aspect<Mesh, MeshContext> Mesh { get; }

    /// <inheritdoc />
    public Meshable Type => Meshable.Complex;

    /// <summary>
    ///     Get the mesh data in a given context.
    /// </summary>
    /// <param name="context">The mesh context.</param>
    /// <returns>The mesh data for the given context.</returns>
    public MeshData GetMeshData(MeshContext context)
    {
        ColorS tint = meshed.Tint.GetValue(ColorS.NoTint, context.State);
        Boolean isAnimated = meshed.IsAnimated.GetValue(original: false, context.State);

        Mesh mesh = Mesh.GetValue(Meshes.CreateFallback(), context);
        Mesh.Quad[] quads = mesh.GetMeshData(out UInt32 quadCount);

        return new MeshData(quads, quadCount, tint, isAnimated);
    }

    /// <summary>
    ///     The mesh data for a complex block.
    /// </summary>
    /// <param name="Quads">The quads that make up the mesh.</param>
    /// <param name="QuadCount">Number of quads in the mesh.</param>
    /// <param name="Tint">The tint color to apply to the mesh.</param>
    /// <param name="IsAnimated">Whether the texture is animated.</param>
    public readonly record struct MeshData(Mesh.Quad[] Quads, UInt32 QuadCount, ColorS Tint, Boolean IsAnimated);
}
