﻿// <copyright file="KeyOrButtonSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Input.Internal;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Settings
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    [SuppressMessage("ReSharper", "CA1001")]
    internal class KeyOrButtonSetting : Setting
    {
        private readonly Func<KeyOrButton> get;
        private readonly Action<KeyOrButton> set;
        private readonly Func<bool> validate;

        private Button rebind = null!;

        internal KeyOrButtonSetting(string name, Func<KeyOrButton> get, Action<KeyOrButton> set, Func<bool> validate)
        {
            this.get = get;
            this.set = set;
            this.validate = validate;

            Name = name;
        }

        protected override string Name { get; }

        private protected override void FillControl(ControlBase control, Context context)
        {
            rebind = new Button(control)
            {
                Text = get().ToString()
            };

            rebind.Clicked += (_, _) => // Using pressed instead of clicked causes that the mouse is used as new bind.
            {
                CloseHandel modal = Modals.OpenBlockingModal(rebind, Language.PressAnyKeyOrButton);

                context.Input.ListenForAnyKeyOrButton(
                    keyOrButton =>
                    {
                        modal.Close();

                        set(keyOrButton);
                        rebind.Text = keyOrButton.ToString();

                        Provider.Validate();
                    });
            };
        }

        internal override void Validate()
        {
            bool valid = validate();
            rebind.TextColorOverride = valid ? Color.White : Color.Red;
        }
    }
}
