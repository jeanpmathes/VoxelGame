// <copyright file="InGameDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>


using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     A display that is shown while playing the game, as a form of HUD.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal class InGameDisplay : ControlBase
{
    private readonly Context context;

    private readonly ControlBase debugView;

    private readonly Label performance;
    private readonly Label playerSelection;

    private bool debugMode;

    internal InGameDisplay(ControlBase parent, Context context) : base(parent)
    {
        this.context = context;

        Dock = Dock.Fill;

        DockLayout top = new(this)
        {
            Dock = Dock.Top,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = Margin.Ten
        };

        playerSelection = new Label(top)
        {
            Text = "Block: _____",
            Dock = Dock.Left
        };

        VerticalLayout right = new(top)
        {
            Dock = Dock.Right,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        performance = new Label(right)
        {
            Text = "FPS/UPS: 000/000",
            Alignment = Alignment.Right,
            AutoSizeToContents = false
        };

        VerticalLayout debugViewContainer = new(right)
        {
            Dock = Dock.Right,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        Empty pad = new(debugViewContainer)
        {
            Padding = Padding.Five
        };

        Control.Used(pad);

        debugView = new Border(debugViewContainer)
        {
            BorderType = BorderType.ListBox
        };

        debugView.Hide();
    }

    internal void SetUpdateRate(double fps, double ups)
    {
        performance.Text = $"FPS/UPS: {fps:000}/{ups:000}";
    }

    internal void SetPlayerData(IPlayerDataProvider playerDataProvider)
    {
        playerSelection.Text = $"{playerDataProvider.Mode}: {playerDataProvider.Selection}";
    }

    internal void SetPlayerDebugData(IPlayerDataProvider playerDataProvider)
    {
        if (!debugMode) return;

        debugView.DeleteAllChildren();

        PropertyBasedListControl property = new(debugView, playerDataProvider.DebugData, context)
        {
            Margin = Margin.Five
        };

        Control.Used(property);
    }

    internal void ToggleDebugDataView()
    {
        debugMode = !debugMode;
        debugView.IsHidden = !debugMode;
    }
}
