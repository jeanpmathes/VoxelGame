// <copyright file="Map.Terrain.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard;

#pragma warning disable S4017 // Internal interfaces, thus confusion is limited.

public partial class Map
{
    private const Single MinimumLandHeight = +0.01f;
    private const Single AverageWaterHeight = -0.1f;
    private const Double PieceHeightChangeRange = 0.05;
    private const Double MaxDivergentBoundaryLandOffset = -0.025;
    private const Double MaxDivergentBoundaryWaterOffset = +0.05;
    private const Double MaxConvergentBoundaryLandLifting = +0.7;
    private const Double MaxConvergentBoundaryWaterLifting = +0.05;
    private const Double MaxConvergentBoundaryWaterSinking = -0.2;

    private static void GenerateTerrain(Data data, GeneratingNoise noise)
    {
        (List<List<Int16>> adjacency, Dictionary<Int16, Double> pieceToValue) pieces = FillWithPieces(data, noise);

        AddPieceHeights(data, pieces.pieceToValue);

        (List<(Int16, Double)> nodes, Dictionary<Int16, List<Int16>> adjancecy) continents = BuildContinents(data, pieces.adjacency, pieces.pieceToValue);

        GenerateStoneTypes(data, noise);
        SimulateTectonics(data, continents);

        SpreadCoastlineHeightIntoOcean(data);
    }

    private static (List<List<Int16>>, Dictionary<Int16, Double>) FillWithPieces(Data data, GeneratingNoise noise)
    {
        Int16 currentPiece = 0;
        Dictionary<Double, Int16> valueToPiece = new();

        Dictionary<Int16, HashSet<Int16>> adjacencyHashed = new();

        Array2D<Single> noiseGrid = noise.Pieces.GetNoiseGrid((0, 0), Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Double value = noiseGrid[x, y];
            ref Cell current = ref data.GetCell(x, y);

            if (!valueToPiece.TryGetValue(value, out Int16 piece))
            {
                piece = currentPiece++;
                valueToPiece[value] = piece;

                Debug.Assert(piece >= 0); // Detect overflow.
            }

            current.continent = piece;

            UpdateAdjacencies(data, adjacencyHashed, ref current, (x, y));
        }

        return (Algorithms.BuildAdjacencyList(adjacencyHashed), Algorithms.InvertDictionary(valueToPiece));
    }

    private static void AddPieceHeights(Data data, Dictionary<Int16, Double> pieceToValue)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell cell = ref data.GetCell(x, y);

