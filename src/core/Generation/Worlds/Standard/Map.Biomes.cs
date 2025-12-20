// <copyright file="Map.Biomes.cs" company="VoxelGame">
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Standard.Biomes;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard;

public partial class Map
{
    private const Single MaxTolerableHeightDifference = 0.04f;
    private const Single MountainHeightThreshold = 0.2f;

    private static void GenerateAdditionalSpecialConditions(Data data)
    {
        DetectMountains(data);
        DetectCoastlines(data);
        DetectCliffs(data);
    }

    private static void DetectMountains(Data data)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);

            if (!current.IsLand)
                continue;

            if (current.height < MountainHeightThreshold)
                continue;

            current.conditions |= CellConditions.Mountainous;
            SpreadMountainsToNeighbors(data, current.height, x, y);
        }
    }

    private static void SpreadMountainsToNeighbors(Data data, Single currentHeight, Int32 x, Int32 y)
    {
        for (Int32 dx = -1; dx <= 1; dx++)
        for (Int32 dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0)
                continue;

            Int32 nx = x + dx;
            Int32 ny = y + dy;

            if (!Data.IsInLimits(nx, ny))
                continue;

            ref Cell neighbor = ref data.GetCell(nx, ny);
            SpreadMountainsToNeighbor(data, currentHeight, nx, ny, ref neighbor);
        }
    }

    private static void SpreadMountainsToNeighbor(Data data, Single currentHeight, Int32 nx, Int32 ny, ref Cell neighbor)
    {
        if (neighbor.conditions.HasFlag(CellConditions.Mountainous))
            return;

        if (!neighbor.IsLand || neighbor.height >= currentHeight)
            return;

        if (Math.Abs(currentHeight - neighbor.height) <= MaxTolerableHeightDifference)
            return;

        neighbor.conditions |= CellConditions.Mountainous;
        SpreadMountainsToNeighbors(data, neighbor.height, nx, ny);
    }

    private static void DetectCliffs(Data data)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);

            if (!current.IsLand || current.conditions.HasFlag(CellConditions.Mountainous))
                continue;

            DetectCliff(data, ref current, x, y);
        }
    }

    private static void DetectCliff(Data data, ref Cell current, Int32 x, Int32 y)
    {
        foreach (Orientation orientation in Orientations.All)
        {
            Vector2i offset = orientation.ToVector3i().Xz;

            Int32 nx = x + offset.X;
            Int32 ny = y + offset.Y;

            if (!Data.IsInLimits(nx, ny))
                continue;

            ref Cell neighbor = ref data.GetCell(nx, ny);

            if (DetectCliff(ref current, ref neighbor))
            {
                current.conditions |= GetCliffCondition(orientation);

                return;
            }
        }
    }

    private static Boolean DetectCliff(ref Cell current, ref Cell neighbor)
    {
        if (current.height > neighbor.height)
            return false;

        return Math.Abs(current.height - neighbor.height) > MaxTolerableHeightDifference;
    }

    private static CellConditions GetCliffCondition(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => CellConditions.CliffNorth,
            Orientation.East => CellConditions.CliffEast,
            Orientation.South => CellConditions.CliffSouth,
            Orientation.West => CellConditions.CliffWest,
            _ => throw Exceptions.UnsupportedEnumValue(orientation)
        };
    }

    private static void DetectCoastlines(Data data)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);

            DetectCoastline(data, ref current, x + 1, y);
            DetectCoastline(data, ref current, x, y + 1);
            DetectCoastline(data, ref current, x + 1, y + 1);
        }
    }

    private static void DetectCoastline(Data data, ref Cell current, Int32 nx, Int32 ny)
    {
        if (!Data.IsInLimits(nx, ny))
            return;

        ref Cell neighbor = ref data.GetCell(nx, ny);

        if (current.IsLand == neighbor.IsLand)
            return;

        current.conditions |= CellConditions.Coastline;
        neighbor.conditions |= CellConditions.Coastline;
    }

    private static async Task EmitBiomeViewAsync(Data data, BiomeDistribution biomes, DirectoryInfo path, CancellationToken token = default)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetBiomeColor(current, biomes));
        }

        await view.SaveAsync(path.GetFile("biome_view.png"), token).InAnyContext();
    }

    private static ColorS GetBiomeColor(in Cell current, BiomeDistribution biomes)
    {
        return current.IsLand ? GetBiome(biomes, current).Definition.Color : ColorS.White;
    }
}
