﻿// <copyright file="Limit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility to manage a limit for a value. Use a context to store the current budget.
/// </summary>
public class Limit
{
    private readonly Int32 id;
    private readonly Object source;

    /// <summary>
    ///     Creates a new limit.
    /// </summary>
    /// <param name="source">The source / issuer of the limit.</param>
    /// <param name="id">The id of the limit.</param>
    public Limit(Object source, Int32 id)
    {
        this.source = source;
        this.id = id;
    }

    /// <summary>
    ///     Get the id of the limit.
    /// </summary>
    /// <param name="requester">The object requesting the id, must be the same as the source.</param>
    /// <returns>The id of the limit.</returns>
    public Int32 GetID(Object requester)
    {
        Debug.Assert(ReferenceEquals(source, requester));

        return id;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return $"Limit({id}, from: {source})";
    }
}
