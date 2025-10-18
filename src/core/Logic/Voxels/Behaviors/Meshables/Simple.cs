// <copyright file="Simple.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.Simple" />.
/// </summary>
public partial class Simple : BlockBehavior, IBehavior<Simple, BlockBehavior, Block>, IMeshable
{
    private readonly Meshed meshed;
    private readonly CubeTextured textured;

    [Constructible]
    private Simple(Block subject) : base(subject)
    {
        meshed = subject.Require<Meshed>();
        textured = subject.Require<CubeTextured>();

        IsTextureRotated = Aspect<Boolean, (State, Side)>.New<Exclusive<Boolean, (State, Side)>>(nameof(IsTextureRotated), this);
    }

    /// <summary>
    ///     Whether the texture is rotated.
    /// </summary>
    public Aspect<Boolean, (State state, Side side)> IsTextureRotated { get; }

    /// <inheritdoc />
    public Meshable Type => Meshable.Simple;

    /// <summary>
    ///     Get the mesh data for a given side and state of the block.
    /// </summary>
    /// <param name="state">The state to get the mesh data for.</param>
    /// <param name="side">The side of the block to get the mesh data for.</param>
    /// <param name="textureIndexProvider">Provides texture indices for given texture IDs.</param>
    /// <returns>The mesh data for the given side and state.</returns>
    public MeshData GetMeshData(State state, Side side, ITextureIndexProvider textureIndexProvider)
    {
        Boolean isTextureRotated = IsTextureRotated.GetValue(original: false, (state, side));
        ColorS tint = meshed.Tint.GetValue(ColorS.None, state);
        Boolean isAnimated = meshed.IsAnimated.GetValue(original: false, state);

        Int32 textureIndex = textured.GetTextureIndex(state, side, textureIndexProvider, isBlock: true);

        return new MeshData(textureIndex, isTextureRotated, tint, isAnimated && textureIndex != ITextureIndexProvider.MissingTextureIndex);
    }

    /// <summary>
    ///     The mesh data for a simple block.
    /// </summary>
    /// <param name="TextureIndex">The index of the texture to use.</param>
    /// <param name="IsTextureRotated">Whether the texture is rotated.</param>
    /// <param name="Tint">The tint color to apply to the mesh.</param>
    /// <param name="IsAnimated">Whether the texture is animated.</param>
    public readonly record struct MeshData(Int32 TextureIndex, Boolean IsTextureRotated, ColorS Tint, Boolean IsAnimated);
}
