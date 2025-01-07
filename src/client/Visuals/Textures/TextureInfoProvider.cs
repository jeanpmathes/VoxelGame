// <copyright file="TextureIndexProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
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

    private static readonly Color fallbackColor = Color.Black;

    private TextureBundle? blockTextures;
    private TextureBundle? fluidTextures;

    /// <inheritdoc />
    public Color GetDominantColor(Int32 index, Boolean isBlock)
    {
        if (index == MissingTextureIndex)
            return fallbackColor;

        if (blockTextures == null || fluidTextures == null)
            return fallbackColor;

        if (Context == null)
        {
            LogLoadingDisabled(logger);

            return fallbackColor;
        }

        TextureBundle bundle = isBlock ? blockTextures : fluidTextures;

        if (index < 0 || index >= bundle.Count)
        {
            Context.ReportWarning(this, $"Texture index '{index}' out of bounds, using fallback instead");

            return fallbackColor;
        }

        return bundle.GetDominantColor(index);
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

    #endregion LOGGING
}
