// <copyright file="MetaVersion.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     The version of the serialization system.
/// </summary>
public enum MetaVersion : UInt32
{
    /// <summary>
    ///     The initial version of the serialization system.
    /// </summary>
    Initial = 1,

    /// <summary>
    ///     The current version of the serialization system.
    /// </summary>
    Current = Initial
}
