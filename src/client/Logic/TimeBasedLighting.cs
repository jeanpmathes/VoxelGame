// <copyright file="TimeBasedLighting.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Sets the light based on the time of day.
/// </summary>
public partial class TimeBasedLighting : WorldComponent
{
    private readonly Double rotation = MathHelper.DegreesToRadians(degrees: 40);

    [Constructible]
    private TimeBasedLighting(Core.Logic.World subject) : base(subject)
    {
        World = subject.Cast();

        World.Space.Light.Direction = GetLightDirection(World.TimeOfDay);
    }

    private World World { get; }

    /// <inheritdoc />
    public override void OnLogicUpdateInActiveState(Double deltaTime, Timer? updateTimer)
    {
        Double timeOfDay = Subject.TimeOfDay;
        Boolean isDay = timeOfDay < 0.5;
        Visuals.Graphics.Instance.SetTimeOfDay(timeOfDay);

        Double maxIntensity = isDay ? 1.0 : 0.6;
        Double intensity = Math.Clamp((Math.Cos(timeOfDay * 2.0 * Math.Tau) * -1.0 * 0.5 + 0.5) * Math.PI, min: 0.0, max: 1.0) * maxIntensity;
        World.Space.Light.Intensity = (Single) intensity;

        World.Space.Light.Color = isDay
            ? ColorS.FromRGB(red: 0.97f, green: 0.97f, blue: 0.94f)
            : ColorS.FromRGB(red: 0.95f, green: 0.96f, blue: 0.98f);

        World.Space.Light.Direction = GetLightDirection(timeOfDay);
    }

    private Vector3d GetLightDirection(Double timeOfDay)
    {
        // Both sun and moon shine down from above.
        if (timeOfDay >= 0.5) timeOfDay -= 0.5;

        Double angle = timeOfDay * Math.Tau;
        Double y = -Math.Sin(angle);
        Double h = Math.Cos(angle);

        return Vector3d.Normalize(new Vector3d(h * Math.Cos(rotation), y, h * Math.Sin(rotation)));
    }
}
