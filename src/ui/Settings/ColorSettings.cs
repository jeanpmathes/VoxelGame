// <copyright file="ColorSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow selecting a color.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class ColorSettings : Setting
{
    private readonly Func<Color> get;
    private readonly Action<Color> set;

    internal ColorSettings(String name, Func<Color> get, Action<Color> set)
    {
        this.get = get;
        this.set = set;

        Name = name;
    }

    protected override String Name { get; }

    private protected override void FillControl(ControlBase control, Context context)
    {
        VerticalLayout layout = new(control);

        ColorPicker colorPicker = new(layout)
        {
            SelectedColor = ConvertColor(get()),
            AlphaVisible = false
        };

        Button select = new(layout)
        {
            Text = Language.Select
        };

        select.Released += (_, _) =>
        {
            set(ConvertColor(colorPicker.SelectedColor));
            Validator.Validate();

            select.Disable();
        };

        select.Disable();

        colorPicker.ColorChanged += (_, _) =>
        {
            select.Enable();
            select.Redraw();
        };
    }

    private static Color ConvertColor(Gwen.Net.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static Gwen.Net.Color ConvertColor(Color color)
    {
        return new Gwen.Net.Color(color.A, color.R, color.G, color.B);
    }
}
