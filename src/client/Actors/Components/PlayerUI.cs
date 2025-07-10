// <copyright file="PlayerUI.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Actors;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Controls the user interface for the player, such as HUD and other UI elements.
/// </summary>
public class PlayerUI : ActorComponent, IConstructible<Player, InGameUserInterface, PlayerUI>
{
    private readonly Player player;
    private readonly InGameUserInterface ui;
    
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlacementSelection? placement;

    private readonly Button debugViewButton;
    
    private PlayerUI(Player player, InGameUserInterface ui) : base(player) 
    {
        this.player = player;
        this.ui = ui;
        
        placement = player.GetComponent<PlacementSelection>();
        if (placement != null)
        {
            placement.SelectionChanged += UpdatePlayerData;
        }

        debugViewButton = player.Scene.Client.Keybinds.GetPushButton(player.Scene.Client.Keybinds.DebugView);
    }

    /// <inheritdoc />
    public static PlayerUI Construct(Player input1, InGameUserInterface input2)
    {
        return new PlayerUI(input1, input2);
    }

    /// <inheritdoc />
    public override void OnActivate()
    {
        Throw.IfDisposed(disposed);
        
        ui.SetActive(active: true);
        ui.UpdatePlayerData();
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        Throw.IfDisposed(disposed);
        
        ui.SetActive(active: false);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        Throw.IfDisposed(disposed);

        if (player.Scene.CanHandleMetaInput && debugViewButton.IsDown)
            ui.ToggleDebugDataView();
        
        ui.UpdatePlayerDebugData();
    }
    
    private void UpdatePlayerData(Object? sender, EventArgs e)
    {
        Throw.IfDisposed(disposed);

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

        if (placement != null)
        {
            placement.SelectionChanged -= UpdatePlayerData;
        }
    }

    #endregion DISPOSABLE
}
