// <copyright file="EnumerationSetting.cs" company="VoxelGame">
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
using Gwen.Net.Control;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Setting that allows to pick from an enum.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class EnumerationSetting<T> : Setting where T : Enum
{
    private readonly Func<T> get;
    private readonly Action<T> set;

    private readonly (T, String)[] values;
    private readonly MenuItem[] items;

    internal EnumerationSetting(String name, (T, String)[] values, Func<T> get, Action<T> set)
    {
        this.get = get;
        this.set = set;

        this.values = values;
        items = new MenuItem[values.Length];

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

    private protected override void FillControl(ControlBase control, Context context)
    {
        ComboBox combo = new(control);

        for (var index = 0; index < values.Length; index++)
        {
            (T value, String label) = values[index];
            items[index] = combo.AddItem(label, "", value);
        }

        T currentValue = get();
        var selectedIndex = 0;

        for (var index = 0; index < values.Length; index++)
        {
            (T value, _) = values[index];

            if (!Equals(currentValue, value)) continue;

            selectedIndex = index;

            break;
        }

        combo.SelectedItem = items[selectedIndex];

        combo.ItemSelected += (_, args) => set((T) ((MenuItem) args.SelectedItem).UserData!);
    }
}
