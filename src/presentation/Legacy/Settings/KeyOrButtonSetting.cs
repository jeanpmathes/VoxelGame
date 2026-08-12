// <copyright file="KeyOrButtonSetting.cs" company="VoxelGame">
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
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Graphics.Input;
using VoxelGame.Presentation.Legacy.UserInterfaces;
using VoxelGame.Presentation.Legacy.Utilities;

namespace VoxelGame.Presentation.Legacy.Settings;

/// <summary>
///     Settings that allow to pick a key or button.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
[SuppressMessage("ReSharper", "CA1001")]
#pragma warning disable S2931
internal sealed class KeyOrButtonSetting : Setting
#pragma warning restore S2931
{
    private readonly Func<Action<KeyOrButtonCombination>, IDisposable> beginCapture;
    private readonly Func<KeyOrButtonCombination> get;
    private readonly Action reset;
    private readonly Action<KeyOrButtonCombination> set;

    private readonly Func<Boolean> validate;

    private CaptureButton rebind = null!;

    internal KeyOrButtonSetting(String name, Func<KeyOrButtonCombination> get, Action<KeyOrButtonCombination> set,
        Func<Action<KeyOrButtonCombination>, IDisposable> beginCapture, Func<Boolean> validate, Action reset)
    {
        this.get = get;
        this.set = set;
        this.beginCapture = beginCapture;

        this.validate = validate;
        this.reset = reset;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

    private protected override void FillControl(ControlBase control, Context context)
    {
        DockLayout layout = new(control);

        rebind = new CaptureButton(layout)
        {
            Text = get().ToString(),
            Dock = Dock.Fill
        };

        rebind.Released += (_, _) =>
        {
            CloseHandel modal = Modals.OpenBlockingModal(rebind, Language.PressAnyKeyOrButton, context);

            IDisposable capture = beginCapture(combination =>
            {
                rebind.ReleaseCapture();
                modal.Close();

                set(combination);
                rebind.Text = combination.ToString();

                Validator.Validate();
            });

            rebind.OwnCapture(capture);
        };

        Button resetBind = new(layout)
        {
            ImageName = Icons.Instance.Reset,
            Size = new Size(width: 40, height: 40),
            ToolTipText = Language.Reset,
            Dock = Dock.Right
        };

        resetBind.Released += (_, _) =>
        {
            reset();
            rebind.Text = get().ToString();

            Validator.Validate();
        };
    }

    internal override void Validate()
    {
        Boolean valid = validate();
        rebind.TextColorOverride = valid ? Colors.Primary : Colors.Error;
    }

    private sealed class CaptureButton(ControlBase parent) : Button(parent)
    {
        private IDisposable? capture;

        internal void OwnCapture(IDisposable registration)
        {
            capture?.Dispose();
            capture = registration;
        }

        internal void ReleaseCapture()
        {
            capture = null;
        }

        public override void Dispose()
        {
            capture?.Dispose();
            capture = null;

            base.Dispose();
        }
    }
}
