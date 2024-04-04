// <copyright file="MapGeneration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Core.Generation.Default;

#pragma warning disable S4017

public partial class Map
{
    private const Single MinimumLandHeight = +0.05f;
    private const Single AverageWaterHeight = -0.4f;

    private const Double PieceHeightChangeRange = 0.2;

    private const Double MaxDivergentBoundaryLandOffset = -0.025;
    private const Double MaxDivergentBoundaryWaterOffset = +0.2;

    private const Double MaxConvergentBoundaryLandLifting = +0.7;
    private const Double MaxConvergentBoundaryWaterLifting = +0.4;
    private const Double MaxConvergentBoundaryWaterSinking = -0.4;

    private static (List<List<Int16>>, Dictionary<Int16, Double>) FillWithPieces(Data data, GeneratingNoise noise)
    {
        noise.Pieces.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.Pieces.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        noise.Pieces.SetFrequency(frequency: 0.05f);

        Int16 currentPiece = 0;
        Dictionary<Double, Int16> valueToPiece = new();

        Dictionary<Int16, HashSet<Int16>> adjacencyHashed = new();

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Double value = noise.Pieces.GetNoise(x, y);
            ref Cell current = ref data.GetCell(x, y);

            if (!valueToPiece.ContainsKey(value)) valueToPiece[value] = currentPiece++;

            current.continent = valueToPiece[value];

            UpdateAdjacencies(data, adjacencyHashed, ref current, (x, y));
        }

