// <copyright file="IIssueSource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     A source for issues during resource loading.
/// </summary>
public interface IIssueSource
{
    /// <summary>
    ///     Optional instance name of the issue source.
    ///     If not provided, just the type name is used.
    /// </summary>
    public String? InstanceName => null;
}
