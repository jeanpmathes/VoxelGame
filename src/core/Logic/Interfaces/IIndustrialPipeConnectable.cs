// <copyright file="IIndustrialPipeConnectable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Logic.Interfaces;

#pragma warning disable S4023 // No reflection, only casts are used and methods might be added in the future.

/// <summary>
///     Allows a block to connect to industrial pipes.
/// </summary>
public interface IIndustrialPipeConnectable : IPipeConnectable;
