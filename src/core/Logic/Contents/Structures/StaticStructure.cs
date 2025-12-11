// <copyright file="StaticStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     A static structure can be stored and loaded from a file.
/// </summary>
public sealed partial class StaticStructure : Structure, IResource, ILocated, IIssueSource
{
    /// <summary>
    ///     The maximum size of a structure in any dimension.
    /// </summary>
    public const Int32 MaxSize = 1024;

    private readonly Content?[,,] contents;

    private StaticStructure(Content?[,,] contents, Vector3i extents)
    {
        this.contents = contents;

        Identifier = RID.Virtual;
        Extents = extents;
    }

    private StaticStructure(StaticStructureDefinition definition, String name, RID identifier, IResourceContext? context)
    {
        StaticStructureDefinitionReader reader = new(definition, name);

        Identifier = identifier;
        Extents = reader.Extents;

        contents = new Content?[Extents.X, Extents.Y, Extents.Z];

        while (reader.AdvanceToNextPlacement())
        {
            Vector3i position = reader.Position;

            var content = Content.Default;

            State? state = reader.GetBlock(out String namedBlockID);

            if (state == null)
            {
                if (context != null) context.ReportWarning(this, $"Unknown block '{namedBlockID}' in structure '{name}'");
                else LogUnknownBlockInStructure(logger, namedBlockID, name);

                state = Blocks.Instance.Core.Air.States.Default;
            }

            content.Block = state.Value;

            FluidInstance? fluid = reader.GetFluid(out String namedFluidID);

            if (fluid == null)
            {
                if (context != null) context.ReportWarning(this, $"Unknown fluid '{namedFluidID}' in structure '{name}'");
                else LogUnknownFluidInStructure(logger, namedFluidID, name);

                fluid = Voxels.Fluids.Instance.None.AsInstance();
            }

            content.Fluid = fluid.Value;

            contents[position.X, position.Y, position.Z] = content;
        }
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
        Result<StaticStructureDefinition> result = await Serialize.LoadJsonAsync<StaticStructureDefinition>(file, token).InAnyContext();

        return result.Map(definition => new StaticStructure(definition, file.GetFileNameWithoutExtension(), RID.Path(file), context));
    }

    /// <summary>
    ///     Create a fallback structure.
    /// </summary>
    /// <returns>The fallback structure.</returns>
    public static StaticStructure CreateFallback()
    {
        var fallback = new Content?[1, 1, 1];
        fallback[0, 0, 0] = Content.Create(Blocks.Instance.Core.Error);

        return new StaticStructure(fallback, Vector3i.One);
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
        StaticStructureBuilder builder = new();

        for (var x = 0; x < Extents.X; x++)
        for (var y = 0; y < Extents.Y; y++)
        for (var z = 0; z < Extents.Z; z++)
        {
            if (contents[x, y, z] is not {} content) continue;

            builder.AddPlacement(new Vector3i(x, y, z), content, content.Fluid.IsStatic);
        }

        StaticStructureDefinition definition = builder.Build(Extents);

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
