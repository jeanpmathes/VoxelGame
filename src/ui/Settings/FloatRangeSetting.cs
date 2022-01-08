// <copyright file="FloatRangeSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings
{
    /// <summary>
    ///     Settings that allow to pick a float value in a range.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class FloatRangeSetting : Setting
    {
        private readonly Func<float> get;

        private readonly float max;
        private readonly float min;
        private readonly Action<float> set;


        internal FloatRangeSetting(string name, float min, float max, Func<float> get, Action<float> set)
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
            VerticalLayout layout = new(control);

            HorizontalSlider floatRange = new(layout)
            {
                Min = min,
                Max = max,
                Value = get()
            };

            Label value = new(layout)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            SetText();

            Button select = new(layout)
            {
                Text = Language.Select
            };

            select.Pressed += (_, _) =>
            {
                set(floatRange.Value);
                Provider.Validate();
            };

            floatRange.ValueChanged += (_, _) => { SetText(); };

            void SetText()
            {
                value.Text = $"{floatRange.Value:F}";
            }
        }
    }
}