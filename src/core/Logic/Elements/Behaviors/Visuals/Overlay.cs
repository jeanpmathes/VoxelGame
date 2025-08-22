﻿// <copyright file="Overlay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Allows blocks to provide an overlay texture.
/// </summary>
/// <seealso cref="IOverlayTextureProvider"/>
public class Overlay : BlockBehavior, IBehavior<Overlay, BlockBehavior, Block>
{
    private IOverlayTextureProvider? overlayTextureProvider;
    
    /// <summary>
    /// The overlay texture provider used by the block.
    /// </summary>
    public Aspect<IOverlayTextureProvider?, Block> OverlayTextureProvider { get; }

    private Overlay(Block subject) : base(subject)
    {
        OverlayTextureProvider = Aspect<IOverlayTextureProvider?, Block>.New<Exclusive<IOverlayTextureProvider?, Block>>(nameof(OverlayTextureProvider), this);
    }
    
    /// <inheritdoc/>
    public static Overlay Construct(Block input)
    {
        return new Overlay(input);
    }
    
    /// <summary>
    /// Get the overlay texture provider for this block.
    /// </summary>
    /// <returns>The overlay texture provider.</returns>
    public IOverlayTextureProvider GetOverlayTextureProvider()
    {
        overlayTextureProvider ??= OverlayTextureProvider.GetValue(original: null, Subject) 
                                   ?? new DefaultOverlayTextureProvider();
        
        return overlayTextureProvider;
    }
    
    private class DefaultOverlayTextureProvider : IOverlayTextureProvider
    {
        public OverlayTexture GetOverlayTexture(Content content)
        {
            return new OverlayTexture(ITextureIndexProvider.MissingTextureIndex, ColorS.None, IsAnimated: false);
        }
    }
}
