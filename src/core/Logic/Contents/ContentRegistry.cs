// <copyright file="ContentRegistry.cs" company="VoxelGame">
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

using System.Collections.Generic;
using System.Linq;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Contents;

/// <summary>
///     A specialized variant of the <see cref="Registry{T}" /> for <see cref="IContent" />.
/// </summary>
public class ContentRegistry
{
    private readonly ContentRegistry? parent;

    private readonly Registry<IContent> registry = new(i => i.ID.Identifier);

    private ContentRegistry(ContentRegistry? parent = null)
    {
        this.parent = parent;
    }

    /// <summary>
    ///     Create a new content registry.
    /// </summary>
    /// <returns>The new content registry.</returns>
    public static ContentRegistry Create()
    {
        return new ContentRegistry();
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
        parent?.RegisterContent(content);

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
