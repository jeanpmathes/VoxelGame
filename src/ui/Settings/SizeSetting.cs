// <copyright file="SizeSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using OpenTK.Mathematics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick a size, represented by two integers that are at least 1.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class SizeSetting : Setting
{
    private readonly Func<Vector2i> get;
    private readonly Action<Vector2i> set;
    private readonly Func<Vector2i>? update;

    internal SizeSetting(string name, Func<Vector2i> get, Action<Vector2i> set, Func<Vector2i>? update)
    {
        this.get = get;
        this.set = set;
        this.update = update;

        Name = name;
    }

    protected override string Name { get; }

    private protected override void FillControl(ControlBase control, Context context)
    {
        Vector2i current = get();

        VerticalLayout layout = new(control);

        NumericUpDown x = new(layout)
        {
            Min = 1,
            Max = short.MaxValue,
            Step = 1,
            Value = current.X,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        Control.Used(new Label(layout)
        {
            Text = Language.SizeBy,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        NumericUpDown y = new(layout)
        {
            Min = 1,
            Max = short.MaxValue,
            Step = 1,
            Value = current.Y,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        if (update != null)
        {
            Button updateButton = new(layout)
            {
                Text = Language.Update,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            updateButton.Clicked += (_, _) =>
            {
                Vector2i value = update();
                x.Value = value.X;
                y.Value = value.Y;
            };
        }

        x.ValueChanged += OnValueChanged;
        y.ValueChanged += OnValueChanged;

        return;

        void OnValueChanged(object? sender, EventArgs args)
        {
            var value = new Vector2i((int) Math.Round(x.Value), (int) Math.Round(y.Value));
            set(value);
            Provider.Validate();
        }
    }
}
