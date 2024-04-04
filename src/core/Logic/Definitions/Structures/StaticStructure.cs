// <copyright file="StaticStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
///     A static structure can be stored and loaded from a file.
/// </summary>
public partial class StaticStructure : Structure
{
    private const Int32 MaxSize = 1024;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<StaticStructure>();

    private static readonly DirectoryInfo structureDirectory = FileSystem.GetResourceDirectory("Structures");

    private static LoadingContext? loadingContext;

    private readonly Content?[,,] contents;

    private StaticStructure(Content?[,,] contents, Vector3i extents)
    {
        this.contents = contents;
        Extents = extents;
    }

    private StaticStructure(Definition definition, String name)
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
    public override Boolean IsPlaceable => true;

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

    private static Boolean IsExtentsAcceptable(Vector3i extents)
    {
        if (extents.X > MaxSize || extents.Y > MaxSize || extents.Z > MaxSize) return false;
        if (extents.X < 1 || extents.Y < 1 || extents.Z < 1) return false;

        return true;
    }

    /// <summary>
    ///     Set the current loading context. All loading operations will then be performed in that context.
    ///     Loading operations can also be performed without a context.
    ///     When a context is used, no other loading operations on any thread should be performed.
    /// </summary>
    /// <param name="newLoadingContext">The new loading context.</param>
    public static void SetLoadingContext(LoadingContext newLoadingContext)
    {
        Debug.Assert(loadingContext == null);
        loadingContext = newLoadingContext;
    }

    /// <summary>
    ///     Clear the current loading context.
    /// </summary>
    public static void ClearLoadingContext()
    {
        Debug.Assert(loadingContext != null);
        loadingContext = null;
    }

    /// <summary>
    ///     Load a structure from the application resources.
    /// </summary>
    /// <param name="name">The name of the structure.</param>
    /// <returns>The loaded structure, or a fallback structure if the loading failed.</returns>
    public static StaticStructure Load(String name)
    {
        return Load(structureDirectory, name);
    }

    /// <summary>
    ///     Load a structure.
    /// </summary>
    /// <param name="directory">The directory to load from.</param>
    /// <param name="name">The name of the structure.</param>
    /// <returns>The loaded structure, or null if the loading failed.</returns>
    public static StaticStructure Load(DirectoryInfo directory, String name)
    {
        FileInfo file = directory.GetFile(GetFileName(name));

        Exception? exception = Serialize.LoadJSON(file, out Definition definition);

        if (exception != null)
        {
            if (loadingContext != null)
                loadingContext.ReportFailure(Events.ResourceLoad, nameof(StaticStructure), file, exception);
            else
                logger.LogWarning(
                    Events.MissingCreation,
                    exception,
                    "Could not load the structure '{Name}' because an exception occurred, fallback will be used instead",
                    name);

            return CreateFallback();
        }

        if (loadingContext != null) loadingContext.ReportSuccess(Events.ResourceLoad, nameof(StaticStructure), file);
        else logger.LogDebug(Events.CreationLoad, "Loaded StaticStructure: {Name}", name);

        return new StaticStructure(definition, name);
    }

    private static String GetFileName(String name)
    {
        return $"{name}.json";
    }

    private static StaticStructure CreateFallback()
    {
        var fallback = new Content?[1, 1, 1];
        fallback[0, 0, 0] = new Content(Logic.Blocks.Instance.Error);

        return new StaticStructure(fallback, Vector3i.One);
    }

    private void ApplyPlacement(Placement placement, String name)
    {
        Vector3i position = GetVector(placement.Position, name);

        if (!IsInExtents(position))
            throw new FileFormatException(name, $"Position {position} is out of bounds.");

        if (contents[position.X, position.Y, position.Z] != null)
            throw new FileFormatException(name, $"Position {position} is already occupied.");

        var content = Content.Default;

        Block? block = Logic.Blocks.Instance.TranslateNamedID(placement.Block);

        if (block == null)
        {
            logger.LogWarning(Events.ResourceLoad, "Unknown block '{Block}' in structure '{Name}'", placement.Block, name);
            block = Logic.Blocks.Instance.Air;
        }

        content.Block = new BlockInstance(block, (((UInt32) placement.Data << Section.DataShift) & Section.DataMask) >> Section.DataShift);

        Fluid? fluid = Logic.Fluids.Instance.TranslateNamedID(placement.Fluid);

        if (fluid == null)
        {
            logger.LogWarning(Events.ResourceLoad, "Unknown fluid '{Fluid}' in structure '{Name}'", placement.Fluid, name);
            fluid = Logic.Fluids.Instance.None;
        }

        content.Fluid = new FluidInstance(fluid, (FluidLevel) ((((UInt32) placement.Level << Section.LevelShift) & Section.LevelMask) >> Section.LevelShift), placement.IsStatic);

        contents[position.X, position.Y, position.Z] = content;
    }

    private static Vector3i GetVector(Vector vector, String name)
    {
        if (vector.Values.Length != 3)
            throw new FileFormatException(name, "Vector must have 3 values.");

        return new Vector3i(vector.Values[0], vector.Values[1], vector.Values[2]);
    }

    /// <inheritdoc />
    protected override (Content content, Boolean overwrite)? GetContent(Vector3i offset)
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
    public Boolean Store(DirectoryInfo directory, String name)
    {
        List<Placement> placements = [];

        for (var x = 0; x < Extents.X; x++)
        for (var y = 0; y < Extents.Y; y++)
        for (var z = 0; z < Extents.Z; z++)
        {
            Content? content = contents[x, y, z];

            if (content == null) continue;

            placements.Add(new Placement
            {
                Position = new Vector {Values = [x, y, z]},
                Block = content.Value.Block.Block.NamedID,
                Data = (Int32) content.Value.Block.Data,
                Fluid = content.Value.Fluid.Fluid.NamedID,
                Level = (Int32) content.Value.Fluid.Level,
                IsStatic = content.Value.Fluid.IsStatic
            });
        }

        Definition definition = new()
        {
            Extents = new Vector {Values = [Extents.X, Extents.Y, Extents.Z]},
            Placements = placements.ToArray()
        };

        Exception? exception = Serialize.SaveJSON(definition, directory.GetFile(GetFileName(name)));

        if (exception == null) return false;

        logger.LogError(Events.FileIO, exception, "Could not store the structure '{Name}'", name);

        return false;

    }
}
