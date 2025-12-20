// <copyright file="CycleTracker.cs" company="VoxelGame">
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
