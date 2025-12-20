// <copyright file="ProfilerConfiguration.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Profiling;

/// <summary>
///     How the profiler should be configured.
/// </summary>
public enum ProfilerConfiguration
{
    /// <summary>
    ///     No profiling is done.
    ///     Has negligible performance impact.
    /// </summary>
    Disabled,

    /// <summary>
    ///     Basic profiling is done.
    ///     Can have a noticeable performance impact, but the application remains usable.
    /// </summary>
    Basic,

    /// <summary>
    ///     Full profiling, including lifetime tracking.
    ///     The results are summarized in a report file when the application is closed.
    ///     Will have a significant performance impact.
    /// </summary>
    Full
}
