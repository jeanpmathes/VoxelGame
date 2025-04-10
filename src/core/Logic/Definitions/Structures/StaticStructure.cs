// <copyright file="StaticStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
///     A static structure can be stored and loaded from a file.
/// </summary>
public sealed partial class StaticStructure : Structure, IResource, ILocated
{
    private const Int32 MaxSize = 1024;

    private readonly Content?[,,] contents;

    private StaticStructure(Content?[,,] contents, Vector3i extents)
    {
        this.contents = contents;

        Identifier = RID.Virtual;
        Extents = extents;
    }

    private StaticStructure(Definition definition, String name, RID identifier, IResourceContext? context)
    {
        Identifier = identifier;
        Extents = new Vector3i(MaxSize);

        if (!IsInExtents(GetVector(definition.Extents, name)))
            throw new FileFormatException(name, $"Extents must be positive and not exceed {MaxSize} in any dimension.");

        Vector3i extents = GetVector(definition.Extents, name);

        Extents = extents;

        contents = new Content?[extents.X, extents.Y, extents.Z];

        foreach (Placement placement in definition.Placements)
            ApplyPlacement(placement, name, context);
    }

    /// <inheritdoc />
    public override Vector3i Extents { get; }

    /// <inheritdoc />
    public static String[] Path { get; } = ["Structures"];

    /// <inheritdoc />
    public static String FileExtension => "json";

    /// <inheritdoc />
    public RID Identifier { get; }

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Structure;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

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
    ///     Load a structure safely, using a fallback if loading fails.
    /// </summary>
    /// <param name="directory">The directory to load from.</param>
    /// <param name="name">The name of the structure.</param>
    /// <param name="token">The token to cancel the operation.</param>
    /// <returns>The loaded structure, or a fallback if loading failed.</returns>
    public static async Task<StaticStructure> LoadSafelyAsync(DirectoryInfo directory, String name, CancellationToken token = default)
    {
        Result<StaticStructure> result = await LoadAsync(directory.GetFile(FileSystem.GetResourceFileName<StaticStructure>(name)), context: null, token).InAnyContext();

        return result.Switch(
            success =>
            {
                LogSuccessfulStructureLoad(logger, name);

                return success;
            },
            exception =>
            {
                LogFailedStructureLoad(logger, exception, name);

                return CreateFallback();
            }
        );
    }

    /// <summary>
    ///     Load a structure from a file.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="context">The context to report loading issues to, or <c>null</c> to just log them.</param>
    /// <param name="token">The token to cancel the operation.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result<StaticStructure>> LoadAsync(FileInfo file, IResourceContext? context, CancellationToken token = default)
    {
        Result<Definition> result = await Serialize.LoadJsonAsync<Definition>(file, token).InAnyContext();

        return result.Map(definition => new StaticStructure(definition, file.GetFileNameWithoutExtension(), RID.Path(file), context));
    }

    /// <summary>
    ///     Create a fallback structure.
    /// </summary>
    /// <returns>The fallback structure.</returns>
    public static StaticStructure CreateFallback()
    {
        var fallback = new Content?[1, 1, 1];
        fallback[0, 0, 0] = new Content(Elements.Blocks.Instance.Error);

        return new StaticStructure(fallback, Vector3i.One);
    }

    private void ApplyPlacement(Placement placement, String name, IResourceContext? context)
    {
        Vector3i position = GetVector(placement.Position, name);

        if (!IsInExtents(position))
            throw new FileFormatException(name, $"Position {position} is out of bounds.");

        if (contents[position.X, position.Y, position.Z] != null)
            throw new FileFormatException(name, $"Position {position} is already occupied.");

        var content = Content.Default;

        Block? block = Elements.Blocks.Instance.TranslateNamedID(placement.Block);

        if (block == null)
        {
            if (context != null) context.ReportWarning(this, $"Unknown block '{placement.Block}' in structure '{name}'");
            else LogUnknownBlockInStructure(logger, placement.Block, name);

            block = Elements.Blocks.Instance.Air;
        }

        content.Block = new BlockInstance(block, (((UInt32) placement.Data << Section.DataShift) & Section.DataMask) >> Section.DataShift);

        Fluid? fluid = Elements.Fluids.Instance.TranslateNamedID(placement.Fluid);

        if (fluid == null)
        {
            if (context != null) context.ReportWarning(this, $"Unknown fluid '{placement.Fluid}' in structure '{name}'");
            else LogUnknownFluidInStructure(logger, placement.Fluid, name);

            fluid = Elements.Fluids.Instance.None;
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
    protected override (Content content, Boolean overwrite)? GetContent(Vector3i offset, Single random)
    {
        Debug.Assert(IsInExtents(offset));

        Content? content = contents[offset.X, offset.Y, offset.Z];

        if (content == null) return null;

        return (content.Value, overwrite: true);
    }

    /// <inheritdoc />
    protected override Random? GetRandomness(Int32 seed)
    {
        return null;
    }

    /// <summary>
    ///     Save the structure in a file.
    /// </summary>
    /// <param name="directory">The directory to save the file in.</param>
    /// <param name="name">The name of the structure.</param>
    /// <param name="token">The token to cancel the operation.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<Result> SaveAsync(DirectoryInfo directory, String name, CancellationToken token = default)
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

        Result result = await Serialize.SaveJsonAsync(definition, directory.GetFile(FileSystem.GetResourceFileName<StaticStructure>(name)), token).InAnyContext();

        result.Switch(
            () => {},
            exception => LogFailedStructureStore(logger, exception, name)
        );

        return result;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<StaticStructure>();

    [LoggerMessage(EventId = LogID.StaticStructure + 0, Level = LogLevel.Warning, Message = "Could not load the structure '{Name}' because an exception occurred, fallback will be used instead")]
    private static partial void LogFailedStructureLoad(ILogger logger, Exception exception, String name);

    [LoggerMessage(EventId = LogID.StaticStructure + 1, Level = LogLevel.Debug, Message = "Loaded StaticStructure: {Name}")]
    private static partial void LogSuccessfulStructureLoad(ILogger logger, String name);

    [LoggerMessage(EventId = LogID.StaticStructure + 2, Level = LogLevel.Warning, Message = "Unknown block '{Block}' in structure '{Name}'")]
    private static partial void LogUnknownBlockInStructure(ILogger logger, String block, String name);

    [LoggerMessage(EventId = LogID.StaticStructure + 3, Level = LogLevel.Warning, Message = "Unknown fluid '{Fluid}' in structure '{Name}'")]
    private static partial void LogUnknownFluidInStructure(ILogger logger, String fluid, String name);

    [LoggerMessage(EventId = LogID.StaticStructure + 4, Level = LogLevel.Error, Message = "Could not store the structure '{Name}'")]
    private static partial void LogFailedStructureStore(ILogger logger, Exception exception, String name);

    #endregion LOGGING
}