        return (Algorithms.BuildAdjacencyList(adjacencyHashed), Algorithms.InvertDictionary(valueToPiece));
    }

    private static void UpdateAdjacencies(Data data, IDictionary<Int16, HashSet<Int16>> adjacencyHashed, ref Cell current, (Int32, Int32) position)
    {
        void AddAdjacency(Int16 a, Int16 b)
        {
            adjacencyHashed.GetOrAdd(a).Add(b);
            adjacencyHashed.GetOrAdd(b).Add(a);
        }

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

            Int16 anyNeighbor = merge.Find(adjacency[piece].First());

            if (adjacency[piece].All(neighbor => anyNeighbor == merge.Find(neighbor))) merge.Union(piece, anyNeighbor);
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
    private static void DoLandBorderFlooding(Data data, IList<Boolean> isLand)
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

    private static void GenerateTerrain(Data data, GeneratingNoise noise)
    {
        (List<List<Int16>> adjacency, Dictionary<Int16, Double> pieceToValue) pieces = FillWithPieces(data, noise);

        AddPieceHeights(data, pieces.pieceToValue);

        (List<(Int16, Double)> nodes, Dictionary<Int16, List<Int16>> adjancecy) continents = BuildContinents(data, pieces.adjacency, pieces.pieceToValue);

        GenerateStoneTypes(data, noise);
        SimulateTectonics(data, continents);
    }

    private static void AddPieceHeights(Data data, IDictionary<Int16, Double> pieceToValue)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell cell = ref data.GetCell(x, y);

            Double offset = pieceToValue[cell.continent] * PieceHeightChangeRange;
            cell.height += (Single) offset;
        }
    }

    private static void SimulateTectonics(Data data,
        (List<(Int16, Double)> nodes, Dictionary<Int16, List<Int16>> adjancecy) continents)
    {
        Dictionary<Int16, Vector2d> driftDirections = GetDriftDirections(continents.nodes);
        Dictionary<(Int16, Int16), TectonicCollision> collisions = new();

        var offsets = new Single[CellCount];

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

    private static void GenerateStoneTypes(Data data, GeneratingNoise noise)
    {
        noise.Stone.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.Stone.SetFrequency(frequency: 0.08f);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Single value = noise.Stone.GetNoise(x, y);
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

    private static void AddOffsetsToData(Data data, Single[] offsets)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            ref Cell current = ref data.GetCell(x, y);
            current.height += Data.Get(offsets, x, y);
        }
    }

    private static Boolean IsOutOfBounds(Vector2i position)
    {
        return position.X is < 0 or >= Width || position.Y is < 0 or >= Width;
    }

    private static void HandleTectonicCollision(Data data, IDictionary<(Int16, Int16), TectonicCollision> collisions, Single[] offsets,
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

    /// <summary>
    ///     Handle a convergent boundary between two tectonic plates.
    ///     A convergent boundary is where two plates move towards each other.
    ///     If both cells are land, they are pushed upwards.
    ///     If one cell is land and the other is not, the water cell is pushed under the land cell.
    /// </summary>
    private static void HandleConvergentBoundary(Data data, Single[] offsets, TectonicCell a, TectonicCell b)
    {
        Double strength = VMath.CalculateAngle(a.drift, b.drift) / Math.PI;
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

            Data.Get(offsets, water.position) = (Single) (strength * MaxConvergentBoundaryWaterSinking);
        }

        foreach (Vector2i cellPosition in Algorithms.TraverseCells(start, direction.Normalized(), strength * 5.0))
        {
            if (IsOutOfBounds(cellPosition)) continue;

            Double maxLifting = data.GetCell(cellPosition).IsLand ? MaxConvergentBoundaryLandLifting : MaxConvergentBoundaryWaterLifting;
            Data.Get(offsets, cellPosition) = (Single) (strength * maxLifting);
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
    private static void HandleDivergentBoundary(Data data, Single[] offsets, TectonicCell a, TectonicCell b)
    {
        Double divergence = VMath.CalculateAngle(a.drift, b.drift) / Math.PI;

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

        Data.Get(offsets, a.position) = (Single) (divergence * (cellA.IsLand ? MaxDivergentBoundaryLandOffset : MaxDivergentBoundaryWaterOffset));
        Data.Get(offsets, b.position) = (Single) (divergence * (cellB.IsLand ? MaxDivergentBoundaryLandOffset : MaxDivergentBoundaryWaterOffset));
    }

    private static Dictionary<Int16, Vector2d> GetDriftDirections(List<(Int16, Double)> continentsNodes)
    {
        Dictionary<Int16, Vector2d> driftDirections = new();

        foreach ((Int16 node, Double value) in continentsNodes)
        {
            Double angle = value * Math.PI;
            driftDirections[node] = VMath.CreateVectorFromAngle(angle);
        }

        return driftDirections;
    }

    private static void EmitTerrainView(Data data, DirectoryInfo path)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetTerrainColor(current));
        }

        view.Save(path.GetFile("terrain_view.png"));
    }

    private static Color GetTerrainColor(Cell current)
    {
        Color water = Color.Blue;
        Color land = Color.Green;

        Color terrain = current.IsLand ? land : water;
        Double mixStrength = Math.Abs(current.height) - 0.5;
        Boolean darken = mixStrength > 0;

        Color mixed = Colors.Mix(terrain, darken ? Color.Black : Color.White, Math.Abs(mixStrength));

        return mixed;
    }

    private static void GenerateTemperature(Data data)
    {
        Vector2 center = new(Width / 2.0f, Width / 2.0f);

        Single GetTemperature(Single distance)
        {
            return Math.Abs(Math.Abs(distance * 0.025f - 1.0f) % 2.0f - 1.0f);
        }

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Single distance = (center - (x, y)).Length;
            Single temperature = GetTemperature(distance);

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

    private static void EmitTemperatureView(Data data, DirectoryInfo path)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetTemperatureColor(current));
        }

        view.Save(path.GetFile("temperature_view.png"));
    }

    private static HumidityData[] CreateInitialHumidityData()
    {
        const Single initialHumidity = 0.15f;

        var initial = new HumidityData[Width * Width];

        for (var index = 0; index < initial.Length; index++) initial[index].humidity = initialHumidity;

        return initial;
    }

    private static void GenerateHumidity(Data data)
    {
        HumidityData[] current = CreateInitialHumidityData();
        HumidityData[] next = CreateInitialHumidityData();

        const Int32 simulationSteps = 100;

        for (var step = 0; step < simulationSteps; step++)
        {
            SimulateClimate(data, current, next);
            VMath.Swap(ref current, ref next);
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
    private static HumidityData SimulateCellClimate(in Data data, in HumidityData[] state, Vector2i position)
    {
        const Single evaporationRate = 0.5f;
        const Single precipitationRate = 0.25f;
        const Single runoffRate = 0.25f;
        const Single windStrength = 5.0f;

        Cell cell = data.GetCell(position);
        HumidityData current = Data.Get(state, position);

        HumidityData next;

        next.clouds = current.clouds;
        next.humidity = current.humidity;
        next.dispersal = 0.0f;
        next.runoff = 0.0f;

        if (cell.IsLand)
        {
            Single evaporation = next.humidity * evaporationRate;
            next.humidity -= evaporation;
            next.clouds += evaporation;
        }
        else
        {
            next.humidity = 1.0f;
            next.clouds += evaporationRate;
        }

        Single precipitation = next.clouds * precipitationRate;
        next.clouds -= precipitation;
        next.humidity += precipitation;

        Single cloudMaximum = 1.0f - Math.Min(cell.height, cell.temperature - 0.1f);

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

    private static void EmitHumidityView(Data data, DirectoryInfo path)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetHumidityColor(current));
        }

        view.Save(path.GetFile("precipitation_view.png"));
    }

    private static Color GetBiomeColor(Cell current, BiomeDistribution biomes)
    {
        return current.IsLand ? biomes.GetBiome(current.temperature, current.humidity).Color : Color.White;
    }

    private static void EmitBiomeView(Data data, BiomeDistribution biomes, DirectoryInfo path)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetBiomeColor(current, biomes));
        }

        view.Save(path.GetFile("biome_view.png"));
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

    private static void EmitStoneView(Data data, DirectoryInfo path)
    {
        Image view = new(Width, Width);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Width; y++)
        {
            Cell current = data.GetCell(x, y);
            view.SetPixel(x, y, GetStoneTypeColor(current));
        }

        view.Save(path.GetFile("stone_view.png"));
    }

    private record struct HumidityData
    {
        public Single clouds;

        public Single dispersal;
        public Single humidity;
        public Single runoff;
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
