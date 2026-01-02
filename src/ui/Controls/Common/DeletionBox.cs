// <copyright file="DeletionBox.cs" company="VoxelGame">
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
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using Gwen.Net.RichText;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A box that first asks for confirmation before deletion is started.
///     When confirmed, it stays open until deletion is finished.
/// </summary>
public sealed class DeletionBox : Window
{
    /// <summary>
    ///     Create a new deletion box.
    /// </summary>
    public DeletionBox(ControlBase parent, Parameters parameters, Actions actions) : base(parent)
    {
        // Initial code from MessageBox.cs in the Gwen.Net library.

        Canvas canvas = GetCanvas();

        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        MaximumSize = new Size((Int32) (canvas.ActualWidth * 0.8), canvas.ActualHeight);
        StartPosition = StartPosition.CenterParent;

        Title = parameters.Title;

        DeleteOnClose = true;
        IsClosable = false;

        DockLayout layout = new(this);

        Empty textSlot = new(layout)
        {
            Dock = Dock.Fill
        };

        RichLabel text = new(textSlot)
        {
            Dock = Dock.Fill,
            Margin = Margin.Ten,
            Document = new Document(parameters.Text)
        };

        HorizontalLayout buttons = new(layout)
        {
            Dock = Dock.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Button cancel = CreateButton(buttons, Language.Cancel);

        cancel.Released += (_, _) =>
        {
            actions.Cancel();
            Close();
        };

        Button delete = CreateButton(buttons, Language.Delete);

        delete.TextColorOverride = Colors.Danger;

        delete.Released += (_, _) =>
        {
            buttons.RemoveChild(cancel, dispose: true);
            buttons.RemoveChild(delete, dispose: true);

            textSlot.RemoveChild(text, dispose: true);

            Label label = new(textSlot)
            {
                Margin = Margin.Ten,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

                Alignment = Alignment.Center,
                Text = Texts.FormatWithStatus(Language.Delete, Status.Running)
            };

            Button ok = CreateButton(buttons, Language.Ok);

            ok.Disable();
            ok.Redraw();

            actions.Delete(status =>
            {
                label.Text = Texts.FormatWithStatus(Language.Delete, status);
                label.TextColor = status == Status.ErrorOrCancel ? Colors.Error : Colors.Primary;

                ok.Enable();
                ok.Redraw();

                ok.Released += (_, _) => Close();
            });
        };
    }

    private static Button CreateButton(ControlBase parent, String text)
    {
        Button button = new(parent)
        {
            Width = 70,
            Margin = Margin.Five,
            Text = text
        };

        return button;
    }

    /// <summary>
    ///     The basic parameters defining the deletion box.
    /// </summary>
    /// <param name="Title">The title of the deletion box.</param>
    /// <param name="Text">The text of the deletion box.</param>
    public record Parameters(String Title, String Text);

    /// <summary>
    ///     The actions associated with the deletion box.
    /// </summary>
    /// <param name="Cancel">Will be invoked when the deletion box is canceled.</param>
    /// <param name="Delete">
    ///     Will be invoked when the deletion box is confirmed and deletion should be started.
    ///     Another action is passed, allowing to set the status when deletion is finished.
    /// </param>
    public record Actions(Action Cancel, Action<Action<Status>> Delete);
}
