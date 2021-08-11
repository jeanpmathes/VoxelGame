// <copyright file="IIdentifiable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Collections
{
    public interface IIdentifiable<out T>
    {
        T Id { get; }
    }
}