// <copyright file="IEntity.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Interface for entities that can be serialized.
///     An entity uses versioned serialization.
///     Implementors should be classes and not structs.
/// </summary>
public interface IEntity
{
    /// <summary>
    ///     Get the current version of the entity.
    /// </summary>
    public static abstract UInt32 CurrentVersion { get; }

    /// <summary>
    ///     Serialize the entity.
    /// </summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="header">The header of the entity.</param>
    public void Serialize(Serializer serializer, Header header);

    /// <summary>
    ///     Header of an entity.
    /// </summary>
    /// <param name="Version">The version of the entity.</param>
    public record struct Header(UInt32 Version);
}
