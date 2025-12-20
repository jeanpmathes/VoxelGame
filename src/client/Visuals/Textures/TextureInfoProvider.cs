// <copyright file="TextureIndexProvider.cs" company="VoxelGame">
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
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Provides the texture indices of loaded textures for blocks and fluids, as well as their dominant colors.
/// </summary>
public partial class TextureInfoProvider : ITextureIndexProvider, IDominantColorProvider
{
    private const Int32 MissingTextureIndex = 0;

    private static readonly ColorS fallbackColor = ColorS.Black;

    private TextureBundle? blockTextures;
    private TextureBundle? fluidTextures;

    /// <inheritdoc />
    public ColorS GetDominantColor(Int32 index, Boolean isBlock)
    {
        if (index == MissingTextureIndex)
            return fallbackColor;

        if (blockTextures == null || fluidTextures == null)
            return fallbackColor;

        TextureBundle bundle = isBlock ? blockTextures : fluidTextures;

        if (index >= 0 && index < bundle.Count)
            return bundle.GetDominantColor(index);

        if (Context != null) Context.ReportWarning(this, $"Texture index '{index}' out of bounds, using fallback instead");
        else LogTextureIndexOutOfBounds(logger, index);

        return fallbackColor;
    }

    /// <inheritdoc />
    public IResourceContext? Context { get; set; }

    /// <inheritdoc />
    public void SetUp()
    {
        blockTextures = Context?.Get<TextureBundle>(Resources.Textures.BlockID);
        fluidTextures = Context?.Get<TextureBundle>(Resources.Textures.FluidID);
    }

    /// <inheritdoc />
    public Int32 GetTextureIndex(TID identifier)
    {
        if (identifier.IsMissingTexture)
            return MissingTextureIndex;

        if (blockTextures == null || fluidTextures == null)
            return MissingTextureIndex;

        if (Context == null)
        {
            LogLoadingDisabled(logger);

            return MissingTextureIndex;
        }

        TextureBundle bundle = identifier.IsBlock ? blockTextures : fluidTextures;

        if (bundle.TryGetTextureIndex(identifier.Key, out Int32 value))
            return value;

        Context.ReportWarning(this, $"Texture '{identifier}' not found, using fallback instead");

        return MissingTextureIndex;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureInfoProvider>();

    [LoggerMessage(EventId = LogID.TextureIndexProvider + 0, Level = LogLevel.Warning, Message = "Loading of textures is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingDisabled(ILogger logger);

    [LoggerMessage(EventId = LogID.TextureIndexProvider + 1, Level = LogLevel.Warning, Message = "Texture index '{index}' out of bounds, using fallback instead")]
    private static partial void LogTextureIndexOutOfBounds(ILogger logger, Int32 index);

    #endregion LOGGING
}
