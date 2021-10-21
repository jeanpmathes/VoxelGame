// <copyright file="KeyOrButtonSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Input.Internal;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class KeyOrButtonSetting : Setting
    {
        private readonly Func<KeyOrButton> get;
        private readonly Action<KeyOrButton> set;

        internal KeyOrButtonSetting(string name, Func<KeyOrButton> get, Action<KeyOrButton> set)
        {
            this.get = get;
            this.set = set;

            Name = name;
        }

        protected override string Name { get; }

        private protected override void FillControl(ControlBase control, Context context)
        {
            Button rebind = new(control)
            {
                Text = get().ToString()
            };

            rebind.Clicked += (_, _) => // Using pressed instead of clicked causes that the mouse is used as new bind.
            {
                rebind.Text = Language.PressAnyKeyOrButton;

                context.Input.ListenForAnyKeyOrButton(
                    keyOrButton =>
                    {
                        set(keyOrButton);
                        rebind.Text = keyOrButton.ToString();
                    });
            };
        }
    }
}
