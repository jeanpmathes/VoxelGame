﻿// <copyright file="KeyOrButtonSetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Graphics.Definition;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Settings;

/// <summary>
///     Settings that allow to pick a key or button.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
[SuppressMessage("ReSharper", "CA1001")]
#pragma warning disable S2931
internal class KeyOrButtonSetting : Setting
#pragma warning restore S2931
{
    private readonly Func<VirtualKeys> get;
    private readonly Action reset;
    private readonly Action<VirtualKeys> set;

    private readonly Func<Boolean> validate;

    private Button rebind = null!;

    internal KeyOrButtonSetting(String name, Func<VirtualKeys> get, Action<VirtualKeys> set, Func<Boolean> validate,
        Action reset)
    {
        this.get = get;
        this.set = set;

        this.validate = validate;
        this.reset = reset;

        Name = name;
    }

    protected override String Name { get; }

    public override Object Value => get();

    private protected override void FillControl(ControlBase control, Context context)
    {
        DockLayout layout = new(control);

        rebind = new Button(layout)
        {
            Text = get().ToString(),
            Dock = Dock.Fill
        };

        rebind.Released += (_, _) =>
        {
            CloseHandel modal = Modals.OpenBlockingModal(rebind, Language.PressAnyKeyOrButton, context);

            context.Input.ListenForAnyKeyOrButton(
                keyOrButton =>
                {
                    modal.Close();

                    set(keyOrButton);
                    rebind.Text = keyOrButton.ToString();

                    Validator.Validate();
                });
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
}
