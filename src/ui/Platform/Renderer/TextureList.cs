// <copyright file="TextureList.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
    private readonly Dictionary<string, LinkedListNode<Entry>> availableTextures = new();

    private readonly Client client;
    private readonly LinkedList<Entry> textures = new();

    /// <summary>
    ///     Creates a new texture list.
    /// </summary>
    /// <param name="client">The client with which the textures are associated.</param>
    public TextureList(Client client)
    {
        this.client = client;

        // The Draw2D pipeline requires at least one texture.
        using Bitmap sentinel = Texture.CreateFallback(resolution: 1);
        textures.AddLast(new Entry(client.LoadTexture(sentinel), NeverDiscard));
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

        draw2D.InitializeTextures(textures.Select((entry, index) =>
        {
            IEntry e = entry;
            e.SetIndex(index);

            return entry.Texture;
        }));

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
        Handle? existing = GetTexture(path.FullName);

        if (existing != null)
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
    public void DiscardTexture(Handle? handle)
    {
        if (handle is not {Index: var index}) return;

        IEntry e = index.Value;

        if (e.UsageCount == NeverDiscard) return;

        e.UsageCount--;

        if (e.UsageCount != 0) return;

        textures.Remove(index);
        index.Value.Texture.Free();

        IsDirty = true;
    }

    private Handle AddEntry(Texture texture, bool allowDiscard)
    {
        int usageCount = allowDiscard ? 0 : NeverDiscard;

        LinkedListNode<Entry> index = textures.AddLast(new Entry(texture, usageCount));
        IncreaseUsageCount(index);

        IsDirty = true;

        return new Handle(index);
    }

    private static void IncreaseUsageCount(LinkedListNode<Entry> index)
    {
        IEntry entry = index.Value;

        if (entry.UsageCount != NeverDiscard) entry.UsageCount++;
    }

    /// <summary>
    ///     Try to get a texture from the list. The use count of the texture is increased.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <returns>The texture entry, if found.</returns>
    public Handle? GetTexture(string name)
    {
        if (!availableTextures.TryGetValue(name, out LinkedListNode<Entry>? entry)) return null;

        IncreaseUsageCount(entry);

        return new Handle(entry);
    }

    /// <summary>
    /// A handle to a texture list entry.
    /// </summary>
    /// <param name="Index">The index of the texture in the texture list.</param>
    public record Handle(LinkedListNode<Entry> Index)
    {
        /// <summary>
        ///     Get the texture list entry.
        /// </summary>
        public Entry Entry => Index.Value;
    }

    /// <summary>
    ///     An internal texture list entry.
    /// </summary>
    public class Entry : IEntry
    {
        /// <summary>
        ///     Creates a new texture list entry.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="usageCount">The usage count.</param>
        public Entry(Texture texture, int usageCount)
        {
            Texture = texture;

            IEntry self = this;
            self.UsageCount = usageCount;
        }

        /// <summary>
        ///     Get the texture.
        /// </summary>
        public Texture Texture { get; }

        /// <summary>
        ///     Get the index of the texture in the uploaded list.
        /// </summary>
        public int Index { get; private set; }

        int IEntry.UsageCount { get; set; }

        void IEntry.SetIndex(int index)
        {
            Index = index;
        }
    }

    private interface IEntry
    {
        public int UsageCount { get; set; }
        public void SetIndex(int index);
    }
}
