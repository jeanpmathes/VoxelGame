// <copyright file="Decoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Decorations are placed in the world during the decoration step of world generation.
///     They are small structures and elements.
/// </summary>
public class Decoration
{
    /// <summary>
    ///     Get the size of the decoration. Must be less or equal than <see cref="Section.Size" />.
    /// </summary>
    public int Size => 1;

    /// <summary>
    ///     The rarity of the decoration. Must be between 0 and 1. A higher value means a lower chance of placement.
    /// </summary>
    public float Rarity => 3.5f;

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

    private void DecorateColumn((int x, int z) column, Noise noise, in Context context)
    {
        Vector3i position = context.Position.FirstBlock + (column.x, 0, column.z);

        Map.Sample sample = context.Map.GetSample(position.Xz);

        if (!context.Biomes.Contains(sample.ActualBiome)) return;

        Control control = new();

        for (var y = 0; y < Section.Size && !control.SkipColumn; y++)
        {
            control.Reset();

            position = context.Position.FirstBlock + (column.x, y, column.z);

            if (noise.CheckCandidate(position, Rarity)) DecoratePosition(position, control, context);
        }
    }

    private void DecoratePosition(Vector3i position, Control control, in Context context)
    {
        if (CanPlace(position, context)) DoPlace(position, control, context);
    }

    private bool CanPlace(Vector3i position, in Context context)
    {
        if (!context.GetContent(position).IsEmpty) return false;
        if (!context.GetContent(position.Below()).Block.IsSolidAndFull) return false;

        return true;
    }

    private void DoPlace(Vector3i position, Control control, in Context context)
    {
        var content = new Content(Block.Pulsating);
        context.SetContent(position, content);

        control.SkipColumn = true;
    }

    /// <summary>
    ///     The context in which placement in a section occurs.
    /// </summary>
    /// <param name="Position">The position of the section.</param>
    /// <param name="Sections">The section and its neighbors.</param>
    /// <param name="Biomes">The biomes in which the decoration may be placed.</param>
    /// <param name="Noise">The noise used for decoration placement.</param>
    /// <param name="Map">The map of the world.</param>
    public readonly record struct Context(SectionPosition Position, Section[,,] Sections, ISet<Biome> Biomes, FastNoiseLite Noise, Map Map)
    {
        private Section GetSection(Vector3i position)
        {
            SectionPosition target = SectionPosition.From(position);
            (int dx, int dy, int dz) = Position.OffsetTo(target);

            Debug.Assert(dx is -1 or 0 or 1);
            Debug.Assert(dy is -1 or 0 or 1);
            Debug.Assert(dz is -1 or 0 or 1);

            return Sections[dx + 1, dy + 1, dz + 1];
        }

        /// <summary>
        ///     Get the content of a position in the neighborhood of the section.
        /// </summary>
        /// <param name="position">The position. Must be in the same section or a neighbor.</param>
        /// <returns>The content of the position.</returns>
        public Content GetContent(Vector3i position)
        {
            uint data = GetSection(position).GetContent(position);
            Section.Decode(data, out Content content);

            return content;
        }

        /// <summary>
        ///     Set the content of a position in the neighborhood of the section.
        /// </summary>
        /// <param name="position">The position. Must be in the same section or a neighbor.</param>
        /// <param name="content">The content of the position.</param>
        public void SetContent(Vector3i position, Content content)
        {
            uint data = Section.Encode(content);

            GetSection(position).SetContent(position, data);
        }
    }

    private sealed class Noise
    {
        private readonly FastNoiseLite noiseGenerator;
        private readonly Random randomNumberGenerator;

        public Noise(in Context context)
        {
            noiseGenerator = context.Noise;
            randomNumberGenerator = new Random(context.Position.GetHashCode());
        }

        public bool CheckCandidate(Vector3i position, float rarity)
        {
            float noise = noiseGenerator.GetNoise(position.X, position.Y, position.Z);
            float random = randomNumberGenerator.NextSingle();

            return noise > random * rarity;
        }
    }

    /// <summary>
    ///     Control the current decoration.
    /// </summary>
    protected class Control
    {
        /// <summary>
        ///     Whether the rest of the column should be skipped.
        /// </summary>
        public bool SkipColumn { get; set; }

        /// <summary>
        ///     Reset the control state.
        /// </summary>
        public void Reset()
        {
            SkipColumn = false;
        }
    }
}
