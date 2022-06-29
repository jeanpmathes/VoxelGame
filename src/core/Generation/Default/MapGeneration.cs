// <copyright file="MapGeneration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace VoxelGame.Core.Generation.Default;

public partial class Map
{
    private static void GenerateContinents(Data data, int seed)
    {
        FastNoiseLite noise = new(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);

        var currentPiece = 0;
        Dictionary<double, int> valueToPiece = new();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            double value = noise.GetNoise(x, y);
            ref Cell current = ref data.GetCell(x, y);

            if (!valueToPiece.ContainsKey(value)) valueToPiece[value] = currentPiece++;

            current.continent = valueToPiece[value];
        }
    }

    [Conditional("DEBUG")]
    private static void EmitContinentView(Data data, string path)
    {
        Random random = new(Seed: 0);
        using Bitmap view = new(Width, Width);

        Dictionary<int, Color> colors = new();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);

            if (!colors.ContainsKey(current.continent))
                colors[current.continent] = Color.FromArgb(
                    random.Next(minValue: 0, maxValue: 255),
                    random.Next(minValue: 0, maxValue: 255),
                    random.Next(minValue: 0, maxValue: 255)
                );

            view.SetPixel(x, y, colors[current.continent]);
        }

        view.Save(Path.Combine(path, "continent_view.png"));
    }
}
