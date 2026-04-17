// <copyright file="Context.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Linq;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Themes;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.GUI;

/// <summary>
///     The context in which UI elements exist.
///     It provides access to inherited values, such as styles.
///     Contexts are immutable.
/// </summary>
public class Context
{
    private readonly Context? parent;
    private readonly Canvas? canvas;

    private readonly Dictionary<Type, Style>? styles;
    private readonly Dictionary<Type, ContentTemplate>? contentTemplates;

    /// <summary>
    ///     Create an inheriting context with the given parent.
    ///     When creating a context for a control, using this is not necessary as the control will automatically parent its
    ///     local context to the context of the parent element when attached.
    /// </summary>
    /// <param name="self">The overriding context, which is the local context of the element.</param>
    /// <param name="parent">The parent context to inherit values from.</param>
    public Context(Context self, Context parent)
    {
        this.parent = parent;

        canvas = self.canvas ?? parent.canvas;

        styles = self.styles;
        contentTemplates = self.contentTemplates;
    }

    /// <summary>
    ///     Create a context from a theme.
    /// </summary>
    /// <param name="theme">The theme to use when creating this context.</param>
    /// <param name="canvas">The root canvas.</param>
    internal Context(Theme theme, Canvas? canvas)
    {
        this.canvas = canvas;

        styles = new Dictionary<Type, Style>();

        foreach (Style style in theme.Styles)
        {
            styles[style.StyledType] = style;
        }

        contentTemplates = new Dictionary<Type, ContentTemplate>();

        foreach (ContentTemplate content in theme.ContentTemplates)
        {
            contentTemplates[content.ContentType] = content;
        }
    }

    /// <summary>
    ///     Create a new context.
    ///     While this scope is not parented, that is not an issue as elements will parent their local context when attached.
    /// </summary>
    private Context() {}

    /// <summary>
    ///     Get the keyboard focus for this context.
    /// </summary>
    /// <value>The keyboard focus for this context.</value>
    /// <exception cref="InvalidOperationException">Thrown if the context is not attached to a root canvas.</exception>
    public Focus KeyboardFocus => canvas?.Input.GetValue()?.KeyboardFocus ?? throw Exceptions.InvalidOperation("Cannot access keyboard focus on an unattached context.");

    /// <summary>
    ///     Get the pointer (mouse) focus for this context.
    /// </summary>
    /// <value>The pointer focus for this context.</value>
    /// <exception cref="InvalidOperationException">Thrown if the context is not attached to a root canvas.</exception>
    public Focus PointerFocus => canvas?.Input.GetValue()?.PointerFocus ?? throw Exceptions.InvalidOperation("Cannot access pointer focus on an unattached context.");

    /// <summary>
    ///     The default, empty context.
    /// </summary>
    public static Context Default { get; } = new();

    private IStyle<T>? GetStyleForType<T>(Type type) where T : IControl
    {
        if (styles != null && styles.TryGetValue(type, out Style? potentialStyle) && potentialStyle is IStyle<T> style)
            return style;

        return parent?.GetStyleForType<T>(type);
    }

    /// <summary>
    ///     Get the styles for the given element type.
    ///     This will provide a list of styles, starting with the most general and ending with the most specific, that should
    ///     be applied to an element of the given type.
    /// </summary>
    /// <typeparam name="T">The element type to get the style for.</typeparam>
    /// <returns>The styles for the given element type, may be empty.</returns>
    public IReadOnlyList<IStyle<T>> GetStyling<T>() where T : IControl
    {
        if (styles == null)
            return parent?.GetStyling<T>() ?? [];

        // todo: caching of lists

        List<IStyle<T>> result = [];
        HashSet<Type> visited = [];
        CollectStylesForType(typeof(T), visited, result);

        return result;
    }

    private static String GetTypeSortKey(Type type)
    {
        return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
    }

    private void CollectStylesForType<T>(Type type, HashSet<Type> visited, List<IStyle<T>> result) where T : IControl
    {
        if (type == typeof(Object)) return;
        if (!visited.Add(type)) return;

        if (type.BaseType != null)
            CollectStylesForType(type.BaseType, visited, result);

        foreach (Type interfaceType in type.GetInterfaces().OrderBy(GetTypeSortKey, StringComparer.Ordinal))
            CollectStylesForInterface(interfaceType, visited, result);

        if (GetStyleForType<T>(type) is {} style)
            result.Add(style);
    }

    private void CollectStylesForInterface<T>(Type interfaceType, HashSet<Type> visited, List<IStyle<T>> result) where T : IControl
    {
        if (!visited.Add(interfaceType)) return;

        foreach (Type parentInterfaceType in interfaceType.GetInterfaces().OrderBy(GetTypeSortKey, StringComparer.Ordinal))
            CollectStylesForInterface(parentInterfaceType, visited, result);

        if (GetStyleForType<T>(interfaceType) is {} style)
            result.Add(style);
    }

    /// <summary>
    ///     Get a content template for the given content type.
    /// </summary>
    /// <typeparam name="TContent">The content type to get the template for.</typeparam>
    /// <returns>The content template for the given type, or null if none is registered.</returns>
    public IContentTemplate<TContent> GetContentTemplate<TContent>() where TContent : class
    {
        if (contentTemplates != null && contentTemplates.TryGetValue(typeof(TContent), out ContentTemplate? potentialTemplate) && potentialTemplate is IContentTemplate<TContent> template)
            return template;

        return parent?.GetContentTemplate<TContent>() ?? ContentTemplate.Default;
    }

    /// <summary>
    ///     Create a context with a theme. Use this to create contexts for visuals in a single expression.
    /// </summary>
    /// <param name="build">The action to build the theme for the context.</param>
    /// <returns>A new instance of the <see cref="Context" /> class.</returns>
    public static Context Create(Action<ThemeBuilder> build)
    {
        ThemeBuilder builder = new();

        build(builder);

        return new Context(builder.BuildTheme(), canvas: null);
    }
}
