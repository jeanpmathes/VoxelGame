// <copyright file="CycleTracker.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.App;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Profiling;
using VoxelGame.UI.Providers;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Tracks the update and render cycles to calculate FPS and UPS.
/// </summary>
public sealed partial class CycleTracker : ApplicationComponent, IPerformanceProvider
{
    private const Int32 DeltaBufferCapacity = 50;
    private readonly CircularTimeBuffer logicDeltaBuffer = new(DeltaBufferCapacity);

    private readonly CircularTimeBuffer renderDeltaBuffer = new(DeltaBufferCapacity);

    [Constructible]
    private CycleTracker(Core.App.Application application) : base(application) {}

    /// <summary>
    ///     Get the FPS of the screen, which are the frames per second.
    /// </summary>
    public Double FPS => 1.0 / renderDeltaBuffer.Average;

    /// <summary>
    ///     Get the UPS of the screen, which are the updates per second.
    /// </summary>
    public Double UPS => 1.0 / logicDeltaBuffer.Average;

    /// <inheritdoc />
    public override void OnRenderUpdate(Double delta, Timer? timer)
    {
        renderDeltaBuffer.Write(delta);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double delta, Timer? timer)
    {
        logicDeltaBuffer.Write(delta);
    }
}
