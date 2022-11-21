// <copyright file="StaticStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Structures;

/// <summary>
///     A static structure can be stored and loaded from a file.
/// </summary>
public partial class StaticStructure : Structure
{
    private const int MaxSize = 1024;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<StaticStructure>();

    private static readonly string structureDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Resources",
        "Structures");

    private readonly Content?[,,] contents;

    private StaticStructure(Content?[,,] contents, Vector3i extents)
    {
        this.contents = contents;
        Extents = extents;
    }

    private StaticStructure(Definition definition, string name)
    {
        Extents = new Vector3i(MaxSize);

        if (!IsInExtents(GetVector(definition.Extents, name)))
            throw new FileFormatException(name, $"Extents must be positive and not exceed {MaxSize} in any dimension.");

        Vector3i extents = GetVector(definition.Extents, name);

        Extents = extents;
        contents = new Content?[extents.X, extents.Y, extents.Z];

        foreach (Placement placement in definition.Placements) ApplyPlacement(placement, name);
    }

    /// <inheritdoc />
    public override Vector3i Extents { get; }

    /// <inheritdoc />
    public override bool IsPlaceable => true;

    /// <summary>
    ///     Read a structure from a grid.
    /// </summary>
    /// <param name="grid">The grid to read from.</param>
    /// <param name="position">The position of the structure.</param>
    /// <param name="extents">The extents of the structure.</param>
    /// <returns>The structure, or null if arguments are invalid.</returns>
    public static StaticStructure? Read(IReadOnlyGrid grid, Vector3i position, Vector3i extents)
    {
        if (!IsExtentsAcceptable(extents)) return null;

        var data = new Content?[extents.X, extents.Y, extents.Z];

        for (var x = 0; x < extents.X; x++)
        for (var y = 0; y < extents.Y; y++)
        for (var z = 0; z < extents.Z; z++)
        {
            Content? content = grid.GetContent(position + new Vector3i(x, y, z));

            if (content == null) continue;
            if (content.Value.IsEmpty) continue;

            data[x, y, z] = content;
        }

        return new StaticStructure(data, extents);
    }

    private static bool IsExtentsAcceptable(Vector3i extents)
    {
        if (extents.X > MaxSize || extents.Y > MaxSize || extents.Z > MaxSize) return false;
        if (extents.X < 1 || extents.Y < 1 || extents.Z < 1) return false;

        return true;
    }

    /// <summary>
    ///     Load a structure from the application resources.
    /// </summary>
    /// <param name="name">The name of the structure.</param>
    /// <returns>The loaded structure, or a fallback structure if the loading failed.</returns>
    public static StaticStructure Load(string name)
    {
        return Load(structureDirectory, name);
    }

    /// <summary>
    ///     Load a structure.
    /// </summary>
    /// <param name="directory">The directory to load from.</param>
    /// <param name="name">The name of the structure.</param>
    /// <returns>The loaded structure, or null if the loading failed.</returns>
    public static StaticStructure Load(string directory, string name)
    {
        try
        {
            string json = File.ReadAllText(Path.Combine(directory, GetFileName(name)));
            Definition definition = JsonSerializer.Deserialize<Definition>(json) ?? new Definition();

            logger.LogDebug(Events.ResourceLoad, "Loaded StaticStructure: {Name}", name);

            return new StaticStructure(definition, name);
        }
        catch (Exception e) when (e is IOException or FileNotFoundException or JsonException or FileFormatException)
        {
            logger.LogWarning(
                Events.MissingResource,
                e,
                "Could not load the structure '{Name}' because an exception occurred, fallback will be used instead",
                name);

            return CreateFallback();
        }
    }

    private static string GetFileName(string name)
    {
        return $"{name}.json";
    }

    private static StaticStructure CreateFallback()
    {
        var fallback = new Content?[1, 1, 1];
        fallback[0, 0, 0] = new Content(Block.Error);

        return new StaticStructure(fallback, Vector3i.One);
    }

    private void ApplyPlacement(Placement placement, string name)
    {
        Vector3i position = GetVector(placement.Position, name);

        if (!IsInExtents(position))
            throw new FileFormatException(name, $"Position {position} is out of bounds.");

        if (contents[position.X, position.Y, position.Z] != null)
            throw new FileFormatException(name, $"Position {position} is already occupied.");

        var content = Content.Default;

        Block? block = Block.TranslateNamedID(placement.Block);

        if (block == null)
        {
            logger.LogWarning(Events.ResourceLoad, "Unknown block '{Block}' in structure '{Name}'", placement.Block, name);
            block = Block.Air;
        }

        content.Block = new BlockInstance(block, (((uint) placement.Data << Section.DataShift) & Section.DataMask) >> Section.DataShift);

        Fluid? fluid = Fluid.TranslateNamedID(placement.Fluid);

        if (fluid == null)
        {
            logger.LogWarning(Events.ResourceLoad, "Unknown fluid '{Fluid}' in structure '{Name}'", placement.Fluid, name);
            fluid = Fluid.None;
        }

        content.Fluid = new FluidInstance(fluid, (FluidLevel) ((((uint) placement.Level << Section.LevelShift) & Section.LevelMask) >> Section.LevelShift), placement.IsStatic);

        contents[position.X, position.Y, position.Z] = content;
    }

    private static Vector3i GetVector(Vector vector, string name)
    {
        if (vector.Values.Length != 3)
            throw new FileFormatException(name, "Vector must have 3 values.");

        return new Vector3i(vector.Values[0], vector.Values[1], vector.Values[2]);
    }

    private bool IsInExtents(Vector3i position)
    {
        return position.X >= 0 && position.X < Extents.X &&
               position.Y >= 0 && position.Y < Extents.Y &&
               position.Z >= 0 && position.Z < Extents.Z;
    }

    /// <inheritdoc />
    protected override (Content content, bool overwrite)? GetContent(Vector3i offset)
    {
        Debug.Assert(IsInExtents(offset));

        Content? content = contents[offset.X, offset.Y, offset.Z];

        if (content == null) return null;

        return (content.Value, overwrite: true);
    }

    /// <summary>
    ///     Store the structure in a file.
    /// </summary>
    /// <param name="directory">The directory to store the file in.</param>
    /// <param name="name">The name of the structure.</param>
    /// <returns>True if the structure was stored successfully, false otherwise.</returns>
    public bool Store(string directory, string name)
    {
        List<Placement> placements = new();

        for (var x = 0; x < Extents.X; x++)
        for (var y = 0; y < Extents.Y; y++)
        for (var z = 0; z < Extents.Z; z++)
        {
            Content? content = contents[x, y, z];

            if (content == null) continue;

            placements.Add(new Placement
            {
                Position = new Vector {Values = new[] {x, y, z}},
                Block = content.Value.Block.Block.NamedID,
                Data = (int) content.Value.Block.Data,
                Fluid = content.Value.Fluid.Fluid.NamedID,
                Level = (int) content.Value.Fluid.Level,
                IsStatic = content.Value.Fluid.IsStatic
            });
        }

        Definition definition = new()
        {
            Extents = new Vector {Values = new[] {Extents.X, Extents.Y, Extents.Z}},
            Placements = placements.ToArray()
        };

        try
        {
            string json = JsonSerializer.Serialize(definition,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            string path = Path.Combine(directory, GetFileName(name));
            File.WriteAllText(path, json);

            return true;
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            logger.LogError(Events.FileIO, e, "Could not store the structure '{Name}'", name);
        }

        return false;
    }
}
