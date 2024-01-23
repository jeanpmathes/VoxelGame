// <copyright file="TextureSupport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gwen.Net;
using VoxelGame.Core.Visuals;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Supports the required operations for texture rendering.
/// </summary>
public sealed class TextureSupport
{
    private readonly TextureList textures;

    private readonly Dictionary<string, string> preloadNameToPath = new();

    private bool textureDiscardAllowed;

    /// <summary>
    ///     Creates a new texture support class.
    /// </summary>
    /// <param name="textures">The texture list to use.</param>
    /// <param name="settings">The settings to use.</param>
    public TextureSupport(TextureList textures, GwenGuiSettings settings)
    {
        this.textures = textures;

        PerformTexturePreload(settings);
    }

    private void PerformTexturePreload(GwenGuiSettings settings)
    {
        foreach (TexturePreload texturePreload in settings.TexturePreloads)
        {
            Exception? exception = textures.LoadTexture(texturePreload.File,
                textureDiscardAllowed,
                _ =>
                {
                    preloadNameToPath.Add(texturePreload.Name, texturePreload.File.FullName);
                });

            if (exception != null)
                settings.TexturePreloadErrorCallback(texturePreload, exception);
        }
    }

    /// <summary>
    ///     Indicate that the loading phase is finished.
    ///     Textures that are loaded after this call can be freed, while texture created during loading are kept alive with the
    ///     client.
    /// </summary>
    public void FinishLoading()
    {
        textureDiscardAllowed = true;
    }

    /// <summary>
    ///     Get the texture handle for the given texture.
    /// </summary>
    public static TextureList.Handle GetTextureHandle(Texture texture)
    {
        return GetRenderData(texture);
    }

    /// <summary>
    ///     Load a texture.
    /// </summary>
    /// <param name="texture">The texture to load.</param>
    /// <param name="errorCallback">The callback to invoke if an error occurs.</param>
    public void LoadTexture(Texture texture, Action<Exception> errorCallback)
    {
        TextureList.Handle handle = TextureList.Handle.Invalid;

        if (preloadNameToPath.TryGetValue(texture.Name, out string? path)) handle = textures.GetTexture(path);

        if (!handle.IsValid) handle = textures.GetTexture(texture.Name);

        if (!handle.IsValid)
        {
            Exception? exception = textures.LoadTexture(new FileInfo(texture.Name),
                textureDiscardAllowed,
                loaded =>
                {
                    handle = loaded;
                });

            if (exception != null) errorCallback(exception);
        }

        if (handle.IsValid) SetTextureProperties(texture, handle);
        else SetFailedTextureProperties(texture);
    }

    /// <summary>
    ///     Load a texture directly from an image.
    /// </summary>
    /// <param name="t">The texture to load.</param>
    /// <param name="image">The image to load.</param>
    public void LoadTextureDirectly(Texture t, Image image)
    {
        TextureList.Handle loadedTexture = textures.LoadTexture(image, allowDiscard: true);
        SetTextureProperties(t, loadedTexture);
    }

    /// <summary>
    ///     Get the color of a pixel in a texture.
    /// </summary>
    /// <param name="texture">The texture in which to get the pixel.</param>
    /// <param name="pixel">The pixel to get.</param>
    /// <returns>The color of the pixel, or null if the texture is not valid.</returns>
    public Color? GetTexturePixel(Texture texture, (uint x, uint y) pixel)
    {
        if (texture.RendererData == null) return null;

        TextureList.Handle handle = GetRenderData(texture);

        return textures.GetPixel(handle, pixel.x, pixel.y);
    }

    /// <summary>
    ///     Free a texture.
    /// </summary>
    public void FreeTexture(Texture texture)
    {
        textures.DiscardTexture(GetRenderData(texture));

        texture.RendererData = null;
        texture.Width = 0;
        texture.Height = 0;
        texture.Failed = false;
    }

    private void SetTextureProperties(Texture texture, TextureList.Handle loadedTexture)
    {
        textures.DiscardTexture(GetRenderData(texture));

        Support.Objects.Texture? entry = textures.GetEntry(loadedTexture);

        Debug.Assert(loadedTexture.IsValid);
        Debug.Assert(entry != null);

        texture.Width = entry.Width;
        texture.Height = entry.Height;
        texture.Failed = false;

        texture.RendererData = loadedTexture;
    }

    private void SetFailedTextureProperties(Texture texture)
    {
        textures.DiscardTexture(GetRenderData(texture));

        texture.RendererData = null;
        texture.Width = 0;
        texture.Height = 0;
        texture.Failed = true;
    }

    private static TextureList.Handle GetRenderData(Texture texture)
    {
        if (texture.RendererData == null) return TextureList.Handle.Invalid;

        var handle = (TextureList.Handle) texture.RendererData;
        Debug.Assert(handle.IsValid);

        return handle;
    }
}
