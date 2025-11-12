// <copyright file="BooleanSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick a quality level.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class BooleanSetting : Setting
{
    private readonly Func<Boolean> get;
    private readonly Action<Boolean> set;

    internal BooleanSetting(String name, Func<Boolean> get, Action<Boolean> set)
    {
        this.get = get;
        this.set = set;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

    private protected override void FillControl(ControlBase control, Context context)
    {
        CheckBox checkBox = new(control);

        checkBox.IsChecked = get();
        checkBox.CheckChanged += (_, _) => set(checkBox.IsChecked);
    }
}
