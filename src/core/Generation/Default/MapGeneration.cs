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
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;

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

    private static (List<(short, double)>, Dictionary<short, List<short>>) BuildContinents(Data data, List<List<short>> adjacency, IDictionary<short, double> pieceToValue)
    {
        UnionFind merge = new((short) adjacency.Count);

        DoContinentBuying(adjacency, pieceToValue, merge);
        DoContinentConsuming(adjacency, merge);
        DoContinentMerging(adjacency, merge);

        bool[] isLand = DoLandCreation(adjacency, pieceToValue);
        DoLandGapFilling(adjacency, isLand, merge);
        DoLandBorderFlooding(data, isLand);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            short piece = current.continent;

            current.continent = merge.Find(piece);
            current.isLand = isLand[piece];
        }

        (List<short> mergedNodes, Dictionary<short, List<short>> mergedAdjacency) = Algorithms.MergeAdjacencyList(adjacency, merge.Find);

        return (Algorithms.AppendData(mergedNodes, pieceToValue), mergedAdjacency);
    }

    /// <summary>
    ///     Merge single pieces together.
    /// </summary>
    private static void DoContinentMerging(List<List<short>> adjacency, UnionFind merge)
    {
        for (short piece = 0; piece < adjacency.Count; piece++)
        {
            if (adjacency[piece].Count == 0) continue;
            if (merge.GetSize(piece) > 1) continue;

            foreach (short neighbor in adjacency[piece])
            {
                if (merge.GetSize(neighbor) != 1) continue;

                merge.Union(piece, neighbor);

                break;
            }
        }
    }

    /// <summary>
    ///     Merge pieces into continents that completely surround them.
    /// </summary>
    private static void DoContinentConsuming(List<List<short>> adjacency, UnionFind merge)
    {
        for (short piece = 0; piece < adjacency.Count; piece++)
        {
            if (adjacency[piece].Count == 0) continue;

            short anyNeighbor = merge.Find(adjacency[piece].First());

            if (adjacency[piece].All(neighbor => anyNeighbor == merge.Find(neighbor))) merge.Union(piece, anyNeighbor);
        }
    }

    /// <summary>
    ///     Let continents buy neighbors with their budget.
    /// </summary>
    private static void DoContinentBuying(List<List<short>> adjacency, IDictionary<short, double> pieceToValue, UnionFind merge)
    {
        int GetBudget(short piece)
        {
            const double continentMerging = 0.525;

            double value = Math.Abs(pieceToValue[piece]);

            return (int) Math.Floor(Math.Pow(x: 2, value / continentMerging) - 0.9);
        }

        for (short piece = 0; piece < adjacency.Count; piece++)
        {
            int budget = GetBudget(piece);

            foreach (short adjacent in adjacency[piece])
            {
                if (budget <= 0) continue;

                merge.Union(piece, adjacent);
                budget--;
            }
        }
    }

    /// <summary>
    ///     Give all pieces a land-budget that they can use to make themselves and other pieces land.
    /// </summary>
    private static bool[] DoLandCreation(List<List<short>> adjacency, IDictionary<short, double> pieceToValue)
    {
        var isLand = new bool[adjacency.Count];

        bool HasBudget(short piece)
        {
            const double landCreation = 0.9;

            double value = Math.Abs(pieceToValue[piece]);

            return value > landCreation;
        }

        for (short piece = 0; piece < adjacency.Count; piece++)
        {
            bool hasBudget = HasBudget(piece);

            if (!hasBudget) continue;

            isLand[piece] = true;

            foreach (short adjacent in adjacency[piece]) isLand[adjacent] = true;
        }

        return isLand;
    }

    /// <summary>
    ///     Fill single pieces of land/water surrounded by the other type if they are not bordering a different continent.
    /// </summary>
    private static void DoLandGapFilling(List<List<short>> adjacency, bool[] isLand, UnionFind merge)
    {
        for (short piece = 0; piece < adjacency.Count; piece++)
        {
            if (adjacency[piece].Count == 0) continue;

            bool surrounded = adjacency[piece].Aggregate(seed: true, (current, neighbor) => current && (isLand[piece] != isLand[neighbor] || !merge.Connected(piece, neighbor)));

            if (surrounded) isLand[piece] = !isLand[piece];
        }
    }

    /// <summary>
    ///     Flood all land pieces at the world border.
    /// </summary>
    private static void DoLandBorderFlooding(Data data, IList<bool> isLand)
    {
        void FloodCell(int x, int y)
        {
            ref readonly Cell cell = ref data.GetCell(x, y);
            isLand[cell.continent] = false;
        }

        for (var i = 0; i < Width; i++)
        {
            FloodCell(i, y: 0);
            FloodCell(i, Width - 1);

            FloodCell(x: 0, i);
            FloodCell(Width - 1, i);
        }
    }

    private static void GenerateContinents(Data data, int seed)
    {
        (List<List<short>> adjacency, Dictionary<short, double> pieceToValue) pieces = FillWithPieces(data, seed);

        AddPieceHeights(data, pieces.pieceToValue);

        (List<(short, double)> nodes, Dictionary<short, List<short>> adjancecy) continents = BuildContinents(data, pieces.adjacency, pieces.pieceToValue);

        SimulateTectonics(data, continents);
    }

    private static void AddPieceHeights(Data data, IDictionary<short, double> pieceToValue)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell cell = ref data.GetCell(x, y);

            double offset = pieceToValue[cell.continent] * 0.1;
            cell.height += (float) offset;
        }
    }

    private static void SimulateTectonics(Data data,
        (List<(short, double)> nodes, Dictionary<short, List<short>> adjancecy) continents)
    {
        Dictionary<short, Vector2d> driftDirections = GetDriftDirections(continents.nodes);
        Dictionary<(short, short), TectonicCollision> collisions = new();

        var offsetsC = new float[CellCount];
        var offsetsD = new float[CellCount];

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);

            short? handledContinent = null;

            void CheckForCollision((int x, int y) neighborPosition)
            {
                Cell neighbor = data.GetCell(neighborPosition.x, neighborPosition.y);

                if (current.continent == neighbor.continent || neighbor.continent == handledContinent) return;

                var a = new TectonicCell
                {
                    cell = current,
                    position = (x, y),
                    drift = driftDirections[current.continent]
                };

                var b = new TectonicCell
                {
                    cell = neighbor,
                    position = neighborPosition,
                    drift = driftDirections[neighbor.continent]
                };

                HandleTectonicCollision(data, collisions, offsetsC, offsetsD, a, b);

                handledContinent = neighbor.continent;
            }

            if (x != 0) CheckForCollision((x - 1, y));

            if (y != 0) CheckForCollision((x, y - 1));
        }

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            current.height += Data.Get(offsetsC, x, y) + Data.Get(offsetsD, x, y);
        }
    }

    private static bool IsOutOfBounds(Vector2i position)
    {
        return position.X is < 0 or >= Width || position.Y is < 0 or >= Width;
    }

    private static void HandleTectonicCollision(Data data, IDictionary<(short, short), TectonicCollision> collisions, float[] offsetsC, float[] offsetsD,
        TectonicCell a, TectonicCell b)
    {
        void HandleTransformBoundary()
        {
            // Intentionally empty.
        }

        void HandleDivergentBoundary()
        {
            const double landFactor = -0.2;
            const double waterFactor = +0.2;

            double divergence = VMath.CalculateAngle(a.drift, b.drift) / Math.PI;

            Data.Get(offsetsD, a.position) = (float) (divergence * (data.GetCell(a.position).isLand ? landFactor : waterFactor));
            Data.Get(offsetsD, b.position) = (float) (divergence * (data.GetCell(b.position).isLand ? landFactor : waterFactor));
        }

        void HandleConvergentBoundary()
        {
            const double liftFactor = +0.8;
            const double sinkFactor = -0.8;

            double strength = VMath.CalculateAngle(a.drift, b.drift) / Math.PI;
            Vector2d direction;
            Vector2i start;

            if (a.cell.isLand && b.cell.isLand)
            {
                start = a.position;

                direction = b.position - a.position;
                direction += a.drift * 0.25;
                direction += b.drift * 0.25;
            }
            else
            {
                (TectonicCell water, TectonicCell other) = a.cell.isLand ? (b, a) : (a, b);

                start = other.position;

                direction = other.position - water.position;
                direction += water.drift * 0.25;
                direction += other.drift * 0.25;

                Data.Get(offsetsC, water.position) = (float) (strength * sinkFactor);
            }

            foreach (Vector2i cellPosition in Algorithms.TraverseCells(start, direction.Normalized(), strength * 5.0))
            {
                if (IsOutOfBounds(cellPosition)) continue;

                Data.Get(offsetsC, cellPosition) = (float) (strength * liftFactor);
            }
        }

        TectonicCollision collision;

        if (collisions.ContainsKey((a.cell.continent, b.cell.continent)))
        {
            collision = collisions[(a.cell.continent, b.cell.continent)];
        }
        else
        {
            Vector2i relativePosition = b.position - a.position;
            Vector2d relativeDrift = b.drift - a.drift;

            if (relativeDrift.Length < 0.5) collision = TectonicCollision.Transform;
            else collision = Vector2d.Dot(relativePosition, relativeDrift) > 0 ? TectonicCollision.Divergent : TectonicCollision.Convergent;

            collisions[(a.cell.continent, b.cell.continent)] = collision;
            collisions[(b.cell.continent, a.cell.continent)] = collision;
        }

        switch (collision)
        {
            case TectonicCollision.Transform:
                HandleTransformBoundary();

                break;

            case TectonicCollision.Divergent:
                HandleDivergentBoundary();

                break;

            case TectonicCollision.Convergent:
                HandleConvergentBoundary();

                break;

            default:
                throw new InvalidOperationException();
        }
    }

    private static Dictionary<short, Vector2d> GetDriftDirections(List<(short, double)> continentsNodes)
    {
        Dictionary<short, Vector2d> driftDirections = new();

        foreach ((short node, double value) in continentsNodes)
        {
            double angle = value * Math.PI;

            Vector2d drift;

            drift.X = Math.Cos(angle);
            drift.Y = Math.Sin(angle);

            driftDirections[node] = drift;
        }

        return driftDirections;
    }

    [Conditional("DEBUG")]
    private static void EmitContinentView(Data data, string path)
    {
        using Bitmap view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetMapColor(current));
        }

        view.Save(Path.Combine(path, "continent_view.png"));
    }

    private static Color GetMapColor(Cell current)
    {
        Color water = Color.Blue;
        Color land = Color.Green;

        Color terrain = current.isLand ? land : water;
        bool darken = current.height * (current.isLand ? 1 : -1) > 0;

        Color mixed = Colors.Mix(terrain, darken ? Color.Black : Color.White, Math.Abs(current.height) / 2);

        return mixed;
    }

    private enum TectonicCollision
    {
        Transform,
        Convergent,
        Divergent
    }

    private record struct TectonicCell
    {
        public Cell cell;
        public Vector2i position;
        public Vector2d drift;
    }
}
