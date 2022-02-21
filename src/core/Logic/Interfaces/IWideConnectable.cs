// <copyright file="IFenceConnectable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic.Interfaces
{
    /// <summary>
    /// Marks a block as able to be connected to by wide blocks from different directions. This interface does not allow connections at the top or bottom side.
    /// The connection surface has to be opaque.
    /// </summary>
    public interface IWideConnectable : IConnectable {}
}
