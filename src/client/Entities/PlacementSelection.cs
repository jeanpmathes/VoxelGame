﻿// <copyright file="PlacementSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Entities;

/// <summary>
///     The selector used to decide which block or fluid to player is placing.
/// </summary>
internal class PlacementSelection
{
    private readonly Func<Block?> getTargetBlock;
    private readonly PlayerInput input;

    /// <summary>
    ///     Creates a new placement selection.
    /// </summary>
    /// <param name="input">The player input.</param>
    /// <param name="getTargetBlock">A function to get the targeted block.</param>
    internal PlacementSelection(PlayerInput input, Func<Block?> getTargetBlock)
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
    internal string SelectionName => IsBlockMode ? ActiveBlock.Name : ActiveFluid.Name;

    /// <summary>
    ///     Get the name of the current mode.
    /// </summary>
    internal string ModeName => IsBlockMode ? Language.Block : Language.Fluid;

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
    internal bool IsBlockMode { get; private set; } = true;

    /// <summary>
    ///     Perform the selection based on current input.
    /// </summary>
    /// <returns>Whether the selection changed.</returns>
    internal bool DoBlockFluidSelection()
    {
        var updateData = false;

        updateData |= SelectMode();
        updateData |= SelectFromList();
        updateData |= SelectTargeted();

        return updateData;
    }

    private bool SelectMode()
    {
        if (!input.ShouldChangePlacementMode) return false;

        IsBlockMode = !IsBlockMode;

        return true;
    }

    private bool SelectFromList()
    {
        int change = input.GetSelectionChange();

        if (change == 0) return false;

        if (IsBlockMode)
        {
            long nextBlockId = ActiveBlock.ID + change;
            nextBlockId = VMath.ClampRotating(nextBlockId, min: 1, Blocks.Instance.Count);
            ActiveBlock = Blocks.Instance.TranslateID((uint) nextBlockId);
        }
        else
        {
            long nextFluidId = ActiveFluid.ID + change;
            nextFluidId = VMath.ClampRotating(nextFluidId, min: 1, Fluids.Instance.Count);
            ActiveFluid = Fluids.Instance.TranslateID((uint) nextFluidId);
        }

        return true;
    }

    private bool SelectTargeted()
    {
        if (!input.ShouldSelectTargeted) return false;

        ActiveBlock = getTargetBlock() ?? ActiveBlock;
        IsBlockMode = true;

        return true;
    }
}


