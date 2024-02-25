﻿// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Support.Input;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     The context in which the user interface is running.
/// </summary>
internal sealed class Context
{
    internal static readonly Size DefaultIconSize = new(size: 40);

    internal static readonly Size SmallIconSize = new(size: 25);

    internal Context(Input input, UIResources resources)
    {
        Fonts = resources.Fonts;
        Input = input;
        Resources = resources;
    }

    internal FontHolder Fonts { get; }
    internal Input Input { get; }

    internal UIResources Resources { get; }

    /// <summary>
    ///     Create a button that uses an icon instead of text.
    /// </summary>
    internal Button CreateIconButton(ControlBase parent, string icon, string toolTip, Color? color = null, bool useAlternativeSkin = true)
    {
        IconButton button = new(parent)
        {
            ImageName = icon,
            ImageSize = DefaultIconSize,
            ToolTipText = toolTip,
            IconOverrideColor = color
        };

        button.SetSkin(useAlternativeSkin ? Resources.AlternativeSkin : Resources.DefaultSkin, doChildren: true);

        return button;
    }
}
