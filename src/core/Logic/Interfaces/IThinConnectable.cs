// <copyright file="IThinConnectable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Logic.Interfaces;

/// <summary>
///     Marks a block as able to be connected to by thin blocks from different directions. This interface does not allow
///     connections at the top or bottom side.
///     The connection surface might be transparent.
/// </summary>
public interface IThinConnectable : IConnectable {}
