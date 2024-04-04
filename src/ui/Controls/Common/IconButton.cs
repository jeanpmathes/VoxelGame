// <copyright file="IconButton.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     Variant of the button control that uses an icon instead of text.
///     The icon changes color depending on the mouse state, just like the text in a regular button.
/// </summary>
public sealed class IconButton : Button
{
    /// <inheritdoc />
    public IconButton(ControlBase parent) : base(parent)
    {
        Text = String.Empty;

        Toggled += (_, _) =>
        {
            if (!IsToggle) return;

            ImageName = ToggleState
                ? ToggledOnIconName ?? ImageName
                : ToggledOffIconName ?? ImageName;
        };
    }

    /// <summary>
    ///     A color to override the icon's color as determined by the mouse state.
    /// </summary>
    public Color? IconOverrideColor { get; init; }

    /// <summary>
    ///     When this is set and the button is a toggle button, this icon will be used when the button is toggled on.
    /// </summary>
    public String? ToggledOnIconName { get; set; }

    /// <summary>
    ///     When this is set and the button is a toggle button, this icon will be used when the button is toggled off.
    /// </summary>
    public String? ToggledOffIconName { get; set; }

    /// <inheritdoc />
    public override void UpdateColors()
    {
        base.UpdateColors();

        ImageColor = IconOverrideColor ?? TextColor;
    }
}
