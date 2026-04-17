// <copyright file="ContentTemplate.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.GUI.Controls.Templates;

/// <summary>
///     Abstract base class of all content templates.
/// </summary>
/// <seealso cref="ContentTemplate{TContent}" />
public abstract class ContentTemplate(RID identifier) : IResource
{
    /// <summary>
    ///     Get the trivial content template for content of type <see cref="Control" />.
    ///     This template simply returns the content as the control structure.
    /// </summary>
    public static IContentTemplate<Control> Trivial { get; } = new ContentTemplate<Control>(content => content, RID.Named<ContentTemplate<Control>>(GetBuiltInContentTemplateName(nameof(Trivial))));

    /// <summary>
    ///     Get a sensible content template for content of type <see cref="String" />.
    ///     It simply displays the string in a text element.
    /// </summary>
    public static IContentTemplate<String> String { get; } = new ContentTemplate<String>(CreateStringContent, RID.Named<ContentTemplate<String>>(GetBuiltInContentTemplateName(nameof(String))));

    /// <summary>
    ///     Get the default content template when no specific template is found for the content type.
    /// </summary>
    public static IContentTemplate<Object> Default { get; } = new ContentTemplate<Object>(content => CreateStringContent(content.ToString() ?? ""), RID.Named<ContentTemplate<Object>>(GetBuiltInContentTemplateName(nameof(Default))));

    /// <summary>
    ///     The type of content this template can be applied to.
    /// </summary>
    public abstract Type ContentType { get; }

    /// <inheritdoc />
    public RID Identifier { get; } = identifier;

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.ContentTemplate;

    private static String GetBuiltInContentTemplateName(String name)
    {
        return $"BuiltIn{name}ContentTemplate";
    }

    /// <summary>
    ///     Creates a content template for content of type <typeparamref name="TContent" /> using the given function.
    /// </summary>
    /// <param name="name">The unique name of this content template.</param>
    /// <param name="function">The function that creates the control structure for the content.</param>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    /// <returns>>The created content template.</returns>
    public static ContentTemplate<TContent> Create<TContent>(String name, Func<TContent, Control> function) where TContent : class
    {
        return new ContentTemplate<TContent>(function, RID.Named<ContentTemplate<TContent>>(name));
    }

    private static Control CreateStringContent(String content)
    {
        return new Text {Content = {Value = content}};
    }

    #region DISPOSABLE

    /// <summary>
    ///     Override to define disposal.
    /// </summary>
    protected virtual void Dispose(Boolean disposing) {}

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~ContentTemplate()
    {
        Dispose(false);
    }

    #endregion DISPOSABLE
}

/// <summary>
///     Interface for content templates, defining how to display content of a specific type.
/// </summary>
/// <typeparam name="TContent">The type of the content.</typeparam>
public interface IContentTemplate<in TContent> : IResource where TContent : class
{
    /// <summary>
    ///     Applies the template to the given content, creating its control structure.
    /// </summary>
    /// <param name="content">The content to apply the template to.</param>
    /// <returns>The created control structure.</returns>
    public Control Apply(TContent content);
}

/// <summary>
///     Defines how to display content of a specific type.
/// </summary>
/// <typeparam name="TContent">The type of the content.</typeparam>
public sealed class ContentTemplate<TContent> : ContentTemplate, IContentTemplate<TContent> where TContent : class
{
    private readonly Func<TContent, Control> function;

    /// <summary>
    ///     Creates a new content template.
    /// </summary>
    /// <param name="function">The function that creates the control structure for the content.</param>
    /// <param name="identifier">The resource identifier of this content template.</param>
    public ContentTemplate(Func<TContent, Control> function, RID identifier) : base(identifier)
    {
        this.function = function;
    }

    /// <inheritdoc />
    public override Type ContentType => typeof(TContent);

    /// <inheritdoc />
    public Control Apply(TContent content)
    {
        return function(content);
    }

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        // Nothing to dispose of.
    }
}
