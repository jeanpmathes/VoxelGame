﻿// <copyright file="WorldActions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Worlds;

/// <summary>
///     Actions that are available for each world.
/// </summary>
public class WorldActions : ControlBase
{
    private readonly IWorldProvider.IWorldInfo world;
    private readonly IWorldProvider worldProvider;

    private readonly Context context;
    private readonly ControlBase menu;

    private Window? worldInfoWindow;
    private CancellationTokenSource? infoCancellation;

    /// <summary>
    ///     Creates a new instance of the <see cref="WorldActions" /> class.
    /// </summary>
    /// <param name="parent">The parent control to add this to.</param>
    /// <param name="world">World for which to offer actions.</param>
    /// <param name="worldProvider">The provider of world operations.</param>
    /// <param name="remove">An action that will remove the parent row from the table.</param>
    /// <param name="context">The context in which this control is created.</param>
    /// <param name="menu">The higher level world selection menu that this element is part of.</param>
    internal WorldActions(ControlBase parent, IWorldProvider.IWorldInfo world, IWorldProvider worldProvider, Action remove, Context context, WorldSelection menu) : base(parent)
    {
        this.world = world;
        this.worldProvider = worldProvider;

        this.context = context;
        this.menu = menu;

        HorizontalAlignment = HorizontalAlignment.Right;
        VerticalAlignment = VerticalAlignment.Bottom;

        HorizontalLayout buttons = new(this);

        Button info = context.CreateIconButton(buttons, context.Resources.InfoIcon, Language.Info);
        info.Released += (_, _) => OpenWorldInfoWindow(info);

        Button duplicate = context.CreateIconButton(buttons, context.Resources.DuplicateIcon, Language.Duplicate);

        duplicate.Released += (_, _) => Modals.OpenNameModal(menu,
            new NameBox.Parameters(Language.Duplicate, world.Name),
            new NameBox.Actions(
                duplicateName =>
                {
                    Operation op = worldProvider.DuplicateWorld(world, duplicateName);

                    op.OnCompletion(_ =>
                    {
                        menu.UpdateList();
                    });

                    return op;
                },
                worldProvider.IsWorldNameValid));

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
                    remove();

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
