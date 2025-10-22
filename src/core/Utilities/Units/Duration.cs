// <copyright file="Duration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

public readonly partial struct Duration
{
    /// <summary>
    ///     Get the duration, in milliseconds.
    /// </summary>
    public Double Milliseconds
    {
        get => Seconds * 1000;
        init => Seconds = value / 1000;
    }
}
