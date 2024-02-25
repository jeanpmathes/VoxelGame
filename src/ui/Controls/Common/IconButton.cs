// <copyright file="IconButton.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
        Text = string.Empty;
    }

    /// <summary>
    ///     A color to override the icon's color as determined by the mouse state.
    /// </summary>
    public Color? IconOverrideColor { get; init; }

    /// <inheritdoc />
    public override void UpdateColors()
    {
        base.UpdateColors();

        ImageColor = IconOverrideColor ?? TextColor;
    }
}
