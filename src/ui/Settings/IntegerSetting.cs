// <copyright file="IntegerSetting.cs" company="VoxelGame">
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
///     Settings that allow to pick an integer value.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class IntegerSetting : Setting
{
    private readonly Func<Int32> get;
    private readonly Int32 max;

    private readonly Int32 min;
    private readonly Action<Int32> set;

    internal IntegerSetting(String name, Int32 min, Int32 max, Func<Int32> get, Action<Int32> set)
    {
        this.get = get;
        this.set = set;

        this.min = min;
        this.max = max;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

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
            var value = (Int32) Math.Round(integer.Value);
            set(value);
            Validator.Validate();
        };
    }
}
