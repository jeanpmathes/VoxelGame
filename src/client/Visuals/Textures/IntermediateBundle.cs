// <copyright file="IntermediateBundle.cs" company="VoxelGame">
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
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Logging;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     An intermediate bundle - contains all textures ready to create a texture bundle, but not yet on the GPU.
/// </summary>
/// <param name="textures">All required textures.</param>
/// <param name="indices">A mapping of texture keys to their indices in the texture array.</param>
public partial class IntermediateBundle(List<Image> textures, Dictionary<String, Int32> indices)
{
    /// <summary>
    ///     Pack the textures into a texture bundle.
    /// </summary>
    /// <param name="client">The client for which the texture bundle is created.</param>
    /// <param name="identifier">The identifier of the texture bundle resource.</param>
    /// <param name="maxCount">
    ///     The maximum number of textures that can be packed into the bundle. Exceeding textures will be
    ///     ignored.
    /// </param>
    /// <param name="resolution">The resolution of each texture in the bundle. Must be a power of 2.</param>
    /// <param name="mipmap">The mipmap algorithm to use for the textures.</param>
    /// <returns>The packed texture bundle.</returns>
    public TextureBundle Pack(VoxelGame.Graphics.Core.Client client, RID identifier, Int32 maxCount, Int32 resolution, Image.MipmapAlgorithm mipmap)
    {
        Int32 mips = BitOperations.Log2((UInt32) resolution) + 1;
        Int32 count = textures.Count;

        List<Image> packedTextures = textures;
        Dictionary<String, Int32> packedIndices = indices;

        if (count > maxCount)
        {
            LogTooManyTextures(logger, count, maxCount);

            packedTextures = textures[..maxCount];
            packedIndices = new Dictionary<String, Int32>(indices);

            count = maxCount;

            Int32 maxIndex = maxCount - 1;

            foreach ((String key, Int32 index) in indices)
                if (index > maxIndex)
                    indices[key] = 0;
        }

        List<Image> data = new(count * mips);

        foreach (Image texture in packedTextures)
        {
            data.Add(texture);
            data.AddRange(texture.GenerateMipmaps(mips, mipmap));
        }

        TextureArray array = TextureArray.Load(client, CollectionsMarshal.AsSpan(data), count, mips);

        return new TextureBundle(identifier, array, packedIndices);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<IntermediateBundle>();

    [LoggerMessage(EventId = LogID.IntermediateBundle + 0,
        Level = LogLevel.Critical,
        Message = "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for this TextureBundle")]
    private static partial void LogTooManyTextures(ILogger logger, Int32 count, Int32 max);

    #endregion LOGGING
}
