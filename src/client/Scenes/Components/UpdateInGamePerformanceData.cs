// <copyright file="UpdateInGamePerformanceData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
/// Updates the in-game performance data on the <see cref="InGameUserInterface"/>.
/// </summary>
public class UpdateInGamePerformanceData : SceneComponent, IConstructible<Scene, InGameUserInterface, UpdateInGamePerformanceData>
{
    private readonly InGameUserInterface ui;

    private UpdateInGamePerformanceData(Scene subject, InGameUserInterface ui) : base(subject)
    {
        this.ui = ui;
    }

    /// <inheritdoc />
    public static UpdateInGamePerformanceData Construct(Scene input1, InGameUserInterface input2)
    {
        return new UpdateInGamePerformanceData(input1, input2);
    }

    /// <inheritdoc />
    public override void OnRenderUpdate(Double deltaTime, Timer? timer)
    {
        ui.UpdatePerformanceData();
    }
}
