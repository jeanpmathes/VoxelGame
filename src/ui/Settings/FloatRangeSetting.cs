﻿// <copyright file="FloatRangeSetting.cs" company="VoxelGame">
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
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick a float value in a range.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class FloatRangeSetting : Setting
{
    private readonly Func<float> get;
    private readonly Action<float> set;

    private readonly float min;
    private readonly float max;

    private readonly bool percentage;
    private readonly float? step;

    internal FloatRangeSetting(string name, float min, float max, bool percentage, float? step, Func<float> get, Action<float> set)
    {
        this.get = get;
        this.set = set;

        this.min = min;
        this.max = max;

        this.percentage = percentage;
        this.step = step;

        Name = name;
    }

    protected override string Name { get; }

    private protected override void FillControl(ControlBase control, Context context)
    {
        VerticalLayout layout = new(control);

        HorizontalSlider floatRange = new(layout)
        {
            Min = min,
            Max = max,
            Value = get()
        };

        if (step is {} stepValue)
        {
            floatRange.NotchCount = (int) ((max - min) / stepValue);
            floatRange.SnapToNotches = true;
        }

        Label value = new(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        void SetText()
        {
            value.Text = percentage
                ? $"{floatRange.Value:P0}"
                : $"{floatRange.Value:F2}";
        }

        SetText();

        Button select = new(layout)
        {
            Text = Language.Select
        };

        select.Released += (_, _) =>
        {
            set(floatRange.Value);
            Validator.Validate();

            select.Disable();
        };

        select.Disable();

        floatRange.ValueChanged += (_, _) =>
        {
            select.Enable();
            select.Redraw();

            SetText();
        };
    }
}
