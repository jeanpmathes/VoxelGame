// <copyright file="UpdateInGamePerformanceData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Profiling;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Updates the in-game performance data on the <see cref="InGameUserInterface" />.
/// </summary>
public partial class UpdateInGamePerformanceData : SceneComponent
{
    private readonly InGameUserInterface ui;

    [Constructible]
    private UpdateInGamePerformanceData(Scene subject, InGameUserInterface ui) : base(subject)
    {
        this.ui = ui;
    }

    /// <inheritdoc />
    public override void OnRenderUpdate(Double deltaTime, Timer? timer)
    {
        ui.UpdatePerformanceData();
    }
}
