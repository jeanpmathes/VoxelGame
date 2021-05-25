// <copyright file="ITickable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Collections
{
    public interface ITickable
    {
        void Tick(World world);
    }
}