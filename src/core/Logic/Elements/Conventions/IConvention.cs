// <copyright file="IConvention.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic.Definitions;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
///     A convention defines a group of content that is used in the game.
/// </summary>
public interface IConvention : IContent // todo: think about how to handle conventions in the manual
{
    /// <inheritdoc />
    ResourceType IResource.Type => ResourceTypes.Convention;
    
    /// <summary>
    ///     The content that is part of this convention instance.
    /// </summary>
    public IEnumerable<IContent> Content { get; }
}
