// <copyright file="LogID.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Logging;

namespace VoxelGame.Graphics;

/// <summary>
///     Defines the logging event IDs for this project.
/// </summary>
internal static class LogID
{
    internal const UInt16 D3D12 = Events.GraphicsID;

    internal const UInt16 Client = Events.Increment + D3D12;
}
