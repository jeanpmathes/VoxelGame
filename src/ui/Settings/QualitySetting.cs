// <copyright file="QualitySetting.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick a quality level.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class QualitySetting : Setting
{
    private readonly Func<Quality> get;

    private readonly MenuItem[] items = new MenuItem[Qualities.Count];
    private readonly Action<Quality> set;

    internal QualitySetting(String name, Func<Quality> get, Action<Quality> set)
    {
        this.get = get;
        this.set = set;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

    private protected override void FillControl(ControlBase control, Context context)
    {
        ComboBox qualitySelection = new(control);

        foreach (Quality quality in Qualities.All())
            items[(Int32) quality] = qualitySelection.AddItem(quality.Name(), "", quality);

        qualitySelection.SelectedItem = items[(Int32) get()];

        qualitySelection.ItemSelected += (_, args) => set((Quality) ((MenuItem) args.SelectedItem).UserData!);
    }
}
