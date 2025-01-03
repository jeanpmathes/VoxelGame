// <copyright file="WorldElement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;
using Colors = VoxelGame.UI.Utilities.Colors;

namespace VoxelGame.UI.Controls.Worlds;

/// <summary>
///     Represents a world element in the world selection menu.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
public sealed class WorldElement : VerticalLayout
{
    /// <summary>
    ///     Creates a new instance of the <see cref="WorldElement" /> class.
    /// </summary>
    /// <param name="table">The table to add this element to.</param>
    /// <param name="world">Data of the world to represent.</param>
    /// <param name="worldProvider">Provides operations related to worlds.</param>
    /// <param name="context">The context in which the user interface is running.</param>
    /// <param name="menu">
    ///     The higher level world selection menu that this element is part of.
    ///     Used as a parent to open windows and modals.
    /// </param>
    internal WorldElement(Table table, IWorldProvider.IWorldInfo world, IWorldProvider worldProvider, Context context, WorldSelection menu) : base(table.AddRow())
    {
        var row = (Parent as TableRow)!;

        row.SetCellContents(column: 0, this);

        DockLayout top = new(this);

        Name name = new(top, context, menu)
        {
            Text = world.Name
        };

        name.SetValidator(worldProvider.IsWorldNameValid);

        name.NameChanged += (_, _) =>
        {
            worldProvider.RenameWorld(world, name.Text);
        };

        IconButton favorite = context.CreateIconButton(top, Icons.Instance.StarEmpty, Language.Favorite, isSmall: true);
        favorite.ToggledOnIconName = Icons.Instance.StarFilled;
        favorite.ToggledOffIconName = Icons.Instance.StarEmpty;
        favorite.IsToggle = true;
        favorite.ShouldDrawToggleDepressedWhenOn = false;
        favorite.Dock = Dock.Right;

        favorite.ToggleState = world.IsFavorite;

        favorite.Toggled += (_, _) =>
        {
            worldProvider.SetFavorite(world, favorite.ToggleState);
            menu.UpdateList();
        };

        DockLayout bottom = new(this);

        VerticalLayout infoPanel = new(bottom)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        Label creation = new(infoPanel)
        {
            Text = $"{Language.CreatedOn}: {Texts.FormatDateTime(world.DateTimeOfCreation)}",
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
            Text = $"{Language.LastLoaded}: {Texts.FormatTimeSinceEvent(world.DateTimeOfLastLoad, out Boolean hasOccurred)}",
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
            Text = world.Version,
            Font = context.Fonts.Small,
            TextColor = ApplicationInformation.Instance.Version == world.Version ? Colors.Good : Colors.Bad
        };

        Control.Used(version);

        LinkLabel file = new(infoPanel)
        {
            Text = world.Directory.FullName.Ellipsis(maxLength: 150),

            Font = context.Fonts.Path,
            HoverFont = context.Fonts.PathU,

            TextColor = Colors.Linkified(Colors.Secondary)
        };

        file.LinkClicked += (_, _) => OS.Start(world.Directory);

        Control.Used(file);

        WorldActions actions = new(bottom, world, worldProvider, () => table.RemoveRow(row), context, menu);

        Control.Used(actions);
    }
}
