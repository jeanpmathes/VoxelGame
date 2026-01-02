// <copyright file="FloatRangeSetting.cs" company="VoxelGame">
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
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick a float value in a range.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class FloatRangeSetting : Setting
{
    private readonly Func<Single> get;
    private readonly Single max;

    private readonly Single min;

    private readonly Boolean percentage;
    private readonly Action<Single> set;
    private readonly Single? step;

    internal FloatRangeSetting(String name, Single min, Single max, Boolean percentage, Single? step, Func<Single> get, Action<Single> set)
    {
        this.get = get;
        this.set = set;

        this.min = min;
        this.max = max;

        this.percentage = percentage;
        this.step = step;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

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
            floatRange.NotchCount = (Int32) ((max - min) / stepValue);
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
