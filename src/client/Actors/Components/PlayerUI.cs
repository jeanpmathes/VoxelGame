// <copyright file="PlayerUI.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Controls the user interface for the player, such as HUD and other UI elements.
/// </summary>
public partial class PlayerUI : ActorComponent
{
    private readonly Button debugViewButton;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlacementSelection? placement;

    private readonly Player player;
    private readonly InGameUserInterface ui;

    [Constructible]
    private PlayerUI(Player player, InGameUserInterface ui) : base(player)
    {
        this.player = player;
        this.ui = ui;

        placement = player.GetComponent<PlacementSelection>();

        if (placement != null) placement.SelectionChanged += UpdatePlayerData;

        debugViewButton = player.Input.Keybinds.GetPushButton(player.Input.Keybinds.DebugView);
    }

    /// <inheritdoc />
    public override void OnActivate()
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        ui.SetActive(active: true);
        ui.UpdatePlayerData();
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        ui.SetActive(active: false);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (player.Input.CanHandleMetaInput && debugViewButton.IsDown)
            ui.ToggleDebugDataView();

        ui.UpdatePlayerDebugData();
    }

    private void UpdatePlayerData(Object? sender, EventArgs e)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        ui.UpdatePlayerData();
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        base.Dispose(disposing);

        disposed = true;

        if (!disposing)
            return;

        if (placement != null) placement.SelectionChanged -= UpdatePlayerData;
    }

    #endregion DISPOSABLE
}
