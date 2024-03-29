﻿// <copyright file="IFenceConnectable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Logic.Interfaces;

#pragma warning disable S4023 // No reflection, only casts are used and methods might be added in the future.

/// <summary>
///     Marks a block as able to be connected to by wide blocks from different directions. This interface does not allow
///     connections at the top or bottom side.
///     The connection surface has to be opaque.
/// </summary>
public interface IWideConnectable : IConnectable;
