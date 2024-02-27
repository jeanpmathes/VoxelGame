// <copyright file="Context.cs" company="VoxelGame">
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
    internal Button CreateIconButton(
        ControlBase parent,
        string icon,
        string toolTip,
        Color? color = null,
        bool isSmall = false,
        bool useAlternativeSkin = true)
    {
        IconButton button = new(parent)
        {
            ImageName = icon,
            ImageSize = isSmall ? SmallIconSize : DefaultIconSize,
            ToolTipText = toolTip,
            IconOverrideColor = color
        };

        button.SetSkin(useAlternativeSkin ? Resources.AlternativeSkin : Resources.DefaultSkin, doChildren: true);

        return button;
    }

    /// <summary>
    ///     Create a non-functional icon.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    /// <param name="icon">The icon name.</param>
    /// <param name="isSmall">Whether the icon should be small.</param>
    /// <returns>The created icon.</returns>
    internal ImagePanel CreateIcon(ControlBase parent, string icon, bool isSmall = false)
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
    internal static void MakeModal(Window window)
    {
        window.MakeModal(dim: true, new Color(a: 170, r: 40, g: 40, b: 40));
    }
}
