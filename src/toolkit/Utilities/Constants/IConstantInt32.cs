// <copyright file="IConstantInt32.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Toolkit.Utilities.Constants;

/// <summary>
///     Defines a constant integer value.
/// </summary>
public interface IConstantInt32
{
    /// <summary>
    ///     The constant integer value.
    /// </summary>
    public static abstract Int32 Value { get; }
}
