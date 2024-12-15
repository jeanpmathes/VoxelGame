// <copyright file="LoggingEvents.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Logging;

/// <summary>
///     Defines basic ranges of the event IDs for the logging system.
///     The ranges are aligned with the project structure.
/// </summary>
public static class Events
{
    /// <summary>
    /// The increment between event IDs of different classes.
    /// </summary>
    public const UInt16 Increment = 0x010;

    /// <summary>
    /// The VoxelGame.Core project.
    /// </summary>
    public const UInt16 CoreID = 0x0000;

    /// <summary>
    /// The VoxelGame.Client project.
    /// </summary>
    public const UInt16 ClientID = 0x1000;

    /// <summary>
    /// The VoxelGame.Server project.
    /// </summary>
    public const UInt16 ServerID = 0x2000;

    /// <summary>
    /// The VoxelGame.Graphics project.
    /// </summary>
    public const UInt16 GraphicsID = 0x3000;

    /// <summary>
    /// The VoxelGame.Toolkit project.
    /// </summary>
    public const UInt16 ToolkitID = 0x4000;

    /// <summary>
    /// The VoxelGame.UI project.
    /// </summary>
    public const UInt16 UserInterfaceID = 0x5000;

    /// <summary>
    /// The VoxelGame.Manual project.
    /// </summary>
    public const UInt16 ManualID = 0x6000;

    /// <summary>
    /// Any plugins and extensions.
    /// </summary>
    public const UInt16 PluginID = 0xF000;
}
