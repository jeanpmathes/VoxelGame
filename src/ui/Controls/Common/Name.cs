﻿// <copyright file="Name.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
