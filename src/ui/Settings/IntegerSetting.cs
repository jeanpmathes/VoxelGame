// <copyright file="IntegerSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick an integer value.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class IntegerSetting : Setting
{
    private readonly Func<int> get;
    private readonly int max;

    private readonly int min;
    private readonly Action<int> set;

    internal IntegerSetting(string name, int min, int max, Func<int> get, Action<int> set)
    {
        this.get = get;
        this.set = set;

        this.min = min;
        this.max = max;

        Name = name;
    }

    protected override string Name { get; }

    private protected override void FillControl(ControlBase control, Context context)
    {
        NumericUpDown integer = new(control)
        {
            Min = min,
            Max = max,
            Step = 1,
            Value = get()
        };

        integer.ValueChanged += (_, _) =>
        {
            var value = (int) Math.Round(integer.Value);
            set(value);
            Provider.Validate();
        };
    }
}
