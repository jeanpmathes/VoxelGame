// <copyright file="InGameDisplay.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls;

/// <summary>
///     A display that is shown while playing the game, as a form of HUD.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal sealed class InGameDisplay : ControlBase
{
    private readonly PropertyBasedTreeControl debugViewContent;

    private readonly ControlBase debugViewRoot;
    private readonly Label performance;
    private readonly Label playerSelection;

    private Boolean debugMode;

    internal InGameDisplay(ControlBase parent) : base(parent)
    {
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

        debugViewRoot = new Border(debugViewContainer)
        {
            MinimumSize = new Size(width: 400, height: 500),
            BorderType = BorderType.TreeControl
        };

        debugViewRoot.Hide();

        debugViewContent = new PropertyBasedTreeControl(debugViewRoot, new Message("", ""))
        {
            Margin = Margin.Five
        };
    }

    internal void SetUpdateRate(Double fps, Double ups)
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

        debugViewContent.Update(playerDataProvider.DebugData);
    }

    internal void ToggleDebugDataView()
    {
        debugMode = !debugMode;
        debugViewRoot.IsHidden = !debugMode;
    }
}
