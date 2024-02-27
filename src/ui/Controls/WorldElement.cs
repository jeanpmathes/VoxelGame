// <copyright file="WorldElement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
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
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
public sealed class WorldElement : VerticalLayout
{
    private readonly WorldData world;
    private readonly IWorldProvider worldProvider;

    private readonly Context context;
    private readonly ControlBase menu;

    private Window? worldInfoWindow;
    private CancellationTokenSource? infoCancellation;

    /// <summary>
    ///     Creates a new instance of the <see cref="WorldElement" /> class.
    /// </summary>
    /// <param name="table">The table to add this element to.</param>
    /// <param name="world">Data of the world to represent.</param>
    /// <param name="worldProvider">Provides operations related to worlds.</param>
    /// <param name="context">The context in which the user interface is running.</param>
    /// <param name="menu">
    ///     A higher level menu control that this element is part of.
    ///     Used as a parent to open windows and modals.
    /// </param>
    internal WorldElement(Table table, WorldData world, IWorldProvider worldProvider, Context context, ControlBase menu) : base(table.AddRow())
    {
        this.world = world;
        this.worldProvider = worldProvider;

        this.context = context;
        this.menu = menu;

        var row = (Parent as TableRow)!;

        row.SetCellContents(column: 0, this);

        Name name = new(context, menu, this)
        {
            Text = world.Information.Name
        };

        name.SetValidator(worldProvider.IsWorldNameValid);

        name.NameChanged += (_, _) =>
        {
            worldProvider.RenameWorld(world, name.Text);
        };

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
            Text = world.WorldDirectory.FullName.Ellipsis(maxLength: 150),

            Font = context.Fonts.Path,
            HoverFont = context.Fonts.PathU,

            TextColor = Colors.Linkified(Colors.Secondary)
        };

        file.LinkClicked += (_, _) => OS.Start(world.WorldDirectory);

        Control.Used(file);

        HorizontalLayout buttons = new(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        Button info = context.CreateIconButton(buttons, context.Resources.InfoIcon, Language.Info);
        info.Released += (_, _) => OpenWorldInfoWindow(info);

        Button load = context.CreateIconButton(buttons, context.Resources.LoadIcon, Language.Load);
        load.Released += (_, _) => worldProvider.BeginLoadingWorld(world);

        Button delete = context.CreateIconButton(buttons, context.Resources.DeleteIcon, Language.Delete, Colors.Danger);

        delete.Released += (_, _) => Modals.OpenDeletionModal(
            menu,
            new DeletionBox.Parameters("", Language.DeleteWorldQuery),
            new DeletionBox.Actions(
                () => {},
                close =>
                {
                    table.RemoveRow(row);

                    worldProvider.DeleteWorld(world).OnCompletion(op =>
                    {
                        close(op.Status);
                    });
                }));
    }

    private void OpenWorldInfoWindow(ControlBase cause)
    {
        if (worldInfoWindow != null || infoCancellation != null)
            return;

        worldInfoWindow = new Window(menu)
        {
            Title = Language.Info,
            DeleteOnClose = true,
            StartPosition = StartPosition.CenterCanvas,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MinimumSize = new Size(width: 500, height: 700)
        };

        ScrollControl scroll = new(worldInfoWindow)
        {
            AutoHideBars = true,

            CanScrollH = false,
            CanScrollV = true
        };

        VerticalLayout layout = new(scroll)
        {
            Padding = Padding.Five,
            Margin = Margin.Ten
        };

        cause.Disable();
        cause.Redraw();

        Label status = new(layout)
        {
            Text = Texts.FormatOperation(Language.Load, Status.Running),
            TextColor = Colors.Secondary,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        worldInfoWindow.Focus();

        infoCancellation = new CancellationTokenSource();

        worldProvider.GetWorldProperties(world).OnCompletion(op =>
            {
                status.Text = Texts.FormatOperation(Language.Load, op.Status);
                status.TextColor = op.IsOk ? Colors.Secondary : Colors.Error;

#pragma warning disable S2952 // Must be disposed because it is overwritten.
                infoCancellation?.Dispose();
                infoCancellation = null;
#pragma warning disable S2952

                if (op.Result == null)
                    return;

                layout.RemoveChild(status, dispose: true);

                PropertyBasedListControl properties = new(layout, op.Result, context);
                Control.Used(properties);
            },
            infoCancellation.Token);

        worldInfoWindow.Closed += (_, _) =>
        {
#pragma warning disable S2952 // Must be disposed because it is overwritten.
            infoCancellation?.Cancel();
            infoCancellation?.Dispose();
            infoCancellation = null;
#pragma warning disable S2952

            cause.Enable();
            cause.Redraw();

            worldInfoWindow = null;
        };
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();

        worldInfoWindow?.Close();

        infoCancellation?.Cancel();
        infoCancellation?.Dispose();
        infoCancellation = null;
    }
}
