// <copyright file="Skin.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

using Gwen.Net.Skin;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI.Resources;

/// <summary>
///     Wraps a skin as a resource.
/// </summary>
public sealed class Skin(RID identifier, SkinBase skin) : IResource
{
    /// <summary>
    ///     The wrapped skin.
    /// </summary>
    public SkinBase Value { get; } = skin;

    /// <inheritdoc />
    public RID Identifier { get; } = identifier;

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Skin;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE
}
