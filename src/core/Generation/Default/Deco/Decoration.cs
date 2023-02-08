// <copyright file="Decoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Decorations are placed in the world during the decoration step of world generation.
///     They are small structures and elements.
/// </summary>
public abstract class Decoration
{
    private readonly Decorator decorator;

    /// <summary>
    ///     Creates a new decoration.
    /// </summary>
    /// <param name="name">The name of the decoration. Must be unique.</param>
    /// <param name="rarity">
    ///     The rarity of the decoration. Must be between 0 and 1. A higher value indicates a lower chance of
    ///     placement.
    /// </param>
    /// <param name="decorator">The decorator that will be used to place the decoration.</param>
    protected Decoration(string name, float rarity, Decorator decorator)
    {
        Name = name;
        Rarity = rarity;

        this.decorator = decorator;
    }

    /// <summary>
    ///     Get the size of the decoration. Must be less or equal than <see cref="Section.Size" />.
    /// </summary>
    public abstract int Size { get; }

    /// <summary>
    ///     The rarity of the decoration. Must be between 0 and 1. A higher value indicates a lower chance of placement.
    /// </summary>
    private float Rarity { get; }

    /// <summary>
    ///     Get the name of the decoration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Place decorations of this type in a section.
    /// </summary>
    /// <param name="context">The context in which placement occurs.</param>
    public void Place(Context context)
    {
        Noise noise = new(context);

        for (var x = 0; x < Section.Size; x++)
        for (var z = 0; z < Section.Size; z++)
            DecorateColumn((x, z), noise, context);
    }

    private void DecorateColumn((int x, int z) column, Noise noise, Context context)
    {
        Vector3i position = context.Position.FirstBlock + (column.x, 0, column.z);

        Map.Sample sample = context.Generator.Map.GetSample(position.Xz);

        if (!context.Biomes.Contains(sample.ActualBiome)) return;

        int surfaceHeight = Generator.GetWorldHeight(column, sample, out _);

        PlacementContext placementContext = new(Random: 0.0f, Depth: 0, context.Generator.Map.GetStoneType((column.x, 0, column.z), sample), context.Palette);

        for (var y = 0; y < Section.Size; y++)
        {
            position = context.Position.FirstBlock + (column.x, y, column.z);

            if (!noise.CheckCandidate(position, Rarity, out float random)) continue;

            placementContext = placementContext with {Random = random, Depth = surfaceHeight - position.Y};

            DecoratePosition(position, placementContext, context);
        }
    }

    private void DecoratePosition(Vector3i position, in PlacementContext context, IGrid grid)
    {
        if (decorator.CanPlace(position, context, grid)) DoPlace(position, context, grid);
    }

    /// <summary>
    ///     Place the decoration at the given position.
    /// </summary>
    /// <param name="position">The position at which to place the decoration.</param>
    /// <param name="placementContext">The placement context object.</param>
    /// <param name="grid">The grid that is being decorated.</param>
    protected abstract void DoPlace(Vector3i position, in PlacementContext placementContext, IGrid grid);

    /// <summary>
    ///     The context in which placement in a section occurs.
    /// </summary>
    /// <param name="Position">The position of the section.</param>
    /// <param name="Sections">The section and its neighbors.</param>
    /// <param name="Biomes">The biomes in which the decoration may be placed.</param>
    /// <param name="Noise">The noise used for decoration placement.</param>
    /// <param name="Index">The current index of the decoration.</param>
    /// <param name="Palette">The palette of the generation.</param>
    /// <param name="Generator">The generator that is placing the decoration.</param>
    public record Context(SectionPosition Position, Array3D<Section> Sections, ISet<Biome> Biomes, Array3D<float> Noise, int Index, Palette Palette, Generator Generator) : IGrid
    {
        /// <summary>
        ///     Get the content of a position in the neighborhood of the section.
        /// </summary>
        /// <param name="position">The position. Must be in the same section or a neighbor.</param>
        /// <returns>The content of the position.</returns>
        public Content? GetContent(Vector3i position)
        {
            uint data = GetSection(position).GetContent(position);
            Section.Decode(data, out Content content);

            return content;
        }

        /// <summary>
        ///     Set the content of a position in the neighborhood of the section.
        /// </summary>
        /// <param name="content">The content of the position.</param>
        /// <param name="position">The position. Must be in the same section or a neighbor.</param>
        public void SetContent(Content content, Vector3i position)
        {
            uint data = Section.Encode(content);

            GetSection(position).SetContent(position, data);
        }

        private Section GetSection(Vector3i position)
        {
            SectionPosition target = SectionPosition.From(position);
            (int dx, int dy, int dz) = Position.OffsetTo(target);

            Debug.Assert(dx is -1 or 0 or 1);
            Debug.Assert(dy is -1 or 0 or 1);
            Debug.Assert(dz is -1 or 0 or 1);

            return Sections[dx + 1, dy + 1, dz + 1];
        }
    }

    private sealed class Noise
    {
        private readonly Array3D<float> noise;
        private readonly Random randomNumberGenerator;

        public Noise(in Context context)
        {
            noise = context.Noise;
            randomNumberGenerator = new Random(HashCode.Combine(context.Position, context.Index));
        }

        public bool CheckCandidate(Vector3i position, float rarity, out float random)
        {
            random = randomNumberGenerator.NextSingle();
            (int x, int y, int z) = Section.ToLocalPosition(position);

            return noise[x, y, z] > random * rarity;
        }
    }

    /// <summary>
    ///     The placement context of the current decoration.
    /// </summary>
    /// <param name="Random">The random value for the placement.</param>
    /// <param name="Depth">The depth of the position.</param>
    /// <param name="StoneType">The stone type of the current column.</param>
    /// <param name="Palette">The palette of the world generation.</param>
    public record struct PlacementContext(float Random, int Depth, Map.StoneType StoneType, Palette Palette);
}

