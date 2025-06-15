// <copyright file="State.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Refers to the state of a block.
/// </summary>
/// <param name="Set">The set of all states of the block.</param>
/// <param name="Index">The index of the state in the set.</param>
public readonly record struct State(StateSet Set, UInt64 Index)
{
    /// <summary>
    /// Get the value index of the given attribute in this state.
    /// </summary>
    /// <param name="attribute">The attribute to get the index for.</param>
    /// <returns>The attribute value index for the given state.</returns>
    public Int32 GetValueIndex(IAttribute attribute)
    {
        return Set.GetValueIndex(attribute, Index);
    }
}