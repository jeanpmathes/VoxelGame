// <copyright file="CID.cs" company="VoxelGame">
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

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Contents;

/// <summary>
///     Represents a content identifier. Must be constructed from an unlocalized string.
/// </summary>
public readonly record struct CID(String Identifier)
{
    /// <summary>
    ///     Get the resource ID for the content type T with this CID.
    /// </summary>
    /// <typeparam name="T">The content type.</typeparam>
    /// <returns>The resource ID.</returns>
    public RID GetResourceID<T>() where T : IContent
    {
        return RID.Named<T>(Identifier);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return Identifier;
    }
}
