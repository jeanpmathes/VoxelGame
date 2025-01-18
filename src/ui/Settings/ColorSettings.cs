// <copyright file="ColorSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow selecting a color.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class ColorSettings : Setting
{
    private readonly Func<ColorS> get;
    private readonly Action<ColorS> set;

    internal ColorSettings(String name, Func<ColorS> get, Action<ColorS> set)
    {
        this.get = get;
        this.set = set;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

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

    private static ColorS ConvertColor(Color color)
    {
        Color32 color32 = Color32.FromRGBA(color.R, color.G, color.B, color.A);

        return color32.ToColorS();
    }

    private static Color ConvertColor(ColorS color)
    {
        var color32 = color.ToColor32();

        return new Color(color32.A, color32.R, color32.G, color32.B);
    }
}
