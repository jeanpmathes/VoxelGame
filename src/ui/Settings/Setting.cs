// <copyright file="Setting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;
using Gwen.Net.Control;
using VoxelGame.Input.Internal;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings
{
    public abstract class Setting
    {
        protected abstract string Name { get; }

        internal ControlBase CreateControl(ControlBase parent, Context context)
        {
            GroupBox box = new(parent)
            {
                Text = Name
            };

            FillControl(box, context);

            return box;
        }

        private protected abstract void FillControl(ControlBase control, Context context);

        public static Setting CreateKeyOrButtonSetting(string name, Func<KeyOrButton> get, Action<KeyOrButton> set)
        {
            return new KeyOrButtonSetting(name, get, set);
        }

        public static Setting CreateIntegerSetting(string name, Func<int> get, Action<int> set,
            int min = int.MinValue, int max = int.MaxValue)
        {
            return new IntegerSetting(name, min, max, get, set);
        }

        public static Setting CreateColorSetting(string name, Func<Color> get, Action<Color> set)
        {
            return new ColorSettings(name, get, set);
        }

        public static Setting CreateFloatRangeSetting(string name, Func<float> get, Action<float> set,
            float min = float.MinValue, float max = float.MaxValue)
        {
            return new FloatRangeSetting(name, min, max, get, set);
        }
    }
}