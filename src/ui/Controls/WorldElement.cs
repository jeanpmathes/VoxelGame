// <copyright file="WorldElement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;
using Colors = VoxelGame.UI.Utilities.Colors;

namespace VoxelGame.UI.Controls;

/// <summary>
///     Represents a world element in the world selection menu.
/// </summary>
public sealed class WorldElement : GroupBox
{
    /// <summary>
    ///     Creates a new instance of the <see cref="WorldElement" /> class.
    /// </summary>
    /// <param name="list">The list which is the direct parent of this element.</param>
    /// <param name="world">Data of the world to represent.</param>
    /// <param name="worldProvider">Provides operations related to worlds.</param>
    /// <param name="context">The context in which the user interface is running.</param>
    /// <param name="menu">
    ///     A higher level menu control that this element is part of.
    ///     Used as a parent to open windows and modals.
    /// </param>
    internal WorldElement(ControlBase list, WorldData world, IWorldProvider worldProvider, Context context, ControlBase menu) : base(list)
    {
        Text = world.Information.Name;

        DockLayout layout = new(this);

        VerticalLayout infoPanel = new(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        Label creation = new(infoPanel)
        {
            Text = $"{Language.CreatedOn}: {Texts.FormatDateTime(world.Information.Creation)}",
            Font = context.Fonts.Small,
            TextColor = Colors.Secondary
        };

        Control.Used(creation);

        HorizontalLayout last = new(infoPanel)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        Control.Used(last);

        Label text = new(last)
        {
            Text = $"{Language.LastLoaded}: {Texts.FormatTimeSinceEvent(worldProvider.GetDateTimeOfLastLoad(world), out bool hasOccurred)}",
            Font = context.Fonts.Small,
            TextColor = Colors.Secondary
        };

        Control.Used(text);

        Label marker = new(last)
        {
            Text = hasOccurred ? "" : "  !!!  ",
            Font = context.Fonts.Small,
            TextColor = hasOccurred ? Colors.Invisible : Colors.Interesting
        };

        Control.Used(marker);

        Label version = new(infoPanel)
        {
            Text = world.Information.Version,
            Font = context.Fonts.Small,
            TextColor = ApplicationInformation.Instance.Version == world.Information.Version ? Colors.Good : Colors.Bad
        };

        Control.Used(version);

        LinkLabel file = new(infoPanel)
        {
            Text = world.WorldDirectory.FullName,

            Font = context.Fonts.Path,
            HoverFont = context.Fonts.PathU,

            TextColor = Colors.Linkified(Colors.Secondary)
        };

        file.LinkClicked += (_, _) => OS.Start(world.WorldDirectory);

        Control.Used(file);

        HorizontalLayout buttons = new(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        Button load = new(buttons)
        {
            ImageName = context.Resources.LoadIcon,
            ImageSize = Context.DefaultIconSize,
            ToolTipText = Language.Load
        };

        load.Released += (_, _) => worldProvider.BeginLoadingWorld(world);

        Button delete = new(buttons)
        {
            ImageName = context.Resources.DeleteIcon,
            ImageSize = Context.DefaultIconSize,
            ToolTipText = Language.Delete
        };

        delete.Released += (_, _) => Modals.OpenDeletionModal(
            menu,
            new DeletionBox.Parameters("", Language.DeleteWorldQuery),
            new DeletionBox.Actions(
                () => {},
                close =>
                {
                    list.RemoveChild(this, dispose: true);

                    worldProvider.DeleteWorld(world).OnCompletion(op =>
                    {
                        close(op.Status);
                    });
                }));
    }
}
