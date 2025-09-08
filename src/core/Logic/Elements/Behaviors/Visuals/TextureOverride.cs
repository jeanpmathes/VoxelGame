// <copyright file="TextureOverride.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Override the texture provided by models with custom textures.
/// </summary>
public class TextureOverride : BlockBehavior, IBehavior<TextureOverride, BlockBehavior, Block>
{
    /// <summary>
    /// Override all textures with the given replacement texture.
    /// </summary>
    /// <param name="replacement">The replacement texture.</param>
    /// <returns>The created replacement dictionary.</returns>
    public static IReadOnlyDictionary<Int32, TID> All(TID replacement) => new Dictionary<Int32, TID> { [-1] = replacement };
    
    /// <summary>
    /// Override a single texture at the given index with the given replacement texture.
    /// </summary>
    /// <param name="index">The index, corresponding to the order of textures in the model.</param>
    /// <param name="replacement">The replacement texture.</param>
    /// <returns>The created replacement dictionary.</returns>
    public static IReadOnlyDictionary<Int32, TID> Single(Int32 index, TID replacement) => new Dictionary<Int32, TID> { [index] = replacement };
    
    private TextureOverride(Block subject) : base(subject)
    {
        TexturesInitializer = Aspect<IReadOnlyDictionary<Int32, TID>?, Block>.New<Exclusive<IReadOnlyDictionary<Int32, TID>?, Block>>(nameof(TexturesInitializer), this);
    }
    
    /// <inheritdoc />
    public static TextureOverride Construct(Block input)
    {
        return new TextureOverride(input);
    }
    
    /// <summary>
    /// Optional textures to override the texture provided by a model.
    /// </summary>
    public IReadOnlyDictionary<Int32, TID>? Textures { get; private set; } // todo: replace with texture override behavior
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Textures"/> property.
    /// </summary>
    public Aspect<IReadOnlyDictionary<Int32, TID>?, Block> TexturesInitializer { get; }
    
    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Textures = TexturesInitializer.GetValue(original: null, Subject);
    }
}
