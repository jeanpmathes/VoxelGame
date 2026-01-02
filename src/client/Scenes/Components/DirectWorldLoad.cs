// <copyright file="DirectWorldLoad.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Linq;
using Microsoft.Extensions.Logging;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Loads a world directly in the first logic update of the scene.
/// </summary>
public partial class DirectWorldLoad : SceneComponent
{
    private readonly Int32 index;
    private readonly IWorldProvider worldProvider;

    private Boolean isLoadingPossible;

    [Constructible]
    private DirectWorldLoad(Scene subject, IWorldProvider worldProvider, Int32 index) : base(subject)
    {
        this.worldProvider = worldProvider;
        this.index = index;
    }

    /// <inheritdoc />
    public override void OnLoad()
    {
        isLoadingPossible = !Subject.HasComponent<ResourceLoadingReportHook>();

        if (!isLoadingPossible)
            LogResourceLoadingFailurePreventsDirectWorldLoading(logger);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        if (!isLoadingPossible)
            return;

        Result result = worldProvider.Refresh().Wait();

        result.Switch(() =>
            {
                IWorldProvider.IWorldInfo? info = worldProvider.Worlds.ElementAtOrDefault(index);

                if (info != null)
                {
                    LogLoadingWorldDirectly(logger, index);

                    worldProvider.LoadAndActivateWorld(info);
                }
                else
                {
                    LogCouldNotDirectlyLoadWorld(logger, index);
                }
            },
            exception =>
            {
                LogCouldNotRefreshWorldsToDirectlyLoadWorld(logger, exception, index);
            });

        RemoveSelf();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<DirectWorldLoad>();

    [LoggerMessage(EventId = LogID.StartScene + 0, Level = LogLevel.Warning, Message = "Resource loading failure prevents direct world loading, going to start menu")]
    private static partial void LogResourceLoadingFailurePreventsDirectWorldLoading(ILogger logger);

    [LoggerMessage(EventId = LogID.StartScene + 1, Level = LogLevel.Error, Message = "Could not refresh worlds to directly-load world at index {Index}, going to main menu")]
    private static partial void LogCouldNotRefreshWorldsToDirectlyLoadWorld(ILogger logger, Exception exception, Int32 index);

    [LoggerMessage(EventId = LogID.StartScene + 2, Level = LogLevel.Information, Message = "Loading world at index {Index} directly")]
    private static partial void LogLoadingWorldDirectly(ILogger logger, Int32 index);

    [LoggerMessage(EventId = LogID.StartScene + 3, Level = LogLevel.Error, Message = "Could not directly-load world at index {Index}, going to main menu")]
    private static partial void LogCouldNotDirectlyLoadWorld(ILogger logger, Int32 index);

    #endregion LOGGING
}
