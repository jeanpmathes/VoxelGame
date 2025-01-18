// <copyright file="PlacementSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     The selector used to decide which block or fluid to player is placing.
/// </summary>
internal class PlacementSelection
{
    private readonly Func<Block?> getTargetBlock;
    private readonly Input input;

    /// <summary>
    ///     Creates a new placement selection.
    /// </summary>
    /// <param name="input">The player input.</param>
    /// <param name="getTargetBlock">A function to get the targeted block.</param>
    internal PlacementSelection(Input input, Func<Block?> getTargetBlock)
    {
        this.input = input;
        this.getTargetBlock = getTargetBlock;

        // The initially selected block and fluid have to be set as block 0 and fluid 0 are not valid in this context.
        ActiveBlock = Blocks.Instance.Grass;
        ActiveFluid = Fluids.Instance.FreshWater;
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

    /// <summary>
    ///     Perform the selection based on current input.
    /// </summary>
    /// <returns>Whether the selection changed.</returns>
    internal Boolean DoBlockFluidSelection()
    {
        var updateData = false;

        updateData |= SelectMode();
        updateData |= SelectFromList();
        updateData |= SelectTargeted();

        return updateData;
    }

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

        ActiveBlock = getTargetBlock() ?? ActiveBlock;
        IsBlockMode = true;

        return true;
    }
}
