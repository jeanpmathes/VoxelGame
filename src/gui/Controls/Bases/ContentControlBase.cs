// <copyright file="ContentControlBase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls.Templates;

namespace VoxelGame.GUI.Controls.Bases;

/// <summary>
///     Abstract base class for a content control, which is a control that displays content using a content template.
/// </summary>
/// <typeparam name="TContent">The type of the content.</typeparam>
/// <typeparam name="TControl">The concrete type of the control.</typeparam>
public abstract class ContentControlBase<TContent, TControl> : Control<TControl> where TContent : class where TControl : ContentControlBase<TContent, TControl>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ContentControlBase{TContent, TControl}" /> class.
    /// </summary>
    protected ContentControlBase()
    {
        Content = Property.Create(this, default(TContent?));
        Content.ValueChanged += OnContentChanged;

        ContentTemplate = Property.Create(this, default(ContentTemplate<TContent>?));
        ContentTemplate.ValueChanged += OnContentTemplateChanged;
    }

    private void OnContentChanged(Object? sender, EventArgs e)
    {
        UpdateChild();
    }

    private void OnContentTemplateChanged(Object? sender, EventArgs e)
    {
        UpdateChild();
    }

    /// <inheritdoc />
    protected override void OnInvalidateContext()
    {
        UpdateChild();
    }

    private void UpdateChild()
    {
        TContent? content = Content.GetValue();

        if (content == null)
        {
            SetChild(null);
            return;
        }

        Control child = GetContentTemplate().Apply(content);

        SetChild(child);
    }

    private IContentTemplate<TContent> GetContentTemplate()
    {
        if (ContentTemplate.GetValue() is {} localTemplate)
            return localTemplate;

        return Context.GetContentTemplate<TContent>();
    }

    #region PROPERTIES

    /// <summary>
    ///     The content to display.
    ///     Content can be any objects, including controls.
    /// </summary>
    public Property<TContent?> Content { get; }

    /// <summary>
    ///     The template used to display the content.
    ///     If not set, the template will be retrieved from the context based on the content type.
    /// </summary>
    public Property<ContentTemplate<TContent>?> ContentTemplate { get; }

    #endregion PROPERTIES
}
