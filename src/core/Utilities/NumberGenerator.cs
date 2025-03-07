﻿// <copyright file="NumberGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A utility class for generating random numbers.
/// </summary>
public static class NumberGenerator
{
    /// <summary>
    ///     The random number generator.
    /// </summary>
    public static Random Random => Random.Shared;
}
