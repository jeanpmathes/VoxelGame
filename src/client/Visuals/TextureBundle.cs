// <copyright file="TextureBundle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Logging;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A list of textures that can be used by shaders.
///     Each texture has a name and index.
/// </summary>
public sealed partial class TextureBundle : ITextureIndexProvider, IDominantColorProvider
{
    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    private const String MissingTextureName = "missing_texture";

    private ILoadingContext? loadingContext;

    /// <summary>
    ///     Create a new texture bundle.
    /// </summary>
    /// <param name="textureArray">The loaded texture array.</param>
    /// <param name="textureIndices">A mapping of texture names to indices.</param>
    public TextureBundle(TextureArray textureArray, Dictionary<String, Int32> textureIndices)
    {
        TextureArray = textureArray;
        TextureIndices = textureIndices;
    }

    private TextureArray TextureArray { get; }
    private Dictionary<String, Int32> TextureIndices { get; }

    /// <summary>
    ///     Get the number of textures in the bundle.
    /// </summary>
    public Int32 Count => TextureArray.Count;

    /// <inheritdoc />
    public Color GetDominantColor(Int32 index)
    {
        return TextureArray.GetDominantColor(index);
    }

    /// <inheritdoc />
    public Int32 GetTextureIndex(String name)
    {
        if (name == MissingTextureName)
            return 0;

        if (loadingContext == null)
        {
            LogLoadingDisabled(logger);

            return 0;
        }

        if (TextureIndices.TryGetValue(name, out Int32 value)) return value;

        loadingContext.ReportWarning("Texture", name, "Texture not found");

        return 0;
    }

    /// <summary>
    /// Get the arrays filling the texture slots.
    /// </summary>
    public static (TextureArray, TextureArray) GetTextureSlots(TextureBundle first, TextureBundle second)
    {
        return (first.TextureArray, second.TextureArray);
    }

    /// <summary>
    ///     Set the loading context. This will be used for reporting results.
    /// </summary>
    /// <param name="usedLoadingContext">The loading context to use.</param>
    public void EnableLoading(ILoadingContext usedLoadingContext)
    {
        loadingContext = usedLoadingContext;
    }

    /// <summary>
    ///     Disable loading. This will prevent any further loading reports. Only the fallback texture will be available.
    /// </summary>
    public void DisableLoading()
    {
        loadingContext = null;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureBundle>();

    [LoggerMessage(EventId = LogID.TextureBundle + 0, Level = LogLevel.Warning, Message = "Loading of textures is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingDisabled(ILogger logger);

    #endregion LOGGING
}
