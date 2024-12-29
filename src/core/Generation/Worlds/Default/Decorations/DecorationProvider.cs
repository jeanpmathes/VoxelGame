// <copyright file="DecorationProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
/// Implements <see cref="IDecorationProvider"/>.
/// </summary>
public class DecorationProvider : ResourceProvider<Decoration>, IDecorationProvider
{
    private static readonly Decoration fallback = new EmptyDecoration();

    /// <summary>
    /// Create a new decoration provider.
    /// </summary>
    public DecorationProvider() : base(() => fallback, decoration => decoration) {}

    /// <inheritdoc />
    public Decoration GetDecoration(RID identifier)
    {
        return GetResource(identifier);
    }

    private sealed class EmptyDecoration() : Decoration("Fallback", Single.PositiveInfinity, new NeverDecorator())
    {
        public override Int32 Size => 0;

        protected override void DoPlace(Vector3i position, in PlacementContext placementContext, IGrid grid)
        {
            // Do nothing.
        }
    }

    private sealed class NeverDecorator : Decorator
    {
        public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
        {
            return false;
        }
    }
}
