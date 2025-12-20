// <copyright file="Player.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Actors.Components;
using VoxelGame.Client.Scenes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Physics;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Actors;

/// <summary>
///     The client player, controlled by the user. There can only be one client player.
/// </summary>
public sealed partial class Player : Core.Actors.Player, IPlayerDataProvider
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlacementSelection selector;

    /// <summary>
    ///     Create a client player.
    /// </summary>
    /// <param name="mass">The mass of the player.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    /// <param name="camera">The camera to use for this player.</param>
    /// <param name="ui">The user interface used for the game.</param>
    /// <param name="engine">The graphics engine to use for rendering.</param>
    /// <param name="input">The input control to use for this player.</param>
    public Player(Double mass, BoundingVolume boundingVolume, Camera camera, InGameUserInterface ui, Engine engine, IInputControl input) : base(mass, boundingVolume)
    {
        Camera = camera;
        Input = input;

        AddComponent<PlayerInput, Player>();
        AddComponent<PlayerMovement, Player>(); // Also updates the targeter.
        selector = AddComponent<PlacementSelection, Player>();
        AddComponent<Interaction, Player>();
        AddComponent<OverlayDisplay, Engine, Player>(engine);
        AddComponent<Targeter>();
        AddComponent<PlayerUI, InGameUserInterface, Player>(ui);
        AddComponent<TargetingDisplay, Engine, Player>(engine);
        AddComponent<CrosshairDisplay, Engine, Player>(engine);

        LogCreatedNewPlayer(logger);
    }

    /// <inheritdoc />
    public override Transform Head => Camera.Transform;

    /// <summary>
    ///     Get access to the camera of the player.
    /// </summary>
    internal Camera Camera { get; }

    /// <summary>
    ///     Get the input control used by this player.
    /// </summary>
    public IInputControl Input { get; }

    /// <inheritdoc />
    public Property DebugData => new PlayerDebugProperties(this);

    String IPlayerDataProvider.Selection => selector.SelectionName;

    String IPlayerDataProvider.Mode => selector.ModeName;

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Player>();

    [LoggerMessage(EventId = LogID.Player + 0, Level = LogLevel.Debug, Message = "Created new player")]
    private static partial void LogCreatedNewPlayer(ILogger logger);

    #endregion LOGGING
}
