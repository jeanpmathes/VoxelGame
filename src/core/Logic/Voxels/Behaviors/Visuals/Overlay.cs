// <copyright file="Overlay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Allows blocks to provide an overlay texture.
/// </summary>
/// <seealso cref="IOverlayTextureProvider" />
public partial class Overlay : BlockBehavior, IBehavior<Overlay, BlockBehavior, Block>
{
    private IOverlayTextureProvider? overlayTextureProvider;

    [Constructible]
    private Overlay(Block subject) : base(subject)
    {
        OverlayTextureProvider = Aspect<IOverlayTextureProvider?, Block>.New<Exclusive<IOverlayTextureProvider?, Block>>(nameof(OverlayTextureProvider), this);
    }

    /// <summary>
    ///     The overlay texture provider used by the block.
    /// </summary>
    public Aspect<IOverlayTextureProvider?, Block> OverlayTextureProvider { get; }

    /// <summary>
    /// The overlay texture provider for the block.
    /// </summary>
    public IOverlayTextureProvider Provider
    {
        get
        {
            overlayTextureProvider ??= OverlayTextureProvider.GetValue(original: null, Subject) ?? new DefaultOverlayTextureProvider();

            return overlayTextureProvider;
        }
    }

    private sealed class DefaultOverlayTextureProvider : IOverlayTextureProvider
    {
        public OverlayTexture GetOverlayTexture(Content content)
        {
            return new OverlayTexture(ITextureIndexProvider.MissingTextureIndex, ColorS.None, IsAnimated: false);
        }
    }
}
