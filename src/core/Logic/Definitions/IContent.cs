// <copyright file="IContent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Definitions;

/// <summary>
///     Game content that is part of the world, such as blocks.
/// </summary>
public interface IContent : IResource
{
    /// <summary>
    ///     An unlocalized string that identifies this content.
    /// </summary>
    public String NamedID { get; }
}
