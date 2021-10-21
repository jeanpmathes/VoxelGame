// <copyright file="Setting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
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
    }
}