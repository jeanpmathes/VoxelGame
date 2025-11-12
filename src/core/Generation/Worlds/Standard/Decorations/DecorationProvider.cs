// <copyright file="DecorationProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Implements <see cref="IDecorationProvider" />.
/// </summary>
public class DecorationProvider : ResourceProvider<Decoration>, IDecorationProvider
{
    private static readonly Decoration fallback = new EmptyDecoration();

    /// <inheritdoc />
    public Decoration GetDecoration(RID identifier)
    {
        return GetResource(identifier);
    }

    /// <inheritdoc />
    protected override Decoration CreateFallback()
    {
        return fallback;
    }

    private sealed class EmptyDecoration() : Decoration("Fallback", new NeverDecorator())
    {
        public override Int32 Size => 0;

        protected override void DoPlace(Vector3i position, IGrid grid, in PlacementContext placementContext)
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
