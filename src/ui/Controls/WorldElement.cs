// <copyright file="WorldElement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls;

/// <summary>
///     Represents a world element in the world selection menu.
/// </summary>
internal sealed class WorldElement : GroupBox
{
    /// <summary>
    ///     Creates a new instance of the <see cref="WorldElement" /> class.
    /// </summary>
    /// <param name="info">Information about the world to display.</param>
    /// <param name="path">The path to the world.</param>
    /// <param name="context">The context in which the user interface is running.</param>
    /// <param name="parent">The parent control.</param>
    internal WorldElement(WorldInformation info, DirectoryInfo path, Context context, ControlBase parent) : base(parent)
    {
        Text = info.Name;

        DockLayout layout = new(this);

        VerticalLayout infoPanel = new(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        Label date = new(infoPanel)
        {
            Text = $"{info.Creation.ToLongDateString()} - {info.Creation.ToLongTimeString()}",
            Font = context.Fonts.Small
        };

        Control.Used(date);

        Label version = new(infoPanel)
        {
            Text = info.Version,
            Font = context.Fonts.Small,
            TextColor = ApplicationInformation.Instance.Version == info.Version ? Color.Green : Color.Red
        };

        Control.Used(version);

        Label file = new(infoPanel)
        {
            Text = path.FullName,
            Font = context.Fonts.Path,
            TextColor = Color.Grey
        };

        Control.Used(file);

        HorizontalLayout buttons = new(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        Button load = new(buttons)
        {
            ImageName = context.Resources.LoadIcon,
            ImageSize = new Size(width: 40, height: 40),
            ToolTipText = Language.Load

        };

        load.Released += (_, _) => OnLoad(this, EventArgs.Empty);

        Button delete = new(buttons)
        {
            ImageName = context.Resources.DeleteIcon,
            ImageSize = new Size(width: 40, height: 40),
            ToolTipText = Language.Delete
        };

        delete.Released += (_, _) => OnDelete(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Invoked when a load operation is requested.
    /// </summary>
    public event EventHandler OnLoad = delegate {};

    /// <summary>
    ///     Invoked when a delete operation is requested.
    /// </summary>
    public event EventHandler OnDelete = delegate {};
}
