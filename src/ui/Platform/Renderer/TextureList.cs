// <copyright file="TextureList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using VoxelGame.Support;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Stores all loaded textures and provides methods to access them for the Draw2D pipeline.
/// </summary>
public class TextureList
{
    private readonly Dictionary<string, int> availableTextures = new();

    private readonly Client client;
    private readonly List<Texture> textures = new();

    /// <summary>
    ///     Creates a new texture list.
    /// </summary>
    /// <param name="client">The client with which the textures are associated.</param>
    public TextureList(Client client)
    {
        this.client = client;

        // The Draw2D pipeline requires at least one texture.
        using Bitmap sentinel = Texture.CreateFallback(resolution: 1);
        textures.Add(client.LoadTexture(sentinel));
    }

    /// <summary>
    ///     Whether the texture list has been modified since the last upload.
    /// </summary>
    private bool IsDirty { get; set; }

    /// <summary>
    ///     Uploads the texture list to the GPU if it has been modified.
    ///     Only the list of textures is uploaded, not the actual textures.
    /// </summary>
    /// <param name="draw2D">The Draw2D pipeline.</param>
    public void UploadIfDirty(Draw2D draw2D)
    {
        if (!IsDirty) return;

        draw2D.InitializeTextures(textures);
        IsDirty = false;
    }

    /// <summary>
    ///     Safely load a texture from a file.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="callback">The callback to call when the texture is loaded.</param>
    /// <returns></returns>
    public Exception? LoadTexture(FileSystemInfo path, Action<Entry> callback)
    {
        try
        {
            using Bitmap bitmap = new(path.FullName);
            Texture texture = client.LoadTexture(bitmap);

            int index = textures.Count;
            textures.Add(texture);
            availableTextures.Add(path.FullName, index);

            IsDirty = true;

            callback(new Entry(texture, index));

            return null;
        }
#pragma warning disable S2221 // Not clear what could be thrown here.
        catch (Exception e)
#pragma warning restore S2221
        {
            return e;
        }
    }

    /// <summary>
    ///     Try to get a texture from the list.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <returns>The texture entry, if found.</returns>
    public Entry? GetTexture(string name)
    {
        if (!availableTextures.TryGetValue(name, out int index)) return null;

        return new Entry(textures[index], index);
    }

    /// <summary>
    ///     A texture entry.
    /// </summary>
    /// <param name="Texture">The texture.</param>
    /// <param name="Index">The index of the texture in the texture list.</param>
    public record struct Entry(Texture Texture, int Index);
}
