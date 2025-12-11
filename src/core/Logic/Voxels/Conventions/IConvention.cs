// <copyright file="IConvention.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A convention defines a group of content that is used in the game.
/// </summary>
public interface IConvention : IContent
{
    /// <summary>
    ///     The content that is part of this convention instance.
    /// </summary>
    IEnumerable<IContent> Content { get; }

    /// <inheritdoc />
    ResourceType IResource.Type => ResourceTypes.Convention;
}
