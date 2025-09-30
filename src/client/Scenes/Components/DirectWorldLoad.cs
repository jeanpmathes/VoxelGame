// <copyright file="DirectWorldLoad.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Loads a world directly in the first logic update of the scene.
/// </summary>
public partial class DirectWorldLoad : SceneComponent, IConstructible<Scene, (IWorldProvider, Int32), DirectWorldLoad>
{
    private readonly Int32 index;
    private readonly IWorldProvider worldProvider;

    private Boolean isLoadingPossible;

    private DirectWorldLoad(Scene subject, IWorldProvider worldProvider, Int32 index) : base(subject)
    {
        this.worldProvider = worldProvider;
        this.index = index;
    }

    /// <inheritdoc />
    public static DirectWorldLoad Construct(Scene input1, (IWorldProvider, Int32) input2)
    {
        return new DirectWorldLoad(input1, input2.Item1, input2.Item2);
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
        if (!isLoadingPossible) return;

        LoadWorldDirectly();

        isLoadingPossible = false;
    }

    private void LoadWorldDirectly()
    {
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
