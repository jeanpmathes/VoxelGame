// <copyright file="Map.Climate.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Worlds.Standard;

public partial class Map
{
    private const Int32 ClimateSimulationSteps = 500;

    private static void GenerateHumidity(Data data)
    {
        Array2D<HumidityData> current = CreateInitialHumidityData();
        Array2D<HumidityData> next = CreateInitialHumidityData();

        for (var step = 0; step < ClimateSimulationSteps; step++)
        {
            SimulateClimate(data, current, next);
            (current, next) = (next, current);
        }

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell cell = ref data.GetCell(x, y);
            cell.humidity = current[x, y].humidity;
        }
    }

    private static Array2D<HumidityData> CreateInitialHumidityData()
    {
        const Single initialHumidity = 0.15f;

        Array2D<HumidityData> initial = new(Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
            initial[x, y].humidity = initialHumidity;

        return initial;
    }

    private static void SimulateClimate(Data data, Array2D<HumidityData> current, Array2D<HumidityData> next)
    {
        Parallel.For(fromInclusive: 0,
            Width * Width,
            index =>
            {
                Vector2i position = new(index % Width, index / Width);
                next[position] = SimulateCellClimate(data, current, position);
            });
    }

    private static IEnumerable<(Vector2i position, Boolean isInWind)> GetNeighbors(Vector2i position)
    {
        if (position.X > 0) yield return ((position.X - 1, position.Y), false);
        if (position.X < Width - 1) yield return ((position.X + 1, position.Y), true);
        if (position.Y > 0) yield return ((position.X, position.Y - 1), false);
        if (position.Y < Width - 1) yield return ((position.X, position.Y + 1), false);
    }

    /// <summary>
    ///     Simulates one step for a single cell, taking into account the cell data and the previous step data of this and
    ///     neighboring cells.
    ///     The system is inspired by the following tutorial by Jasper Flick:
    ///     https://catlikecoding.com/unity/tutorials/hex-map/part-25/
    /// </summary>
    private static HumidityData SimulateCellClimate(in Data data, in Array2D<HumidityData> state, Vector2i position)
    {
        const Single landEvaporationRate = 0.10f;
        const Single seaEvaporationRate = 0.75f;
        const Single precipitationRate = 0.50f;
        const Single runoffRate = 0.03f;
        const Single windStrength = 3.0f;

        Cell cell = data.GetCell(position);
        HumidityData current = state[position];

        HumidityData next;

        next.clouds = current.clouds;
        next.humidity = current.humidity;
        next.dispersal = 0.0f;
        next.runoff = 0.0f;

        if (cell.IsLand)
        {
            Single evaporation = next.humidity * landEvaporationRate;
            next.humidity -= evaporation;
            next.clouds += evaporation;
        }
        else
        {
            next.humidity = 1.0f;
            next.clouds += seaEvaporationRate;
        }

        Single precipitation = next.clouds * precipitationRate;
        next.clouds -= precipitation;
        next.humidity += precipitation;

        Single cloudMaximum = MathTools.Clamp01(Math.Min(1.0f - cell.height * 1.5f, cell.temperature + 0.1f));

        if (next.clouds > cloudMaximum)
        {
            next.humidity += next.clouds - cloudMaximum;
            next.clouds = cloudMaximum;
        }

        next.dispersal = next.clouds * (1.0f / (3.0f + windStrength));
        next.runoff = next.humidity * runoffRate * (1.0f / 4.0f);
        next.clouds = 0.0f;

        foreach ((Vector2i neighborPosition, Boolean isInWind) in GetNeighbors(position))
        {
            Cell neighborCell = data.GetCell(neighborPosition);
            HumidityData neighborData = state[neighborPosition];

            next.clouds += isInWind ? neighborData.dispersal * windStrength : neighborData.dispersal;

            if (!cell.IsLand || !neighborCell.IsLand)
                continue;

            if (neighborCell.height > cell.height) next.humidity += neighborData.runoff;
            else if (neighborCell.height < cell.height) next.humidity -= next.runoff;
        }

        next.humidity = Math.Min(next.humidity, cell.temperature);

        return next;
    }

    private static ColorS GetHumidityColor(Cell current)
    {
        ColorS precipitation = ColorS.FromRGB(current.humidity, current.humidity, current.humidity);

        return current.IsLand ? precipitation : ColorS.Aqua;
    }

    private static async Task EmitHumidityViewAsync(Data data, DirectoryInfo path, CancellationToken token = default)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetHumidityColor(current));
        }

        await view.SaveAsync(path.GetFile("precipitation_view.png"), token).InAnyContext();
    }

    private record struct HumidityData
    {
        public Single clouds;
        public Single dispersal;
        public Single humidity;
        public Single runoff;
    }
}
