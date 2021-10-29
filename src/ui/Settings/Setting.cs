// <copyright file="Setting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;
using Gwen.Net.Control;
using VoxelGame.Input.Internal;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings
{
    public abstract class Setting
    {
        protected abstract string Name { get; }

        protected ISettingsProvider Provider { get; init; } = null!;

        internal ControlBase CreateControl(ControlBase parent, Context context)
        {
            GroupBox box = new(parent)
            {
                Text = Name
            };

            FillControl(box, context);

            return box;
        }

        internal virtual void Validate() {}

        private protected abstract void FillControl(ControlBase control, Context context);

        public static Setting CreateKeyOrButtonSetting(ISettingsProvider provider, string name, Func<KeyOrButton> get,
            Action<KeyOrButton> set, Func<bool> validate, Action reset)
        {
            return new KeyOrButtonSetting(name, get, set, validate, reset)
            {
                Provider = provider
            };
        }

        public static Setting CreateIntegerSetting(ISettingsProvider provider, string name, Func<int> get,
            Action<int> set,
            int min = int.MinValue, int max = int.MaxValue)
        {
            return new IntegerSetting(name, min, max, get, set)
            {
                Provider = provider
            };
        }

        public static Setting CreateColorSetting(ISettingsProvider provider, string name, Func<Color> get,
            Action<Color> set)
        {
            return new ColorSettings(name, get, set)
            {
                Provider = provider
            };
        }

        public static Setting CreateFloatRangeSetting(ISettingsProvider provider, string name, Func<float> get,
            Action<float> set,
            float min = float.MinValue, float max = float.MaxValue)
        {
            return new FloatRangeSetting(name, min, max, get, set)
            {
                Provider = provider
            };
        }
    }
}