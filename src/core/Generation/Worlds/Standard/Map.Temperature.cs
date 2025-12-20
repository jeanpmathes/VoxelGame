// <copyright file="Map.Temperature.cs" company="VoxelGame">
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
using VoxelGame.Core.Collections;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Worlds.Standard;

public partial class Map
{
    private static readonly Polyline temperatureFunction = new()
    {
        Points =
        {
            (0.0, 0.0),
            (0.2, 0.4),
            (0.8, 0.6),
            (1.0, 1.0)
        }
    };

    private static void GenerateTemperature(Data data)
    {
        Vector2 center = new(Width / 2.0f, Width / 2.0f);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Single distance = (center - (x, y)).Length;
            Single angle = MathF.Atan2(center.Y - y, center.X - x) + MathF.PI;

            Single temperature = GetTemperature(distance, angle);

            ref Cell current = ref data.GetCell(x, y);
            current.temperature = temperature;
        }

        return;

        Single GetTemperature(Single distance, Single angle)
        {
            Single scale = distance / Width * 2.0f;

            Single offset = MathF.Sin(angle * 20.0f) + MathTools.Cube(MathF.Cos(angle * 15.0f));
            Single amplitude = scale * 0.02f;

            return (Single) temperatureFunction.Evaluate(scale + offset * amplitude);
        }
    }

    private static ColorS GetTemperatureColor(Cell current)
    {
        ColorS tempered = ColorS.FromRGB(2.0f * current.temperature, 2.0f * (1 - current.temperature), blue: 0.0f);
        ColorS other = current.IsLand ? ColorS.Black : tempered;

        return ColorS.Mix(tempered, other);
    }

    private static async Task EmitTemperatureViewAsync(Data data, DirectoryInfo path, CancellationToken token = default)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetTemperatureColor(current));
        }

        await view.SaveAsync(path.GetFile("temperature_view.png"), token).InAnyContext();
    }
}
