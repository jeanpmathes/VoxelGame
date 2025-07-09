// <copyright file="PlacementSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Decides which block or fluid the player is placing.
/// </summary>
public class PlacementSelection : ActorComponent, IConstructible<Player, PlacementSelection>
{
    private readonly Player player;
    
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;
    
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlayerInput input;
    
    private PlacementSelection(Player player) : base(player) 
    {
        this.player = player;
        
        targeting = player.GetRequiredComponent<Targeting>();
        input = player.GetRequiredComponent<PlayerInput, Player>();
        
        // The initially selected block and fluid have to be set as block 0 and fluid 0 are not valid in this context.
        ActiveBlock = Blocks.Instance.Grass;
        ActiveFluid = Fluids.Instance.FreshWater;
    }
    
    /// <inheritdoc />
    public static PlacementSelection Construct(Player input)
    {
        return new PlacementSelection(input);
    }
    
    /// <summary>
    ///     Get the name of the current selection.
    /// </summary>
    internal String SelectionName => IsBlockMode ? ActiveBlock.Name : ActiveFluid.Name;

    /// <summary>
    ///     Get the name of the current mode.
    /// </summary>
    internal String ModeName => IsBlockMode ? Language.Block : Language.Fluid;

    /// <summary>
    ///     The current active block.
    /// </summary>
    internal Block ActiveBlock { get; private set; }

    /// <summary>
    ///     The current active fluid.
    /// </summary>
    internal Fluid ActiveFluid { get; private set; }

    /// <summary>
    ///     Whether the current mode is block mode.
    /// </summary>
    internal Boolean IsBlockMode { get; private set; } = true;

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        if (!player.Scene.CanHandleGameInput)
            return;
        
        var changed = false;

        changed |= SelectMode();
        changed |= SelectFromList();
        changed |= SelectTargeted();

        if (changed)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invoked if the selection has changed in any way, e.g. the block or fluid was changed or the mode was switched.
    /// </summary>
    public event EventHandler? SelectionChanged;
    
    private Boolean SelectMode()
    {
        if (!input.ShouldChangePlacementMode) return false;

        IsBlockMode = !IsBlockMode;

        return true;
    }

    private Boolean SelectFromList()
    {
        Int32 change = input.GetSelectionChange();

        if (change == 0) return false;

        if (IsBlockMode)
        {
            Int64 nextBlockId = ActiveBlock.ID + change;
            nextBlockId = MathTools.ClampRotating(nextBlockId, min: 1, Blocks.Instance.Count);
            ActiveBlock = Blocks.Instance.TranslateID((UInt32) nextBlockId);
        }
        else
        {
            Int64 nextFluidId = ActiveFluid.ID + change;
            nextFluidId = MathTools.ClampRotating(nextFluidId, min: 1, Fluids.Instance.Count);
            ActiveFluid = Fluids.Instance.TranslateID((UInt32) nextFluidId);
        }

        return true;
    }

    private Boolean SelectTargeted()
    {
        if (!input.ShouldSelectTargeted) return false;

        ActiveBlock = targeting.Block?.Block ?? ActiveBlock;
        IsBlockMode = true;

        return true;
    }
}
