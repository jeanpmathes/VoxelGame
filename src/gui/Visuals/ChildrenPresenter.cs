// <copyright file="ChildrenPresenter.cs" company="VoxelGame">
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
using System.Diagnostics;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Internals;

namespace VoxelGame.GUI.Visuals;

/// <summary>
///     Presents the children of the template owner, or nothing if the owner has no children.
///     This should be in templates of <see cref="MultiChildControl{TControl}" /> controls, and will visualize the child
///     controls of the owner.
///     As this does not define any layout behavior, using this class on its own is not sensible, instead a subclass should
///     be used.
/// </summary>
public abstract class ChildrenPresenter : Visual
{
    private readonly Dictionary<Control, Visual> visualizedChildren = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChildrenPresenter" /> class.
    /// </summary>
    protected ChildrenPresenter()
    {
        UpdateVisibility();
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        Control? templateOwner = TemplateOwner.GetValue();
        if (templateOwner == null) return;

        templateOwner.ChildAdded += OnTemplateOwnerChildAdded;
        templateOwner.ChildRemoved += OnTemplateOwnerChildRemoved;

        Boolean isReparenting = visualizedChildren.Count > 0;
        if (isReparenting) return;

        foreach (Control child in templateOwner.Children)
        {
            AddVisualization(child);
        }
    }

    /// <inheritdoc />
    public override void OnDetach(Boolean isReparenting)
    {
        Control? templateOwner = TemplateOwner.GetValue();
        if (templateOwner == null) return;

        templateOwner.ChildAdded -= OnTemplateOwnerChildAdded;
        templateOwner.ChildRemoved -= OnTemplateOwnerChildRemoved;

        if (isReparenting) return;

        foreach (Visual childVisualization in visualizedChildren.Values)
        {
            RemoveChild(childVisualization);
        }

        visualizedChildren.Clear();

        UpdateVisibility();
    }

    private void OnTemplateOwnerChildAdded(Object? sender, ChildAddedEventArgs e)
    {
        AddVisualization(e.Child);
    }

    private void OnTemplateOwnerChildRemoved(Object? sender, ChildRemovedEventArgs e)
    {
        RemoveVisualization(e.Child);
    }

    private void AddVisualization(Control child)
    {
        if (visualizedChildren.ContainsKey(child)) return;

        Visual childVisualization = child.Visualize();
        visualizedChildren[child] = childVisualization;
        AddChild(childVisualization);

        child.Visualization.ValueChanged += OnVisualizedChildVisualizationChanged;

        UpdateVisibility();
    }

    private void RemoveVisualization(Control child)
    {
        if (visualizedChildren.Remove(child, out Visual? childVisualization))
        {
            RemoveChild(childVisualization);
        }

        UpdateVisibility();
    }

    private void OnVisualizedChildVisualizationChanged(Object? sender, EventArgs e)
    {
        Control? visualizedChild = sender as Control;
        Debug.Assert(visualizedChild != null);

        if (visualizedChildren.TryGetValue(visualizedChild, out Visual? childVisualization))
            RemoveChild(childVisualization);

        childVisualization = visualizedChild.Visualization.GetValue();

        if (childVisualization == null) return;

        visualizedChildren[visualizedChild] = childVisualization;
        AddChild(childVisualization);
    }

    private void UpdateVisibility()
    {
        if (visualizedChildren.Count > 0)
        {
            Visibility.Clear();
        }
        else
        {
            Visibility.Set(GUI.Visibility.Collapsed);
        }
    }
}
