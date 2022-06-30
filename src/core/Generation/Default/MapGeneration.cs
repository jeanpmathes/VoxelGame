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

#pragma warning disable S4017

public partial class Map
{
    private static List<List<int>> FillWithPieces(Data data, int seed)
    {
        FastNoiseLite noise = new(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);

        var currentPiece = 0;
        Dictionary<double, int> valueToPiece = new();

        Dictionary<int, HashSet<int>> adjacencyHashed = new();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            double value = noise.GetNoise(x, y);
            ref Cell current = ref data.GetCell(x, y);

            if (!valueToPiece.ContainsKey(value)) valueToPiece[value] = currentPiece++;

            current.continent = valueToPiece[value];

            UpdateAdjacencies(data, adjacencyHashed, ref current, (x, y));
        }

        return BuildAdjacencyList(adjacencyHashed);
    }

    private static void UpdateAdjacencies(Data data, IDictionary<int, HashSet<int>> adjacencyHashed, ref Cell current, (int, int) position)
    {
        void AddAdjacency(int a, int b)
        {
            if (!adjacencyHashed.ContainsKey(a))
                adjacencyHashed[a] = new HashSet<int>();

            if (!adjacencyHashed.ContainsKey(b))
                adjacencyHashed[b] = new HashSet<int>();

            adjacencyHashed[a].Add(b);
            adjacencyHashed[b].Add(a);
        }

        (int x, int y) = position;

        if (x != 0)
        {
            ref Cell left = ref data.GetCell(x - 1, y);

            if (left.continent != current.continent) AddAdjacency(left.continent, current.continent);
        }

        if (y != 0)
        {
            ref Cell top = ref data.GetCell(x, y - 1);

            if (top.continent != current.continent) AddAdjacency(top.continent, current.continent);
        }
    }

    private static List<List<int>> BuildAdjacencyList(Dictionary<int, HashSet<int>> adjacencyHashed)
    {
        List<List<int>> adjacency = new();

        for (var continent = 0; continent < adjacencyHashed.Count; continent++)
        {
            List<int> neighbors = new(adjacencyHashed[continent]);
            adjacency.Add(neighbors);
        }

        return adjacency;
    }

    private static void GenerateContinents(Data data, int seed)
    {
        List<List<int>> adjacency = FillWithPieces(data, seed);
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
