﻿// <copyright file="TextureList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gwen.Net;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Toolkit.Utilities;
using Texture = VoxelGame.Graphics.Objects.Texture;

namespace VoxelGame.UI.Platform.Renderer;

/// <summary>
///     Stores all loaded textures and provides methods to access them for the Draw2D pipeline.
/// </summary>
public sealed class TextureList : IDisposable
{
    private const Int32 NeverDiscard = -1;
    private readonly Dictionary<String, Int32> availableTextures = new();

    private readonly Client client;

    private readonly Bag<Texture> textures;
    private readonly PooledList<Image> images = new();
    private readonly PooledList<Int32> usage = new();

    /// <summary>
    ///     Contains the indices of textures that were added since the last upload.
    ///     This is used to prevent textures from being discarded during the same frame they are uploaded.
    /// </summary>
    private SortedSet<Int32> newTextures = new();

    private SortedSet<Int32> previousNewTextures = new();

    /// <summary>
    ///     Creates a new texture list.
    /// </summary>
    /// <param name="client">The client with which the textures are associated.</param>
    public TextureList(Client client)
    {
        this.client = client;

        // The Draw2D pipeline requires at least one texture.
        var image = Image.CreateFallback(size: 1);
        Texture sentinel = client.LoadTexture(image);

        textures = new Bag<Texture>(sentinel);
        AddEntry(sentinel, image, allowDiscard: false);
    }

    /// <summary>
    ///     Whether the texture list has been modified since the last upload.
    /// </summary>
    private Boolean IsDirty { get; set; }

    /// <summary>
    ///     Uploads the texture list to the GPU if it has been modified.
    ///     Only the list of textures is uploaded, not the actual textures.
    /// </summary>
    /// <param name="drawer">The Draw2D pipeline.</param>
    public void UploadIfDirty(Draw2D drawer)
    {
        Throw.IfDisposed(disposed);

        if (!IsDirty) return;

        foreach (Int32 index in previousNewTextures) DiscardIfUnused(new Handle(index));

        previousNewTextures = newTextures;
        newTextures = new SortedSet<Int32>();

        drawer.InitializeTextures(textures.AsSpan());

        IsDirty = false;
    }

    /// <summary>
    ///     Safely load a texture from a file. If there is already a texture with the same name, it is returned.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="allowDiscard">Whether the texture should be discarded when it is no longer used.</param>
    /// <param name="callback">The callback that receives the texture handle if the load was successful.</param>
    /// <returns>An exception if the load failed, null otherwise.</returns>
    public Exception? LoadTexture(FileInfo path, Boolean allowDiscard, Action<Handle> callback)
    {
        Throw.IfDisposed(disposed);

        Handle existing = GetTexture(path.FullName);

        if (existing.IsValid)
        {
            callback(existing);

            return null;
        }

        try
        {
            Image image = Image.LoadFromFile(path);
            Texture texture = client.LoadTexture(image);

            Handle loadedTexture = AddEntry(texture, image, allowDiscard);
            availableTextures[path.FullName] = loadedTexture.Index;

            callback(loadedTexture);

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    ///     Load a texture from a bitmap.
    /// </summary>
    /// <param name="image">The image to load from.</param>
    /// <param name="allowDiscard">Whether the texture should be discarded when it is no longer used.</param>
    public Handle LoadTexture(Image image, Boolean allowDiscard)
    {
        Throw.IfDisposed(disposed);

        Texture texture = client.LoadTexture(image);

        return AddEntry(texture, image, allowDiscard);
    }

    /// <summary>
    ///     Decrease the usage count of a texture.
    ///     If the usage count reaches zero, the texture is discarded.
    /// </summary>
    /// <param name="handle">The texture handle.</param>
    public void DiscardTexture(Handle handle)
    {
        Throw.IfDisposed(disposed);

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
        images[handle.Index] = images[index: 0];
        usage[handle.Index] = NeverDiscard;

        textures.RemoveAt(handle.Index);

        IsDirty = true;
    }

    private Handle AddEntry(Texture texture, Image image, Boolean allowDiscard)
    {
        Int32 usageCount = allowDiscard ? 0 : NeverDiscard;
        Handle handle = new(textures.Add(texture));

        SafelySetImage(handle, image);
        SafelySetUsage(handle, usageCount);
        IncreaseUsageCount(handle);

        IsDirty = true;
        newTextures.Add(handle.Index);

        return handle;
    }

    private void SafelySetImage(Handle handle, Image image)
    {
        if (images.Count > handle.Index)
        {
            images[handle.Index] = image;
        }
        else
        {
            Debug.Assert(images.Count == handle.Index);
            images.Add(image);
        }
    }

    private void SafelySetUsage(Handle handle, Int32 usageCount)
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
    public Handle GetTexture(String name)
    {
        Throw.IfDisposed(disposed);

        if (!availableTextures.TryGetValue(name, out Int32 index)) return Handle.Invalid;

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
        Throw.IfDisposed(disposed);

        return handle.IsValid ? textures[handle.Index] : null;
    }

    /// <summary>
    ///     Get the pixel at the given coordinates.
    /// </summary>
    public Color GetPixel(Handle handle, UInt32 x, UInt32 y)
    {
        Throw.IfDisposed(disposed);

        Color32 color = images[handle.Index].GetPixel((Int32) x, (Int32) y);

        return new Color(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    ///     A handle to a texture list entry.
    /// </summary>
    /// <param name="Index">The index of the texture in the texture list.</param>
    public readonly record struct Handle(Int32 Index)
    {
        private const Int32 InvalidIndex = -1;

        /// <summary>
        ///     Get the handle that represents no texture.
        /// </summary>
        public static Handle Invalid => new(InvalidIndex);

        /// <summary>
        ///     Get whether the handle is valid.
        /// </summary>
        public Boolean IsValid => Index != InvalidIndex;
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        // Because the sentinel texture is used as the gap value, the iteration will not process it.
        textures[index: 0].Free();
        foreach (Texture texture in textures) texture.Free();

        images.Dispose();
        usage.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~TextureList()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
