// <copyright file="IContent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Contents;

/// <summary>
///     Game content that is part of the world, such as blocks.
/// </summary>
public interface IContent : IResource
{
    /// <summary>
    ///     The identifier of this content.
    /// </summary>
    public CID ID { get; }
}
