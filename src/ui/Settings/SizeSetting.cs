// <copyright file="SizeSetting.cs" company="VoxelGame">
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
internal sealed class SizeSetting : Setting
{
    private readonly Func<Vector2i> get;
    private readonly Action<Vector2i> set;
    private readonly Func<Vector2i>? update;

    internal SizeSetting(String name, Func<Vector2i> get, Action<Vector2i> set, Func<Vector2i>? update)
    {
        this.get = get;
        this.set = set;
        this.update = update;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

    private protected override void FillControl(ControlBase control, Context context)
    {
        Vector2i current = get();

        VerticalLayout layout = new(control);

        NumericUpDown x = new(layout)
        {
            Min = 1,
            Max = Int16.MaxValue,
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
            Max = Int16.MaxValue,
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

            updateButton.Released += (_, _) =>
            {
                Vector2i value = update();
                x.Value = value.X;
                y.Value = value.Y;
            };
        }

        x.ValueChanged += OnValueChanged;
        y.ValueChanged += OnValueChanged;

        return;

        void OnValueChanged(Object? sender, EventArgs args)
        {
            var value = new Vector2i((Int32) Math.Round(x.Value), (Int32) Math.Round(y.Value));
            set(value);
            Validator.Validate();
        }
    }
}
