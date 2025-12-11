// <copyright file="IValue.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Interface for values that can be serialized.
///     Values use simple serialization without versioning.
///     As no versioning is used, the contents should not change.
///     Implementors should provide a parameterless constructor.
/// </summary>
public interface IValue
{
    /// <summary>
    ///     Serialize the value.
    /// </summary>
    /// <param name="serializer">The serializer to use.</param>
    void Serialize(Serializer serializer);
}
