// <copyright file="ContentRegistry.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Linq;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions;

/// <summary>
///     A specialized variant of the <see cref="Registry{T}" /> for <see cref="IContent" />.
/// </summary>
public class ContentRegistry
{
    private readonly ContentRegistry? parent;

    private readonly Registry<IContent> registry = new(i => i.NamedID);

    private ContentRegistry(ContentRegistry? parent = null)
    {
        this.parent = parent;
    }

    /// <summary>
    ///     The blocks that are registered.
    ///     Ugly fix until the block states are added.
    /// </summary>
    public Registry<Block> Blocks { get; } = new(i => i.NamedID);

    /// <summary>
    ///     Create a new content registry.
    /// </summary>
    /// <returns>The new content registry.</returns>
    public static ContentRegistry Create()
    {
        return new ContentRegistry();
    }

    /// <summary>
    ///     Register a block.
    /// </summary>
    /// <param name="block">The block to register.</param>
    /// <returns>The registered block.</returns>
    public Block Register(Block block)
    {
        Blocks.Register(block);
        parent?.Blocks.Register(block);

        RegisterContent(block);
        parent?.RegisterContent(block);

        return block;
    }

    /// <summary>
    ///     Register new content.
    /// </summary>
    /// <param name="content">The content to register.</param>
    /// <typeparam name="T">The type of the content.</typeparam>
    /// <returns>The registered content.</returns>
    public T RegisterContent<T>(T content) where T : IContent
    {
        registry.Register(content);

        return content;
    }

    /// <summary>
    ///     Get all registered content of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the content.</typeparam>
    /// <returns>The registered content.</returns>
    public IEnumerable<T> RetrieveContent<T>() where T : IContent
    {
        return registry.Values.OfType<T>();
    }

    /// <summary>
    ///     Get all registered content.
    /// </summary>
    /// <returns>The registered content.</returns>
    public IEnumerable<IContent> RetrieveContent()
    {
        return registry.Values;
    }

    /// <summary>
    ///     Create a scoped content registry.
    ///     It will only see content added to it, but all content added will also be added to the parent.
    /// </summary>
    /// <returns>The scoped content registry.</returns>
    public ContentRegistry CreateScoped()
    {
        return new ContentRegistry(this);
    }
}
