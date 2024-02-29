// <copyright file="NameBox.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A box that asks for a (new) name.
///     When done, the box can also show the status of a following operation.
/// </summary>
public class NameBox : Window
{
    /// <summary>
    ///     Create a new name box.
    /// </summary>
    public NameBox(ControlBase parent, Parameters parameters, Actions actions) : base(parent)
    {
        Title = parameters.Title;
        MinimumSize = new Size(width: 200, height: 100);

        StartPosition = StartPosition.CenterCanvas;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        Resizing = Resizing.None;
        IsDraggingEnabled = false;
        IsClosable = false;
        DeleteOnClose = true;

        VerticalLayout windowLayout = new(this)
        {
            Margin = Margin.Ten,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        TextBox name = new(windowLayout);

        Label status = new(windowLayout);
        status.Hide();

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

        Button cancel = new(buttons)
        {
            Text = Language.Cancel,
            Dock = Dock.Left
        };

        ok.Released += OnNameSet;

        cancel.Released += (_, _) => Close();

        name.TextChanged += (_, _) =>
        {
            bool valid = actions.Validator(name.Text);
            bool different = name.Text != parameters.Initial;

            name.TextColor = valid ? Colors.Primary : Colors.Error;

            ok.IsDisabled = !valid || !different;
            ok.Redraw();
        };

        name.Text = parameters.Initial;

        void OnNameSet(ControlBase controlBase, EventArgs eventArgs)
        {
            if (!actions.Validator(name.Text)) return;
            if (name.Text == parameters.Initial) return;

            Operation op = actions.Apply(name.Text);

            if (op.IsRunning)
            {
                ok.Disable();
                ok.Redraw();

                cancel.Disable();
                cancel.Redraw();

                status.Text = Texts.FormatOperation(parameters.Title, Status.Running);
                status.TextColor = Colors.Secondary;

                status.Show();

                op.OnCompletion(_ =>
                {
                    status.Text = Texts.FormatOperation(parameters.Title, op.Status);
                    status.TextColor = op.IsOk ? Colors.Secondary : Colors.Error;

                    ok.Enable();
                    ok.Redraw();

                    ok.Released -= OnNameSet;
                    ok.Released += (_, _) => Close();
                });
            }
            else
            {
                Close();
            }
        }
    }

    /// <summary>
    ///     The parameters for the name box.
    /// </summary>
    /// <param name="Title">The title of the box.</param>
    /// <param name="Initial">The initial string for the name, can be empty.</param>
    public record Parameters(string Title, string Initial);

    /// <summary>
    ///     The actions for the name box.
    /// </summary>
    /// <param name="Apply">Invoked when the name should be applied. This can only be the case if the name is valid and new.</param>
    /// <param name="Validator">A function that checks whether a name is valid.</param>
    public record Actions(Func<string, Operation> Apply, Func<string, bool> Validator);
}