            Double offset = pieceToValue[cell.continent] * PieceHeightChangeRange;
            cell.height += (Single) offset;
        }
    }

    private static (List<(Int16, Double)>, Dictionary<Int16, List<Int16>>) BuildContinents(Data data, List<List<Int16>> adjacency, IDictionary<Int16, Double> pieceToValue)
    {
        UnionFind merge = new((Int16) adjacency.Count);

        DoContinentBuying(adjacency, pieceToValue, merge);
        DoContinentConsuming(adjacency, merge);
        DoContinentMerging(adjacency, merge);

        Boolean[] isLand = DoLandCreation(adjacency, pieceToValue);
        DoLandGapFilling(adjacency, isLand, merge);
        DoLandBorderFlooding(data, isLand);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            Int16 piece = current.continent;

            current.continent = merge.Find(piece);

            if (isLand[piece]) current.height = Math.Abs(current.height) + MinimumLandHeight;
            else current.height += AverageWaterHeight;
        }

        (List<Int16> mergedNodes, Dictionary<Int16, List<Int16>> mergedAdjacency) = Algorithms.MergeAdjacencyList(adjacency, merge.Find);

        return (Algorithms.AppendData(mergedNodes, pieceToValue), mergedAdjacency);
    }

    /// <summary>
    ///     Merge single pieces together.
    /// </summary>
    private static void DoContinentMerging(List<List<Int16>> adjacency, UnionFind merge)
    {
        for (Int16 piece = 0; piece < adjacency.Count; piece++)
        {
            if (adjacency[piece].Count == 0) continue;
            if (merge.GetSize(piece) > 1) continue;

            foreach (Int16 neighbor in adjacency[piece])
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
    private static void DoContinentConsuming(List<List<Int16>> adjacency, UnionFind merge)
    {
        for (Int16 piece = 0; piece < adjacency.Count; piece++)
        {
            if (adjacency[piece].Count == 0) continue;

            Int16 anyNeighbor = merge.Find(adjacency[piece][index: 0]);

            if (adjacency[piece].TrueForAll(neighbor => anyNeighbor == merge.Find(neighbor))) merge.Union(piece, anyNeighbor);
        }
    }

    /// <summary>
    ///     Let continents buy neighbors with their budget.
    /// </summary>
    private static void DoContinentBuying(List<List<Int16>> adjacency, IDictionary<Int16, Double> pieceToValue, UnionFind merge)
    {
        Int32 GetBudget(Int16 piece)
        {
            const Double continentMerging = 0.525;

            Double value = Math.Abs(pieceToValue[piece]);

            return (Int32) Math.Floor(Math.Pow(x: 2, value / continentMerging) - 0.9);
        }

        for (Int16 piece = 0; piece < adjacency.Count; piece++)
        {
            Int32 budget = GetBudget(piece);

            foreach (Int16 adjacent in adjacency[piece])
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
    private static Boolean[] DoLandCreation(List<List<Int16>> adjacency, IDictionary<Int16, Double> pieceToValue)
    {
        var isLand = new Boolean[adjacency.Count];

        Boolean HasBudget(Int16 piece)
        {
            const Double landCreation = 0.9;

            Double value = Math.Abs(pieceToValue[piece]);

            return value > landCreation;
        }

        for (Int16 piece = 0; piece < adjacency.Count; piece++)
        {
            Boolean hasBudget = HasBudget(piece);

            if (!hasBudget) continue;

            isLand[piece] = true;

            foreach (Int16 adjacent in adjacency[piece]) isLand[adjacent] = true;
        }

        return isLand;
    }

    /// <summary>
    ///     Fill single pieces of land/water surrounded by the other type if they are not bordering a different continent.
    /// </summary>
    private static void DoLandGapFilling(List<List<Int16>> adjacency, Boolean[] isLand, UnionFind merge)
    {
        for (Int16 piece = 0; piece < adjacency.Count; piece++)
        {
            if (adjacency[piece].Count == 0) continue;

            Boolean surrounded = adjacency[piece].Aggregate(seed: true, (current, neighbor) => current && (isLand[piece] != isLand[neighbor] || !merge.Connected(piece, neighbor)));

            if (surrounded) isLand[piece] = !isLand[piece];
        }
    }

    /// <summary>
    ///     Flood all land pieces at the world border.
    /// </summary>
    private static void DoLandBorderFlooding(Data data, Boolean[] isLand)
    {
        void FloodCell(Int32 x, Int32 y)
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

    private static void SimulateTectonics(Data data,
        (List<(Int16, Double)> nodes, Dictionary<Int16, List<Int16>> adjancecy) continents)
    {
        Dictionary<Int16, Vector2d> driftDirections = GetDriftDirections(continents.nodes);
        Dictionary<(Int16, Int16), TectonicCollision> collisions = new();

        Array2D<Single> offsets = new(Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);

            void CheckForCollision((Int32 x, Int32 y) neighborPosition)
            {
                Cell neighbor = data.GetCell(neighborPosition.x, neighborPosition.y);

                if (current.continent == neighbor.continent) return;

                var a = new TectonicCell
                {
                    continent = current.continent,
                    position = (x, y),
                    drift = driftDirections[current.continent]
                };

                var b = new TectonicCell
                {
                    continent = neighbor.continent,
                    position = neighborPosition,
                    drift = driftDirections[neighbor.continent]
                };

                HandleTectonicCollision(data, collisions, offsets, a, b);
            }

            if (x != 0) CheckForCollision((x - 1, y));
            if (y != 0) CheckForCollision((x, y - 1));
        }

        AddOffsetsToData(data, offsets);
    }

    private static void UpdateAdjacencies(Data data, IDictionary<Int16, HashSet<Int16>> adjacencyHashed, ref Cell current, (Int32, Int32) position)
    {
        (Int32 x, Int32 y) = position;

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

        void AddAdjacency(Int16 a, Int16 b)
        {
            adjacencyHashed.GetOrAdd(a).Add(b);
            adjacencyHashed.GetOrAdd(b).Add(a);
        }
    }

    private static void GenerateStoneTypes(Data data, GeneratingNoise noise)
    {
        Array2D<Single> noiseGrid = noise.Stone.GetNoiseGrid((0, 0), Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Single value = noiseGrid[x, y];
            value = Math.Abs(value);

            StoneType stoneType = value switch
            {
                < 0.15f => StoneType.Marble,
                < 0.75f => StoneType.Limestone,
                _ => StoneType.Sandstone
            };

            ref Cell current = ref data.GetCell(x, y);
            current.stoneType = stoneType;
        }
    }

    private static void HandleTectonicCollision(
        Data data,
        IDictionary<(Int16, Int16), TectonicCollision> collisions,
        Array2D<Single> offsets,
        TectonicCell a, TectonicCell b)
    {
        if (!collisions.TryGetValue((a.continent, b.continent), out TectonicCollision collision))
        {
            Vector2i relativePosition = b.position - a.position;
            Vector2d relativeDrift = b.drift - a.drift;

            TectonicCollision alternatives = Vector2d.Dot(relativePosition, relativeDrift) > 0 ? TectonicCollision.Divergent : TectonicCollision.Convergent;
            collision = relativeDrift.Length < 0.5 ? TectonicCollision.Transform : alternatives;

            collisions[(a.continent, b.continent)] = collision;
            collisions[(b.continent, a.continent)] = collision;
        }

        switch (collision)
        {
            case TectonicCollision.Transform:
                HandleTransformBoundary(data, a, b);

                break;

            case TectonicCollision.Divergent:
                HandleDivergentBoundary(data, offsets, a, b);

                break;

            case TectonicCollision.Convergent:
                HandleConvergentBoundary(data, offsets, a, b);

                break;

            default:
                throw Exceptions.UnsupportedEnumValue(collision);
        }
    }

    /// <summary>
    ///     Handle a convergent boundary between two tectonic plates.
    ///     A convergent boundary is where two plates move towards each other.
    ///     If both cells are land, they are pushed upwards.
    ///     If one cell is land and the other is not, the water cell is pushed under the land cell.
    /// </summary>
    private static void HandleConvergentBoundary(Data data, Array2D<Single> offsets, TectonicCell a, TectonicCell b)
    {
        Double strength = MathTools.CalculateAngle(a.drift, b.drift) / Math.PI;
        Vector2d direction;
        Vector2i start;

        Cell cellA = data.GetCell(a.position);
        Cell cellB = data.GetCell(b.position);

        if (cellA.IsLand && cellB.IsLand)
        {
            start = a.position;

            direction = b.position - a.position;
            direction += a.drift * 0.25;
            direction += b.drift * 0.25;
        }
        else
        {
            (TectonicCell water, TectonicCell other) = cellA.IsLand ? (b, a) : (a, b);

            start = other.position;

            direction = other.position - water.position;
            direction += water.drift * 0.25;
            direction += other.drift * 0.25;

            ref Cell otherCell = ref data.GetCell(other.position);
            otherCell.conditions |= CellConditions.Vulcanism;
            otherCell.stoneType = StoneType.Granite;

            offsets[water.position] = (Single) (strength * MaxConvergentBoundaryWaterSinking);
        }

        foreach (Vector2i cellPosition in Algorithms.TraverseCells(start, direction.Normalized(), strength * 5.0))
        {
            if (IsOutOfBounds(cellPosition)) continue;

            Double maxLifting = data.GetCell(cellPosition).IsLand ? MaxConvergentBoundaryLandLifting : MaxConvergentBoundaryWaterLifting;
            offsets[cellPosition] = (Single) (strength * maxLifting);
        }
    }

    /// <summary>
    ///     Handle a transform boundary between two tectonic plates.
    ///     A transform boundary is where two plates slide past each other.
    ///     This does not affect the height of the cells, but it does cause seismic activity.
    /// </summary>
    private static void HandleTransformBoundary(Data data, TectonicCell a, TectonicCell b)
    {
        ref Cell cellA = ref data.GetCell(a.position);
        cellA.conditions |= CellConditions.SeismicActivity;

        ref Cell cellB = ref data.GetCell(b.position);
        cellB.conditions |= CellConditions.SeismicActivity;
    }

    /// <summary>
    ///     Handle a divergent boundary between two tectonic plates.
    ///     A divergent boundary is where two plates move away from each other.
    ///     Land cells are pushed downwards, water cells are pushed upwards.
    ///     If both cells are land, a rift is created.
    ///     If both cells are water, a rift and vulcanism is created.
    /// </summary>
    private static void HandleDivergentBoundary(Data data, Array2D<Single> offsets, TectonicCell a, TectonicCell b)
    {
        Double divergence = MathTools.CalculateAngle(a.drift, b.drift) / Math.PI;

        ref Cell cellA = ref data.GetCell(a.position);
        ref Cell cellB = ref data.GetCell(b.position);

        var conditions = CellConditions.None;

        if (cellA.IsLand && cellB.IsLand) conditions = CellConditions.Rift;

        if (!cellA.IsLand && !cellB.IsLand)
        {
            conditions = CellConditions.Rift | CellConditions.Vulcanism;

            cellA.stoneType = StoneType.Granite;
            cellB.stoneType = StoneType.Granite;
        }

        cellA.conditions |= conditions;
        cellB.conditions |= conditions;

        offsets[a.position] = (Single) (divergence * (cellA.IsLand ? MaxDivergentBoundaryLandOffset : MaxDivergentBoundaryWaterOffset));
        offsets[b.position] = (Single) (divergence * (cellB.IsLand ? MaxDivergentBoundaryLandOffset : MaxDivergentBoundaryWaterOffset));
    }

    private static void SpreadCoastlineHeightIntoOcean(Data data)
    {
        Array2D<Single> offsets = new(Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);

            if (!current.IsLand)
                continue;

            Int32 oceanNeighbors = GetNumberOfOceanNeighbors(data, x, y);

            if (oceanNeighbors == 0)
                continue;

            Single availableHeight = current.height * GetHeightSpreadingFactor(current.stoneType);
            Single heightPerNeighbor = availableHeight / oceanNeighbors;

            AddHeightToOceanNeighbors(data, offsets, heightPerNeighbor, x, y);

            offsets[x, y] -= availableHeight;
        }

        AddOffsetsToData(data, offsets);
    }

    private static Int32 GetNumberOfOceanNeighbors(Data data, Int32 x, Int32 y)
    {
        var count = 0;

        for (Int32 dx = -1; dx <= 1; dx++)
        for (Int32 dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0)
                continue;

            Int32 nx = x + dx;
            Int32 ny = y + dy;

            if (!Data.IsInLimits(nx, ny))
                continue;

            if (!data.GetCell(nx, ny).IsLand)
                count++;
        }

        return count;
    }

    private static void AddHeightToOceanNeighbors(Data data, Array2D<Single> offsets, Single height, Int32 x, Int32 y)
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

            if (!data.GetCell(nx, ny).IsLand)
                offsets[nx, ny] += height;
        }
    }

    private static Single GetHeightSpreadingFactor(StoneType stoneType)
    {
        return stoneType switch
        {
            StoneType.Granite => 0.5f,
            StoneType.Limestone => 0.8f,
            StoneType.Marble => 0.6f,
            StoneType.Sandstone => 0.9f,
            _ => 0.0f
        };
    }

    private static void AddOffsetsToData(Data data, Array2D<Single> offsets)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            current.height += offsets[x, y];
        }
    }

    private static Boolean IsOutOfBounds(Vector2i position)
    {
        return position.X is < 0 or >= Width || position.Y is < 0 or >= Width;
    }

    private static Dictionary<Int16, Vector2d> GetDriftDirections(List<(Int16, Double)> continentsNodes)
    {
        Dictionary<Int16, Vector2d> driftDirections = new();

        foreach ((Int16 node, Double value) in continentsNodes)
        {
            Double angle = value * Math.PI;
            driftDirections[node] = MathTools.CreateVectorFromAngle(angle);
        }

        return driftDirections;
    }

    private static async Task EmitTerrainViewAsync(Data data, DirectoryInfo path, CancellationToken token = default)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetTerrainColor(current));
        }

        await view.SaveAsync(path.GetFile("terrain_view.png"), token).InAnyContext();
    }

    private static ColorS GetTerrainColor(Cell current)
    {
        ColorS water = ColorS.Blue;
        ColorS land = ColorS.Green;

        ColorS terrain = current.IsLand ? land : water;
        Double mixStrength = Math.Abs(current.height) - 0.5;
        Boolean darken = mixStrength > 0;

        ColorS mixed = ColorS.Mix(terrain, darken ? ColorS.Black : ColorS.White, Math.Abs(mixStrength));

        return mixed;
    }

    private static ColorS GetStoneTypeColor(Cell current)
    {
        if (current.IsLand)
            return current.stoneType switch
            {
                StoneType.Granite => ColorS.Green,
                StoneType.Limestone => ColorS.Blue,
                StoneType.Marble => ColorS.Red,
                StoneType.Sandstone => ColorS.Yellow,
                _ => ColorS.Black
            };

        return ColorS.White;
    }

    private static async Task EmitStoneViewAsync(Data data, DirectoryInfo path, CancellationToken token = default)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetStoneTypeColor(current));
        }

        await view.SaveAsync(path.GetFile("stone_view.png"), token).InAnyContext();
    }

    private static ColorS GetContinentColor(Int16 continent, Boolean isLand)
    {
        Single hue = continent * 0.618033988749895f % 1.0f;
        Single saturation = isLand ? 0.8f : 0.2f;

        return ColorS.FromHSV(hue, saturation, value: 0.95f);
    }

    private static async Task EmitContinentViewAsync(Data data, DirectoryInfo path, CancellationToken token = default)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetContinentColor(current.continent, current.IsLand));
        }

        await view.SaveAsync(path.GetFile("continent_view.png"), token).InAnyContext();
    }

    private enum TectonicCollision
    {
        Transform,
        Convergent,
        Divergent
    }

    private record struct TectonicCell
    {
        public Int16 continent;
        public Vector2d drift;
        public Vector2i position;
    }
}
