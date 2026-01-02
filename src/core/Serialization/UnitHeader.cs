// <copyright file="UnitHeader.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Serialization;

/// <summary>
///     A header describing information about the serialization system and the unit format.
///     It is used once at the beginning of a serialized unit.
/// </summary>
public class UnitHeader : IValue
{
    private readonly String signature;
    private MetaVersion version = MetaVersion.Current;

    /// <summary>
    ///     Create a new meta header.
    /// </summary>
    /// <param name="signature">The signature of the specific format.</param>
    public UnitHeader(String signature)
    {
        this.signature = signature;
    }

    /// <summary>
    ///     Get the version of the serialization system.
    /// </summary>
    public MetaVersion Version => version;

    /// <inheritdoc />
    public void Serialize(Serializer serializer)
    {
        serializer.Signature(signature);
        serializer.Serialize(ref version);

        if (version > MetaVersion.Current)
            serializer.Fail("Unit was created with a newer version of the serialization system.");
    }
}
