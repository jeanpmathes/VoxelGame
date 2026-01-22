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
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Graphics.Input;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Resources;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     The context in which the user interface is running.
/// </summary>
internal sealed class Context
{
    internal static readonly Size DefaultIconSize = new(size: 40);
    internal static readonly Size SmallIconSize = new(size: 25);

    private Int32 modalDepth;

    internal Context(Input input, UserInterfaceResources resources)
    {
        Fonts = resources.Fonts;
        Input = input;
        Resources = resources;
    }

    /// <summary>
    ///     All fonts available to be used.
    ///     Each font is associated with a role.
    /// </summary>
    internal FontBundle Fonts { get; }

    /// <summary>
    ///     The input manager.
    /// </summary>
    internal Input Input { get; }

    /// <summary>
    ///     Loaded resources like icons and the names to access them.
    /// </summary>
    internal UserInterfaceResources Resources { get; }

    /// <summary>
    ///     Whether the user interface currently shows any modal windows.
    /// </summary>
    internal Boolean IsInModal => modalDepth > 0;

    /// <summary>
    ///     Create a button that uses an icon instead of text.
    /// </summary>
    internal IconButton CreateIconButton(
        ControlBase parent,
        String icon,
        String toolTip,
        Color? color = null,
        Boolean isSmall = false,
        Boolean useAlternativeSkin = true)
    {
        IconButton button = new(parent)
        {
            ImageName = icon,
            ImageSize = isSmall ? SmallIconSize : DefaultIconSize,
            ToolTipText = toolTip,
            IconOverrideColor = color
        };

        Skin skin = useAlternativeSkin ? Resources.AlternativeSkin : Resources.DefaultSkin;
        button.SetSkin(skin.Value, doChildren: true);

        return button;
    }

    /// <summary>
    ///     Create a non-functional icon.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    /// <param name="icon">The icon name.</param>
    /// <param name="isSmall">Whether the icon should be small.</param>
    /// <returns>The created icon.</returns>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    internal ImagePanel CreateIcon(ControlBase parent, String icon, Boolean isSmall = false)
    {
        ImagePanel image = new(parent)
        {
            ImageName = icon,
            ImageSize = isSmall ? SmallIconSize : DefaultIconSize
        };

        return image;
    }

    /// <summary>
    ///     Make a window modal.
    /// </summary>
    internal void MakeModal(Window window)
    {
        window.MakeModal(dim: true, new Color(a: 170, r: 40, g: 40, b: 40));

        modalDepth++;

        window.Closed += (_, _) =>
        {
            modalDepth--;
        };
    }
}
