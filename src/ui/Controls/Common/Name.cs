// <copyright file="Name.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A label for a name, allowing to rename it.
/// </summary>
public class Name : ControlBase
{
    private readonly Label label;

    private Func<string, bool> validator = _ => true;

    /// <inheritdoc />
    internal Name(Context context, ControlBase menu, ControlBase parent) : base(parent)
    {
        HorizontalLayout layout = new(this);

        label = new Label(layout);

        Button rename = context.CreateIconButton(layout, context.Resources.RenameIcon, Language.Rename, isSmall: true);

        rename.Released += (_, _) =>
        {
            Window window = new(menu)
            {
                Title = Language.Rename,
                MinimumSize = new Size(width: 200, height: 100),

                StartPosition = StartPosition.CenterCanvas,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                Resizing = Resizing.None,
                IsDraggingEnabled = false,
                IsClosable = false,
                DeleteOnClose = true
            };

            Context.MakeModal(window);

            VerticalLayout windowLayout = new(window)
            {
                Margin = Margin.Ten,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            TextBox name = new(windowLayout)
            {
                Text = label.Text
            };

            Empty space = new(windowLayout)
            {
                Padding = Padding.Five
            };

            Control.Used(space);

            DockLayout buttons = new(windowLayout)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Button ok = new(buttons)
            {
                Text = Language.Ok,
                Dock = Dock.Right
            };

            ok.Released += (_, _) =>
            {
                if (!validator(name.Text)) return;

                string oldName = label.Text;
                label.Text = name.Text;

                if (oldName != name.Text) NameChanged?.Invoke(this, EventArgs.Empty);

                window.Close();
            };

            Button cancel = new(buttons)
            {
                Text = Language.Cancel,
                Dock = Dock.Left
            };

            cancel.Released += (_, _) => window.Close();

            name.TextChanged += (_, _) =>
            {
                bool valid = validator(name.Text);

                name.TextColor = valid ? Colors.Primary : Colors.Error;

                ok.IsDisabled = !valid;
                ok.Redraw();
            };
        };
    }

    /// <summary>
    ///     Gets or sets the text of the label.
    /// </summary>
    public string Text
    {
        get => label.Text;
        init => label.Text = value;
    }

    /// <summary>
    ///     Set the validator for the name.
    /// </summary>
    /// <param name="newValidator">The new validator function.</param>
    public void SetValidator(Func<string, bool> newValidator)
    {
        validator = newValidator;
    }

    /// <summary>
    ///     Invoked when the name is changed.
    /// </summary>
    public event EventHandler? NameChanged;
}
