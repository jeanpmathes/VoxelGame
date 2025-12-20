// <copyright file="Deck.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.IO;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Defines a deck, which is a collection of layers and modifiers to create textures.
///     See <see cref="Bundler" /> for more information.
/// </summary>
public class Deck
{
    /// <summary>
    ///     The file which defines this deck.
    /// </summary>
    public required FileInfo File { get; init; }

    /// <summary>
    ///     The mode in which the layers are combined.
    /// </summary>
    public required String? Combinator { get; init; }

    /// <summary>
    ///     The layers of this deck.
    /// </summary>
    public required IReadOnlyCollection<Layer> Layers { get; init; }

    /// <summary>
    ///     The modifiers that are applied to this deck after all layers have been combined.
    /// </summary>
    public required IReadOnlyCollection<Modifier> Modifiers { get; init; }

    /// <summary>
    ///     Defines a layer in a deck. Layers are blended together from top to bottom.
    /// </summary>
    public class Layer
    {
        /// <summary>
        ///     The image source of this layer. Can be a sheet, part or another deck.
        /// </summary>
        public required String Source { get; init; }

        /// <summary>
        ///     The modifiers that are applied to this layer.
        /// </summary>
        public required IReadOnlyCollection<Modifier> Modifiers { get; init; }
    }

    /// <summary>
    ///     Defines a modifier that can be applied to a layer or a deck.
    /// </summary>
    public class Modifier
    {
        /// <summary>
        ///     The type of this modifier.
        /// </summary>
        public required String Type { get; init; }

        /// <summary>
        ///     All parameters of this modifier.
        /// </summary>
        public required IReadOnlyDictionary<String, String> Parameters { get; init; }
    }
}
