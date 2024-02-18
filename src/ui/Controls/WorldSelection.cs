// <copyright file="WorldSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls;

/// <summary>
///     The menu displaying worlds, allowing to select and create worlds.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class WorldSelection : StandardMenu
{
    private readonly IWorldProvider worldProvider;

    private readonly List<Button> buttonBar = new();
    private ControlBase? worldList;

    private bool isFirstOpen = true;

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

        GroupBox scrollBox = new(layout)
        {
            Text = Language.Worlds,
            Dock = Dock.Fill
        };

        ScrollControl scroll = new(scrollBox)
        {
            AutoHideBars = true,
            CanScrollH = false,
            CanScrollV = true,
            Dock = Dock.Fill
        };

        worldList = new VerticalLayout(scroll);
        BuildWorldList();

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
    }

    protected override void OnOpen()
    {
        if (!isFirstOpen) return;

        isFirstOpen = false;
        Refresh();
    }

    private void Refresh()
    {
        if (refreshCancellation != null)
            return;

        refreshCancellation = new CancellationTokenSource();

        BuildTextDisplay(Texts.FormatOperation(Language.SearchingWorlds, Status.Running));
        SetButtonBarEnabled(enabled: false);

        worldProvider.Refresh().OnCompletion(op =>
            {
                SetButtonBarEnabled(enabled: true);

                if (op.IsOk) BuildWorldList();
                else BuildTextDisplay(Texts.FormatOperation(Language.SearchingWorlds, op.Status), isError: true);

#pragma warning disable S2952 // Must be disposed because it is overwritten.
                refreshCancellation?.Dispose();
                refreshCancellation = null;
#pragma warning disable S2952
            },
            refreshCancellation.Token);
    }

    private void SetButtonBarEnabled(bool enabled)
    {
        foreach (Button button in buttonBar)
        {
            if (enabled) button.Enable();
            else button.Disable();

            button.Redraw();
        }
    }

    private void BuildTextDisplay(string text, bool isError = false)
    {
        Debug.Assert(worldList != null);

        worldList.DeleteAllChildren();

        Label label = new(worldList)
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextColor = isError ? Colors.Error : Colors.Secondary
        };

        Control.Used(label);
    }

    private void BuildWorldList()
    {
        Debug.Assert(worldList != null);

        worldList.DeleteAllChildren();

        foreach (WorldData data in worldProvider.Worlds.OrderByDescending(entry => worldProvider.GetDateTimeOfLastLoad(entry) ?? DateTime.MaxValue))
        {
            WorldElement element = new(worldList, data, worldProvider, Context, this);

            Control.Used(element);
        }

        if (!worldProvider.Worlds.Any()) BuildTextDisplay(Language.NoWorldsFound);
    }

    private void OpenWorldCreationWindow()
    {
        if (worldCreationWindow != null) return;

        worldCreationWindow = new Window(this)
        {
            Title = Language.CreateNewWorld,
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

        void ValidateInput(out bool isValid)
        {
            string input = name.Text;
            isValid = worldProvider.IsWorldNameValid(input);

            name.TextColor = isValid ? Colors.Primary : Colors.Error;

            create.IsDisabled = !isValid;
            create.UpdateColors();
        }

        void CreateWorld()
        {
            ValidateInput(out bool isValid);

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
