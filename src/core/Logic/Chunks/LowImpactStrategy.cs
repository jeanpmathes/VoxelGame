// <copyright file="LowImpactStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     A strategy that tries to reduce the impact of chunk updates on game performance.
///     It takes longer to update all chunks, but the game should be more responsive.
/// </summary>
public class LowImpactStrategy(ChunkStateUpdateList list) : ChunkStateUpdateStrategy(list)
{
    /// <summary>
    ///     Always update at least some low priority chunks to prevent starvation.
    /// </summary>
    private const Int32 MinLowPriorityUpdates = 50;

    /// <summary>
    ///     Try to not run more than this number of updates per frame.
    /// </summary>
    private const Int32 MaxDesiredUpdates = 250;

    /// <summary>
    ///     How much budget is recovered per frame.
    /// </summary>
    private const Int32 BudgetRecoveryRate = 10;

    private readonly List<Chunk?> local = [];

    private Int32 budget = MaxDesiredUpdates;

    private void RecoverBudget()
    {
        budget = Math.Clamp(budget + BudgetRecoveryRate, MinLowPriorityUpdates, MaxDesiredUpdates);
    }

    /// <inheritdoc />
    public override Int32 Update(Bag<Chunk> chunks)
    {
        local.Clear();
        chunks.CopyDirectlyTo(local);

        var updatedChunkCount = 0;
        var highPriorityChunkCount = 0;

        foreach (Chunk? chunk in local)
        {
            if (chunk == null)
                continue;

            if (chunk.IsRequestedToSimulate) highPriorityChunkCount += 1;
            else if (budget > 0) budget -= 1;
            else continue;

            Update(chunk);
            updatedChunkCount += 1;
        }

        budget -= highPriorityChunkCount;
        RecoverBudget();

        return updatedChunkCount;
    }
}
