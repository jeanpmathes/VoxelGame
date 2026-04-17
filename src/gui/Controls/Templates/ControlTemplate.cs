// <copyright file="ControlTemplate.cs" company="VoxelGame">
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
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Controls.Templates;

/// <summary>
///     Utility class for control templates.
/// </summary>
public static class ControlTemplate
{
    /// <summary>
    ///     Creates a new control template from the given function.
    /// </summary>
    /// <param name="function">The function that creates the visual structure for the control.</param>
    /// <typeparam name="TControl">The type of the templated control.</typeparam>
    /// <returns>The created control template.</returns>
    public static ControlTemplate<TControl> Create<TControl>(Func<TControl, Visual> function) where TControl : Control<TControl>
    {
        return new ControlTemplate<TControl>(function);
    }
}

/// <summary>
///     A template defines how a control is visually structured.
/// </summary>
/// <typeparam name="TControl">The type of the templated control.</typeparam>
public class ControlTemplate<TControl> where TControl : Control<TControl>
{
    private readonly Func<TControl, Visual> function;

    /// <summary>
    ///     Creates a new control template.
    /// </summary>
    /// <param name="function">The function that creates the visual structure for the control.</param>
    public ControlTemplate(Func<TControl, Visual> function)
    {
        this.function = function;
    }

    /// <summary>
    ///     Applies the template to the given control, creating its visual structure.
    /// </summary>
    /// <param name="control">The control to apply the template to.</param>
    /// <returns>The created visual structure.</returns>
    public Visual Apply(TControl control)
    {
        return function(control);
    }
}
