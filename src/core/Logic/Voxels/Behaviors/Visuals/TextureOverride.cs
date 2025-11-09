// <copyright file="TextureOverride.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Override the texture provided by models with custom textures.
/// </summary>
public partial class TextureOverride : BlockBehavior, IBehavior<TextureOverride, BlockBehavior, Block>
{
    [Constructible]
    private TextureOverride(Block subject) : base(subject)
    {
    }

    /// <summary>
    ///     Optional textures to override the texture provided by a model.
    /// </summary>
    public ResolvedProperty<IReadOnlyDictionary<Int32, TID>?> Textures { get; } 
        = ResolvedProperty<IReadOnlyDictionary<Int32, TID>?>.New<Exclusive<IReadOnlyDictionary<Int32, TID>?, Void>>(nameof(Textures));

    /// <summary>
    ///     Override all textures with the given replacement texture.
    /// </summary>
    /// <param name="replacement">The replacement texture.</param>
    /// <returns>The created replacement dictionary.</returns>
    public static IReadOnlyDictionary<Int32, TID> All(TID replacement)
    {
        return new Dictionary<Int32, TID> {[-1] = replacement};
    }

    /// <summary>
    ///     Override a single texture at the given index with the given replacement texture.
    /// </summary>
    /// <param name="index">The index, corresponding to the order of textures in the model.</param>
    /// <param name="replacement">The replacement texture.</param>
    /// <returns>The created replacement dictionary.</returns>
    public static IReadOnlyDictionary<Int32, TID> Single(Int32 index, TID replacement)
    {
        return new Dictionary<Int32, TID> {[index] = replacement};
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Textures.Initialize(this);
    }
}
