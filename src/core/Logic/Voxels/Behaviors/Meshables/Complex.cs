// <copyright file="Complex.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.Complex" />.
/// </summary>
public class Complex : BlockBehavior, IBehavior<Complex, BlockBehavior, Block>, IMeshable
{
    private readonly Meshed meshed;

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
    public static Complex Construct(Block input)
    {
        return new Complex(input);
    }

    /// <inheritdoc />
    public Meshable Type => Meshable.Complex;

    /// <summary>
    ///     Get the mesh data for a given state of the block.
    /// </summary>
    /// <param name="state">The state to get the mesh data for.</param>
    /// <param name="textureIndexProvider">Provides texture indices for the block.</param>
    /// <param name="modelProvider">Provides models for the block.</param>
    /// <param name="visuals">The visual configuration for the block.</param>
    /// <returns>The mesh data for the given state.</returns>
    public MeshData GetMeshData(State state, ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals)
    {
        ColorS tint = meshed.Tint.GetValue(ColorS.None, state);
        Boolean isAnimated = meshed.IsAnimated.GetValue(original: false, state);

        Mesh mesh = Mesh.GetValue(Meshes.CreateFallback(), new MeshContext(state, textureIndexProvider, modelProvider));
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
