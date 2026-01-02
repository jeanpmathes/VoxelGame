// <copyright file="DecorationProvider.cs" company="VoxelGame">
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
