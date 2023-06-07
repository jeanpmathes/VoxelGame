// <copyright file="TextureList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using VoxelGame.Core.Collections;
using VoxelGame.Support;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Stores all loaded textures and provides methods to access them for the Draw2D pipeline.
/// </summary>
public class TextureList
{
    private const int NeverDiscard = -1;
    private readonly Dictionary<string, int> availableTextures = new();

    private readonly Client client;

    /// <summary>
    ///     Contains the indices of textures that were added since the last upload.
    ///     This is used to prevent textures from being discarded during the same frame they are uploaded.
    /// </summary>
    private readonly SortedSet<int> newTextures = new();

    private readonly GappedList<Texture> textures;

    private readonly PooledList<int> usage = new();

    private SortedSet<int> previousNewTextures = new();

    /// <summary>
    ///     Creates a new texture list.
    /// </summary>
    /// <param name="client">The client with which the textures are associated.</param>
    public TextureList(Client client)
    {
        this.client = client;

        // The Draw2D pipeline requires at least one texture.
        using Bitmap image = Texture.CreateFallback(resolution: 1);
        Texture sentinel = client.LoadTexture(image);

        textures = new GappedList<Texture>(sentinel);
        AddEntry(sentinel, allowDiscard: false);
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

        foreach (int index in previousNewTextures) DiscardIfUnused(new Handle(index));

        previousNewTextures = newTextures;
        newTextures.Clear();

        draw2D.InitializeTextures(textures.AsSpan());

        IsDirty = false;
    }

    /// <summary>
    ///     Safely load a texture from a file. If there is already a texture with the same name, it is returned.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="allowDiscard">Whether the texture should be discarded when it is no longer used.</param>
    /// <param name="callback">The callback that receives the texture handle if the load was successful.</param>
    /// <returns>An exception if the load failed, null otherwise.</returns>
    public Exception? LoadTexture(FileSystemInfo path, bool allowDiscard, Action<Handle> callback)
    {
        Handle existing = GetTexture(path.FullName);

        if (existing.IsValid)
        {
            callback(existing);

            return null;
        }

        try
        {
            using Bitmap bitmap = new(path.FullName);
            Texture texture = client.LoadTexture(bitmap);

            Handle loadedTexture = AddEntry(texture, allowDiscard);
            availableTextures[path.FullName] = loadedTexture.Index;

            callback(loadedTexture);

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
    ///     Load a texture from a bitmap.
    /// </summary>
    /// <param name="bitmap">The bitmap to load from.</param>
    /// <param name="allowDiscard">Whether the texture should be discarded when it is no longer used.</param>
    public Handle LoadTexture(Bitmap bitmap, bool allowDiscard)
    {
        Texture texture = client.LoadTexture(bitmap);

        return AddEntry(texture, allowDiscard);
    }

    /// <summary>
    ///     Decrease the usage count of a texture.
    ///     If the usage count reaches zero, the texture is discarded.
    /// </summary>
    /// <param name="handle">The texture handle.</param>
    public void DiscardTexture(Handle handle)
    {
        if (!handle.IsValid) return;
        if (usage[handle.Index] == NeverDiscard) return;

        usage[handle.Index] = Math.Max(val1: 0, usage[handle.Index] - 1);

        DiscardIfUnused(handle);
    }

    private void DiscardIfUnused(Handle handle)
    {
        if (usage[handle.Index] != 0) return;
        if (newTextures.Contains(handle.Index)) return;

        textures[handle.Index].Free();
        usage[handle.Index] = NeverDiscard;

        textures.RemoveAt(handle.Index);

        IsDirty = true;
    }

    private Handle AddEntry(Texture texture, bool allowDiscard)
    {
        int usageCount = allowDiscard ? 0 : NeverDiscard;
        Handle handle = new(textures.Add(texture));

        SafelySetUsage(handle, usageCount);
        IncreaseUsageCount(handle);

        IsDirty = true;
        newTextures.Add(handle.Index);

        return handle;
    }

    private void SafelySetUsage(Handle handle, int usageCount)
    {
        if (usage.Count > handle.Index)
        {
            usage[handle.Index] = usageCount;
        }
        else
        {
            Debug.Assert(usage.Count == handle.Index);
            usage.Add(usageCount);
        }
    }

    private void IncreaseUsageCount(Handle handle)
    {
        if (handle.IsValid && usage[handle.Index] != NeverDiscard) usage[handle.Index]++;
    }

    /// <summary>
    ///     Try to get a texture from the list. The use count of the texture is increased.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <returns>The texture entry, if found.</returns>
    public Handle GetTexture(string name)
    {
        if (!availableTextures.TryGetValue(name, out int index)) return Handle.Invalid;

        Handle handle = new(index);

        IncreaseUsageCount(handle);

        return handle;
    }

    /// <summary>
    ///     Get the texture list entry for a handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The texture list entry, if the handle is valid.</returns>
    public Texture? GetEntry(Handle handle)
    {
        return handle.IsValid ? textures[handle.Index] : null;
    }

    /// <summary>
    /// A handle to a texture list entry.
    /// </summary>
    /// <param name="Index">The index of the texture in the texture list.</param>
    public readonly record struct Handle(int Index)
    {
        private const int InvalidIndex = -1;

        /// <summary>
        /// Get the handle that represents no texture.
        /// </summary>
        public static Handle Invalid => new(InvalidIndex);

        /// <summary>
        ///     Get whether the handle is valid.
        /// </summary>
        public bool IsValid => Index != InvalidIndex;
    }
}
