﻿// <copyright file="UpdateCounter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A counter for update cycles.
/// </summary>
public class UpdateCounter
{
    /// <summary>
    ///     The number of the current update cycle. It is incremented every time a new cycle begins.
    /// </summary>
    public UInt64 Current { get; private set; }

    /// <summary>
    ///     Increment the update counter.
    /// </summary>
    public void Increment()
    {
        Debug.Assert(Current < UInt64.MaxValue);

        Current++;
    }

    /// <summary>
    ///     Reset the update counter.
    /// </summary>
    public void Reset()
    {
        Current = 0;
    }
}
