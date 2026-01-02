// <copyright file="Name.cs" company="VoxelGame">
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
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A label for a name, allowing to rename it.
/// </summary>
public class Name : ControlBase
{
    private readonly Label label;

    private Func<String, Boolean> validator = _ => true;

    /// <inheritdoc />
    internal Name(ControlBase parent, Context context, ControlBase menu) : base(parent)
    {
        HorizontalLayout layout = new(this);

        label = new Label(layout);

        Button rename = context.CreateIconButton(layout, Icons.Instance.Rename, Language.Rename, isSmall: true);

        rename.Released += (_, _) =>
        {
            Modals.OpenNameModal(menu,
                new NameBox.Parameters(Language.Rename, label.Text),
                new NameBox.Actions(
                    name =>
                    {
                        label.Text = name;

                        NameChanged?.Invoke(this, EventArgs.Empty);

                        return Operations.CreateDone();
                    },
                    validator),
                context);
        };
    }

    /// <summary>
    ///     Gets or sets the text of the label.
    /// </summary>
    public String Text
    {
        get => label.Text;
        init => label.Text = value;
    }

    /// <summary>
    ///     Set the validator for the name.
    /// </summary>
    /// <param name="newValidator">The new validator function.</param>
    public void SetValidator(Func<String, Boolean> newValidator)
    {
        validator = newValidator;
    }

    /// <summary>
    ///     Invoked when the name is changed.
    /// </summary>
    public event EventHandler? NameChanged;
}
