// <copyright file="Blocks.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Definitions;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements;

// todo: rework the namespaces like content, definitions, elements and so on, also add note to do even more there when fluids are changed to also be blocks

/// <summary>
///     Contains all block definitions of the core game.
/// </summary>
public partial class Blocks(BlockBuilder builder, Registry<Category> categories) : IIssueSource
{
    private readonly List<State> states = [];

    /// <summary>
    ///     Get the singleton instance of the <see cref="Blocks" /> class, which contains all block definitions.
    /// </summary>
    public static Blocks Instance { get; } = new(BlockBuilder.Create(), new Registry<Category>(category => Reflections.GetLongName(category.GetType())));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Core" />
    public Core Core { get; } = categories.Register(new Core(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Environment" />
    public Environment Environment { get; } = categories.Register(new Environment(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Woods" />
    public Woods Woods { get; } = categories.Register(new Woods(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Stones" />
    public Stones Stones { get; } = categories.Register(new Stones(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Metals" />
    public Metals Metals { get; } = categories.Register(new Metals(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Coals" />
    public Coals Coals { get; } = categories.Register(new Coals(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Organic" />
    public Organic Organic { get; } = categories.Register(new Organic(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Flowers" />
    public Flowers Flowers { get; } = categories.Register(new Flowers(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Crops" />
    public Crops Crops { get; } = categories.Register(new Crops(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Construction" />
    public Construction Construction { get; } = categories.Register(new Construction(builder.CreateScoped()));

    /// <inheritdoc cref="VoxelGame.Core.Logic.Elements.Fabricated" />
    public Fabricated Fabricated { get; } = categories.Register(new Fabricated(builder.CreateScoped()));

    /// <summary>
    ///     Get all categories of blocks defined in the game.
    /// </summary>
    public IEnumerable<Category> Categories => categories.Values;

    /// <summary>
    ///     Get the total number of blocks defined in the game.
    /// </summary>
    public UInt32 Count => (UInt32) builder.BlocksByID.Count;

    /// <summary>
    ///     Initialize all blocks. This should be called exactly once during loading.
    /// </summary>
    /// <param name="textureIndexProvider">The texture index provider to use for resolving textures.</param>
    /// <param name="modelProvider">The model provider to use for resolving block models.</param>
    /// <param name="visuals">The visual configuration to use.</param>
    /// <param name="context">The resource context in which loading is done.</param>
    /// <returns>All content defined in this class.</returns>
    public IEnumerable<IContent> Initialize(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IResourceContext context)
    {
        states.Clear();
        
        Validator validator = new(context);
        
        foreach (Block block in builder.BlocksWithCollisionOnID)
        {
            validator.SetScope(block);
            validator.ReportError($"Block with natural name '{block.Name}' ({block.ID}) is part of a named ID collision");
        }
        
        UInt32 offset = 0;

        foreach (Block block in builder.BlocksByID)
        {
            offset += block.Initialize(offset, validator);

            states.AddRange(block.States.GetAllStates());
        }
        
        BehaviorSystem<Block, BlockBehavior>.Bake(validator);
        
        foreach (Block block in builder.BlocksByID)
            block.Activate(textureIndexProvider, modelProvider, visuals);

        if (validator.HasError) return [];
        
        const Int64 maxNumberOfState = Section.BlockStateMask + 1;

        if (states.Count <= maxNumberOfState) 
            return builder.Registry.RetrieveContent();

        context.ReportError(this, $"The total number of block states ({states.Count}) exceeds the maximum allowed ({maxNumberOfState})");
            
        return [];
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public State TranslateStateID(UInt32 id)
    {
        if (id < states.Count)
            return states[(Int32) id];

        LogUnknownStateID(logger, id, Core.Error.NamedID);

        return Core.Error.States.Default;

        // todo: check memory usage of the new block system, especially of the state list used here
        // todo: compare memory usage to before the new block system
        // todo: if memory usage is too high and the list is a big part of that, consider this alternative:
        // todo:    a list of ranges, each entry containing only the end ID (as start is previous entry + 1)
        // todo:    then, a simple binary search can be used
        // todo: if memory usage is fine for now, add a new note in far future to check it again
    }

    /// <summary>
    ///     Translates a block ID to a reference to the block that has that ID. If the ID is not valid, the error block is
    ///     returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or the error block if the ID is not valid.</returns>
    public Block TranslateBlockID(UInt32 id)
    {
        if (id < builder.BlocksByID.Count)
            return builder.BlocksByID[(Int32) id];

        LogUnknownBlockID(logger, id, Core.Error.NamedID);

        return Core.Error;
    }

    /// <summary>
    ///     Translate a named ID to the block that has that ID.
    /// </summary>
    /// <param name="namedID">The named ID to translate.</param>
    /// <returns>The block, or null if no block with the ID exists.</returns>
    public Block? TranslateNamedID(String namedID)
    {
        return builder.BlocksByNamedID.GetValueOrDefault(namedID);
    }

    /// <summary>
    ///     Translate a named ID to the block that has that ID. If the ID is not valid, the error block is returned.
    /// </summary>
    /// <param name="namedID">The named ID of the block to return.</param>
    /// <returns>The block with the ID or error if the ID is not valid.</returns>
    public Block SafelyTranslateNamedID(String? namedID) // todo: maybe make this method use resource identifiers ?
    {
        if (String.IsNullOrEmpty(namedID))
        {
            LogUnknownNamedBlockID(logger, "", Core.Error.NamedID);

            return Core.Error;
        }

        if (builder.BlocksByNamedID.TryGetValue(namedID, out Block? block))
            return block;

        LogUnknownNamedBlockID(logger, namedID, Core.Error.NamedID);

        return Core.Error;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Blocks>();

    [LoggerMessage(EventId = LogID.Blocks + 0, Level = LogLevel.Warning, Message = "No Block with ID {ID} could be found, returning {Fallback} instead")]
    private static partial void LogUnknownBlockID(ILogger logger, UInt32 id, String fallback); // todo: remove if unused

    [LoggerMessage(EventId = LogID.Blocks + 1, Level = LogLevel.Warning, Message = "No Block with named ID {NamedID} could be found, returning {Fallback} instead")]
    private static partial void LogUnknownNamedBlockID(ILogger logger, String namedID, String fallback);

    [LoggerMessage(EventId = LogID.Blocks + 2, Level = LogLevel.Warning, Message = "No State with ID {ID} could be found, returning {Fallback} instead")]
    private static partial void LogUnknownStateID(ILogger logger, UInt32 id, String fallback);

    #endregion LOGGING
}
