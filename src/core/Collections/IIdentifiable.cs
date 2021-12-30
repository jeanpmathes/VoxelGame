// <copyright file="IIdentifiable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Collections
{
    /// <summary>
    ///     Allows identification of an object using an id.
    /// </summary>
    /// <typeparam name="T">The type of the id.</typeparam>
    public interface IIdentifiable<out T>
    {
        /// <summary>
        ///     Gets the id of the object.
        /// </summary>
        T Id { get; }
    }
}