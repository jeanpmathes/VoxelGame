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
using System.Linq;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Generation.Default;

#pragma warning disable S4017

public partial class Map
{
    private static (List<List<short>>, Dictionary<short, double>) FillWithPieces(Data data, int seed)
    {
        FastNoiseLite noise = new(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        noise.SetFrequency(frequency: 0.05f);

        short currentPiece = 0;
        Dictionary<double, short> valueToPiece = new();

        Dictionary<short, HashSet<short>> adjacencyHashed = new();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            double value = noise.GetNoise(x, y);
            ref Cell current = ref data.GetCell(x, y);

            if (!valueToPiece.ContainsKey(value)) valueToPiece[value] = currentPiece++;

            current.continent = valueToPiece[value];

            UpdateAdjacencies(data, adjacencyHashed, ref current, (x, y));
        }

        return (Algorithms.BuildAdjacencyList(adjacencyHashed), Algorithms.InvertDictionary(valueToPiece));
    }

    private static void UpdateAdjacencies(Data data, IDictionary<short, HashSet<short>> adjacencyHashed, ref Cell current, (int, int) position)
    {
        void AddAdjacency(short a, short b)
        {
            if (!adjacencyHashed.ContainsKey(a))
                adjacencyHashed[a] = new HashSet<short>();

            if (!adjacencyHashed.ContainsKey(b))
                adjacencyHashed[b] = new HashSet<short>();

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

    private static void BuildContinents(Data data, List<List<short>> adjacency, IDictionary<short, double> pieceToValue)
    {
        UnionFind merge = new((short) adjacency.Count);

        DoContinentBuying(adjacency, pieceToValue, merge);
        DoContinentConsuming(adjacency, merge);
        DoContinentMerging(adjacency, merge);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            current.continent = merge.Find(current.continent);
        }
    }

    /// <summary>
    ///     Merge single pieces together.
    /// </summary>
    private static void DoContinentMerging(List<List<short>> adjacency, UnionFind merge)
    {
        for (short continent = 0; continent < adjacency.Count; continent++)
        {
            if (adjacency[continent].Count == 0) continue;
            if (merge.GetSize(continent) > 1) continue;

            foreach (short neighbor in adjacency[continent])
            {
                if (merge.GetSize(neighbor) != 1) continue;

                merge.Union(continent, neighbor);

                break;
            }
        }
    }

    /// <summary>
    ///     Merge pieces into continents that completely surround them.
    /// </summary>
    private static void DoContinentConsuming(List<List<short>> adjacency, UnionFind merge)
    {
        for (short continent = 0; continent < adjacency.Count; continent++)
        {
            if (adjacency[continent].Count == 0) continue;

            short anyNeighbor = merge.Find(adjacency[continent].First());

            if (adjacency[continent].All(neighbor => anyNeighbor == merge.Find(neighbor))) merge.Union(continent, anyNeighbor);
        }
    }

    /// <summary>
    ///     Let continents buy neighbors with their budget.
    /// </summary>
    private static void DoContinentBuying(List<List<short>> adjacency, IDictionary<short, double> pieceToValue, UnionFind merge)
    {
        const double continentMerging = 0.525;

        int GetBudget(short continent, double factor)
        {
            double value = Math.Abs(pieceToValue[continent]);

            return (int) Math.Floor(Math.Pow(x: 2, value / factor) - 0.9);
        }

        for (short continent = 0; continent < adjacency.Count; continent++)
        {
            int budget = GetBudget(continent, continentMerging);

            foreach (short adjacent in adjacency[continent])
            {
                if (budget <= 0) continue;

                merge.Union(continent, adjacent);
                budget--;
            }
        }
    }

    private static void GenerateContinents(Data data, int seed)
    {
        (List<List<short>> adjacency, Dictionary<short, double> pieceToValue) = FillWithPieces(data, seed);
        BuildContinents(data, adjacency, pieceToValue);
    }

    [Conditional("DEBUG")]
    private static void EmitContinentView(Data data, string path)
    {
        using Bitmap view = new(Width, Width);

        Color water = Color.FromArgb(red: 0x8A, green: 0xB4, blue: 0xF8);
        Color border = Color.FromArgb(red: 0x8C, green: 0x8F, blue: 0x93);
        Color land = Color.FromArgb(red: 0xA8, green: 0xDA, blue: 0xB5);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);

            if (x != 0 && current.continent != data.GetCell(x - 1, y).continent)
            {
                view.SetPixel(x, y, border);

                continue;
            }

            if (y != 0 && current.continent != data.GetCell(x, y - 1).continent)
            {
                view.SetPixel(x, y, border);

                continue;
            }

            view.SetPixel(x, y, water);
        }

        view.Save(Path.Combine(path, "continent_view.png"));
    }
}
