// <copyright file = "IDefault.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Toolkit.Utilities.Constants;

/// <summary>
///     Gives a type an easily accessible default value.
/// </summary>
public interface IDefault<out T> where T : IDefault<T>
{
    /// <summary>
    ///     The default value of the type.
    /// </summary>
    static abstract T Default { get; }
}
