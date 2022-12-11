// <copyright file="MapGeneration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default;

#pragma warning disable S4017

public partial class Map
{
    private const float MinimumLandHeight = +0.05f;
    private const float AverageWaterHeight = -0.4f;

    private const double PieceHeightChangeRange = 0.2;

    private const double MaxDivergentBoundaryLandOffset = -0.025;
    private const double MaxDivergentBoundaryWaterOffset = +0.2;

    private const double MaxConvergentBoundaryLandLifting = +0.7;
    private const double MaxConvergentBoundaryWaterLifting = +0.4;
    private const double MaxConvergentBoundaryWaterSinking = -0.4;

    private static (List<List<short>>, Dictionary<short, double>) FillWithPieces(Data data, GeneratingNoise noise)
    {
        noise.Pieces.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.Pieces.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        noise.Pieces.SetFrequency(frequency: 0.05f);

        short currentPiece = 0;
        Dictionary<double, short> valueToPiece = new();

        Dictionary<short, HashSet<short>> adjacencyHashed = new();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            double value = noise.Pieces.GetNoise(x, y);
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
            adjacencyHashed.GetOrAdd(a).Add(b);
            adjacencyHashed.GetOrAdd(b).Add(a);
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

            if (isLand[piece]) current.height = Math.Abs(current.height) + MinimumLandHeight;
            else current.height += AverageWaterHeight;
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

    private static void GenerateTerrain(Data data, GeneratingNoise noise)
    {
        (List<List<short>> adjacency, Dictionary<short, double> pieceToValue) pieces = FillWithPieces(data, noise);

        AddPieceHeights(data, pieces.pieceToValue);

        (List<(short, double)> nodes, Dictionary<short, List<short>> adjancecy) continents = BuildContinents(data, pieces.adjacency, pieces.pieceToValue);

        GenerateStoneTypes(data, noise);
        SimulateTectonics(data, continents);
    }

    private static void AddPieceHeights(Data data, IDictionary<short, double> pieceToValue)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell cell = ref data.GetCell(x, y);

            double offset = pieceToValue[cell.continent] * PieceHeightChangeRange;
            cell.height += (float) offset;
        }
    }

    private static void SimulateTectonics(Data data,
        (List<(short, double)> nodes, Dictionary<short, List<short>> adjancecy) continents)
    {
        Dictionary<short, Vector2d> driftDirections = GetDriftDirections(continents.nodes);
        Dictionary<(short, short), TectonicCollision> collisions = new();

        var offsets = new float[CellCount];

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);

            void CheckForCollision((int x, int y) neighborPosition)
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

    private static void GenerateStoneTypes(Data data, GeneratingNoise noise)
    {
        noise.Stone.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.Stone.SetFrequency(frequency: 0.08f);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            float value = noise.Stone.GetNoise(x, y);
            value = Math.Abs(value);

            StoneType stoneType = value switch
            {
                < 0.05f => StoneType.Marble,
                < 0.45f => StoneType.Limestone,
                _ => StoneType.Sandstone
            };

            ref Cell current = ref data.GetCell(x, y);
            current.stoneType = stoneType;
        }
    }

    private static void AddOffsetsToData(Data data, float[] offsets)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            current.height += Data.Get(offsets, x, y);
        }
    }

    private static bool IsOutOfBounds(Vector2i position)
    {
        return position.X is < 0 or >= Width || position.Y is < 0 or >= Width;
    }

    private static void HandleTectonicCollision(Data data, IDictionary<(short, short), TectonicCollision> collisions, float[] offsets,
        TectonicCell a, TectonicCell b)
    {
        TectonicCollision collision;

        if (collisions.ContainsKey((a.continent, b.continent)))
        {
            collision = collisions[(a.continent, b.continent)];
        }
        else
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
                throw new InvalidOperationException();
        }
    }

    private static void HandleConvergentBoundary(Data data, float[] offsets, TectonicCell a, TectonicCell b)
    {
        double strength = VMath.CalculateAngle(a.drift, b.drift) / Math.PI;
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

            Data.Get(offsets, water.position) = (float) (strength * MaxConvergentBoundaryWaterSinking);
        }

        foreach (Vector2i cellPosition in Algorithms.TraverseCells(start, direction.Normalized(), strength * 5.0))
        {
            if (IsOutOfBounds(cellPosition)) continue;

            double maxLifting = data.GetCell(cellPosition).IsLand ? MaxConvergentBoundaryLandLifting : MaxConvergentBoundaryWaterLifting;
            Data.Get(offsets, cellPosition) = (float) (strength * maxLifting);
        }
    }

    private static void HandleTransformBoundary(Data data, TectonicCell a, TectonicCell b)
    {
        ref Cell cellA = ref data.GetCell(a.position);
        cellA.conditions |= CellConditions.SeismicActivity;

        ref Cell cellB = ref data.GetCell(b.position);
        cellB.conditions |= CellConditions.SeismicActivity;
    }

    private static void HandleDivergentBoundary(Data data, float[] offsets, TectonicCell a, TectonicCell b)
    {
        double divergence = VMath.CalculateAngle(a.drift, b.drift) / Math.PI;

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

        Data.Get(offsets, a.position) = (float) (divergence * (cellA.IsLand ? MaxDivergentBoundaryLandOffset : MaxDivergentBoundaryWaterOffset));
        Data.Get(offsets, b.position) = (float) (divergence * (cellB.IsLand ? MaxDivergentBoundaryLandOffset : MaxDivergentBoundaryWaterOffset));
    }

    private static Dictionary<short, Vector2d> GetDriftDirections(List<(short, double)> continentsNodes)
    {
        Dictionary<short, Vector2d> driftDirections = new();

        foreach ((short node, double value) in continentsNodes)
        {
            double angle = value * Math.PI;
            driftDirections[node] = VMath.CreateVectorFromAngle(angle);
        }

        return driftDirections;
    }

    private static void EmitTerrainView(Data data, string path)
    {
        using Bitmap view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetTerrainColor(current));
        }

        view.Save(Path.Combine(path, "terrain_view.png"));
    }

    private static Color GetTerrainColor(Cell current)
    {
        Color water = Color.Blue;
        Color land = Color.Green;

        Color terrain = current.IsLand ? land : water;
        double mixStrength = Math.Abs(current.height) - 0.5;
        bool darken = mixStrength > 0;

        Color mixed = Colors.Mix(terrain, darken ? Color.Black : Color.White, Math.Abs(mixStrength));

        return mixed;
    }

    private static void GenerateTemperature(Data data)
    {
        Vector2 center = new(Width / 2.0f, Width / 2.0f);

        float GetTemperature(float distance)
        {
            return Math.Abs(Math.Abs(distance * 0.025f - 1.0f) % 2.0f - 1.0f);
        }

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            float distance = (center - (x, y)).Length;
            float temperature = GetTemperature(distance);

            ref Cell current = ref data.GetCell(x, y);
            current.temperature = temperature;
        }
    }

    private static Color GetTemperatureColor(Cell current)
    {
        Color tempered = Colors.FromRGB(2.0f * current.temperature, 2.0f * (1 - current.temperature), b: 0.0f);
        Color other = current.IsLand ? Color.Black : tempered;

        return Colors.Mix(tempered, other);
    }

    private static void EmitTemperatureView(Data data, string path)
    {
        using Bitmap view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetTemperatureColor(current));
        }

        view.Save(Path.Combine(path, "temperature_view.png"));
    }

    private static HumidityData[] CreateInitialHumidityData()
    {
        const float initialHumidity = 0.15f;

        var initial = new HumidityData[Width * Width];

        for (var index = 0; index < initial.Length; index++) initial[index].humidity = initialHumidity;

        return initial;
    }

    private static void GenerateHumidity(Data data)
    {
        HumidityData[] current = CreateInitialHumidityData();
        HumidityData[] next = CreateInitialHumidityData();

        const int simulationSteps = 100;

        for (var step = 0; step < simulationSteps; step++)
        {
            SimulateClimate(data, current, next);
            (current, next) = (next, current);
        }

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell cell = ref data.GetCell(x, y);
            cell.humidity = Data.Get(current, (x, y)).humidity;
        }
    }

    private static void SimulateClimate(Data data, HumidityData[] current, HumidityData[] next)
    {
        Parallel.For(fromInclusive: 0,
            CellCount,
            index =>
            {
                Vector2i position = Data.GetPosition(index);
                Data.Get(next, position) = SimulateCellClimate(data, current, position);
            });
    }

    private static IEnumerable<(Vector2i position, bool isInWind)> GetNeighbors(Vector2i position)
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
    private static HumidityData SimulateCellClimate(in Data data, in HumidityData[] state, Vector2i position)
    {
        const float evaporationRate = 0.5f;
        const float precipitationRate = 0.25f;
        const float runoffRate = 0.25f;
        const float windStrength = 5.0f;

        Cell cell = data.GetCell(position);
        HumidityData current = Data.Get(state, position);

        HumidityData next;

        next.clouds = current.clouds;
        next.humidity = current.humidity;
        next.dispersal = 0.0f;
        next.runoff = 0.0f;

        if (cell.IsLand)
        {
            float evaporation = next.humidity * evaporationRate;
            next.humidity -= evaporation;
            next.clouds += evaporation;
        }
        else
        {
            next.humidity = 1.0f;
            next.clouds += evaporationRate;
        }

        float precipitation = next.clouds * precipitationRate;
        next.clouds -= precipitation;
        next.humidity += precipitation;

        float cloudMaximum = 1.0f - Math.Min(cell.height, cell.temperature - 0.1f);

        if (next.clouds > cloudMaximum)
        {
            next.humidity += next.clouds - cloudMaximum;
            next.clouds = cloudMaximum;
        }

        next.dispersal = next.clouds * (1.0f / (3.0f + windStrength));
        next.runoff = next.humidity * runoffRate * (1.0f / 4.0f);
        next.clouds = 0.0f;

        foreach ((Vector2i neighborPosition, bool isInWind) in GetNeighbors(position))
        {
            Cell neighborCell = data.GetCell(neighborPosition);
            HumidityData neighborData = Data.Get(state, neighborPosition);

            next.clouds += isInWind ? neighborData.dispersal * windStrength : neighborData.dispersal;

            if (neighborCell.height > cell.height) next.humidity += neighborData.runoff;

            if (neighborCell.height < cell.height) next.humidity -= next.runoff;
        }

        next.humidity = Math.Min(next.humidity, cell.temperature);

        return next;
    }

    private static Color GetHumidityColor(Cell current)
    {
        Color precipitation = Colors.FromRGB(current.humidity, current.humidity, current.humidity);

        return current.IsLand ? precipitation : Color.Aqua;
    }

    private static void EmitHumidityView(Data data, string path)
    {
        using Bitmap view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetHumidityColor(current));
        }

        view.Save(Path.Combine(path, "precipitation_view.png"));
    }

    private static Color GetBiomeColor(Cell current, BiomeDistribution biomes)
    {
        return current.IsLand ? biomes.GetBiome(current.temperature, current.humidity).Color : Color.White;
    }

    private static void EmitBiomeView(Data data, BiomeDistribution biomes, string path)
    {
        using Bitmap view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetBiomeColor(current, biomes));
        }

        view.Save(Path.Combine(path, "biome_view.png"));
    }

    private static Color GetStoneTypeColor(Cell current)
    {
        if (current.IsLand)
            return current.stoneType switch
            {
                StoneType.Granite => Color.Green,
                StoneType.Limestone => Color.Blue,
                StoneType.Marble => Color.Red,
                StoneType.Sandstone => Color.Yellow,
                _ => Color.Black
            };

        return Color.White;
    }

    private static void EmitStoneView(Data data, string path)
    {
        using Bitmap view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetStoneTypeColor(current));
        }

        view.Save(Path.Combine(path, "stone_view.png"));
    }

    private record struct HumidityData
    {
        public float clouds;

        public float dispersal;
        public float humidity;
        public float runoff;
    }

    private enum TectonicCollision
    {
        Transform,
        Convergent,
        Divergent
    }

    private record struct TectonicCell
    {
        public short continent;
        public Vector2d drift;
        public Vector2i position;
    }
}
