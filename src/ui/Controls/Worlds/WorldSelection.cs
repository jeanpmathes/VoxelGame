// <copyright file="WorldSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;
using Colors = VoxelGame.UI.Utilities.Colors;

namespace VoxelGame.UI.Controls.Worlds;

/// <summary>
///     The menu displaying worlds, allowing to select and create worlds.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
internal class WorldSelection : StandardMenu
{
    private readonly IWorldProvider worldProvider;

    private readonly List<Button> buttonBar = new();

    private Search search = null!;
    private WorldList worlds = null!;

    private Boolean isFirstOpen = true;

    private CancellationTokenSource? refreshCancellation;

    private Window? worldCreationWindow;

    internal WorldSelection(ControlBase parent, IWorldProvider worldProvider, Context context) : base(
        parent,
        context)
    {
        this.worldProvider = worldProvider;
        CreateContent();
    }

    protected override void CreateMenu(ControlBase menu)
    {
        Button back = new(menu)
        {
            Text = Language.Back
        };

        back.Released += (_, _) =>
        {
            worldCreationWindow?.Close();
            Cancel(this, EventArgs.Empty);
        };
    }

    protected override void CreateDisplay(ControlBase display)
    {
        DockLayout layout = new(display)
        {
            Padding = Padding.Five,
            Margin = Margin.Ten
        };

        GroupBox box = new(layout)
        {
            Text = Language.Worlds,
            Dock = Dock.Fill
        };

        DockLayout content = new(box);

        Empty space = new(content)
        {
            Dock = Dock.Top,
            Padding = Padding.Five
        };

        Control.Used(space);

        search = new Search(content, Context)
        {
            Dock = Dock.Top
        };

        Separator separator = new(content)
        {
            Dock = Dock.Top
        };

        Control.Used(separator);

        ScrollControl scroll = new(content)
        {
            AutoHideBars = true,
            CanScrollH = false,
            CanScrollV = true,
            Dock = Dock.Fill
        };

        worlds = new WorldList(scroll, worldProvider, Context, this);

        search.FilterChanged += (_, _) =>
        {
            if (refreshCancellation == null)
                worlds.BuildList(search.Filter);
        };

        GroupBox options = new(layout)
        {
            Text = Language.Options,
            Dock = Dock.Bottom
        };

        GridLayout bar = new(options);
        bar.SetColumnWidths(0.5f, 0.25f, 0.25f);

        Button createNewWorldButton = new(bar)
        {
            Text = Language.CreateNewWorld
        };

        buttonBar.Add(createNewWorldButton);
        createNewWorldButton.Released += (_, _) => OpenWorldCreationWindow();

        Button refreshButton = new(bar)
        {
            Text = Language.Refresh
        };

        buttonBar.Add(refreshButton);
        refreshButton.Released += (_, _) => Refresh();

        Button openDirectoryButton = new(bar)
        {
            Text = Language.OpenDirectory
        };

        buttonBar.Add(openDirectoryButton);
        openDirectoryButton.Released += (_, _) => OS.Start(worldProvider.WorldsDirectory);
    }

    protected override void OnOpen()
    {
        if (!isFirstOpen) return;

        isFirstOpen = false;
        Refresh();
    }

    /// <summary>
    ///     Update the entries in the world list.
    /// </summary>
    internal void UpdateList()
    {
        if (refreshCancellation == null)
            worlds.BuildList(search.Filter);
    }

    private void Refresh()
    {
        if (refreshCancellation != null)
            return;

        refreshCancellation = new CancellationTokenSource();

        worlds.BuildText(Texts.FormatOperation(Language.SearchingWorlds, Status.Running));
        SetButtonBarEnabled(enabled: false);

        worldProvider.Refresh().OnCompletion(op =>
            {
                SetButtonBarEnabled(enabled: true);

#pragma warning disable S2952 // Must be disposed because it is overwritten.
                refreshCancellation?.Dispose();
                refreshCancellation = null;
#pragma warning disable S2952

                if (op.IsOk) UpdateList();
                else worlds.BuildText(Texts.FormatOperation(Language.SearchingWorlds, op.Status), isError: true);
            },
            refreshCancellation.Token);
    }

    private void SetButtonBarEnabled(Boolean enabled)
    {
        foreach (Button button in buttonBar)
        {
            if (enabled) button.Enable();
            else button.Disable();

            button.Redraw();
        }
    }

    private void OpenWorldCreationWindow()
    {
        if (worldCreationWindow != null) return;

        worldCreationWindow = new Window(this)
        {
            Title = Language.CreateNewWorld,
            DeleteOnClose = true,
            StartPosition = StartPosition.CenterCanvas,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Resizing = Resizing.Both
        };

        SetButtonBarEnabled(enabled: false);

        worldCreationWindow.Closed += (_, _) =>
        {
            worldCreationWindow = null;

            SetButtonBarEnabled(enabled: true);
        };

        VerticalLayout layout = new(worldCreationWindow)
        {
            Padding = Padding.Five,
            Margin = Margin.Ten
        };

        Label info = new(layout)
        {
            Text = Language.EnterWorldName,
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = Padding.Five
        };

        Control.Used(info);

        TextBox name = new(layout)
        {
            Text = "Hello World",
            Padding = Padding.Five
        };

        Button create = new(layout)
        {
            Text = Language.Create,
            Padding = Padding.Five
        };

        name.TextChanged += (_, _) => ValidateInput(out _);
        create.Released += (_, _) => CreateWorld();

        void ValidateInput(out Boolean isValid)
        {
            String input = name.Text;
            isValid = worldProvider.IsWorldNameValid(input);

            name.TextColor = isValid ? Colors.Primary : Colors.Error;

            create.IsDisabled = !isValid;
            create.UpdateColors();
        }

        void CreateWorld()
        {
            ValidateInput(out Boolean isValid);

            if (isValid) worldProvider.BeginCreatingWorld(name.Text);
        }
    }

    internal event EventHandler Cancel = delegate {};

    public override void Dispose()
    {
        base.Dispose();

        refreshCancellation?.Cancel();
        refreshCancellation?.Dispose();
        refreshCancellation = null;
    }
}
