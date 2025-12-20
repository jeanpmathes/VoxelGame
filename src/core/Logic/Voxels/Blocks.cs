// <copyright file="Blocks.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels.Contents;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using Environment = VoxelGame.Core.Logic.Voxels.Contents.Environment;

namespace VoxelGame.Core.Logic.Voxels;

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

    /// <inheritdoc cref="Contents.Core" />
    public Contents.Core Core { get; } = categories.Register(new Contents.Core(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Environment" />
    public Environment Environment { get; } = categories.Register(new Environment(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Woods" />
    public Woods Woods { get; } = categories.Register(new Woods(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Stones" />
    public Stones Stones { get; } = categories.Register(new Stones(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Metals" />
    public Metals Metals { get; } = categories.Register(new Metals(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Coals" />
    public Coals Coals { get; } = categories.Register(new Coals(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Organic" />
    public Organic Organic { get; } = categories.Register(new Organic(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Flowers" />
    public Flowers Flowers { get; } = categories.Register(new Flowers(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Crops" />
    public Crops Crops { get; } = categories.Register(new Crops(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Construction" />
    public Construction Construction { get; } = categories.Register(new Construction(builder.CreateScoped()));

    /// <inheritdoc cref="Contents.Fabricated" />
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
    /// <param name="dominantColorProvider">The dominant color provider to use for getting block colors.</param>
    /// <param name="modelProvider">The model provider to use for resolving block models.</param>
    /// <param name="visuals">The visual configuration to use.</param>
    /// <param name="context">The resource context in which loading is done.</param>
    /// <returns>All content defined in this class.</returns>
    public IEnumerable<IContent> Initialize(
        ITextureIndexProvider textureIndexProvider,
        IDominantColorProvider dominantColorProvider,
        IModelProvider modelProvider,
        VisualConfiguration visuals,
        IResourceContext context)
    {
        states.Clear();

        Validator validator = new(context);

        foreach (Block block in builder.BlocksWithCollisionOnID)
        {
            validator.SetScope(block);
            validator.ReportError($"Block with natural name '{block.Name}' ({block.BlockID}) is part of a named ID collision");
        }

        UInt32 offset = 0;

        foreach (Block block in builder.BlocksByID)
        {
            offset += block.Initialize(offset, validator);

            states.AddRange(block.States.AllStates);
        }

        BehaviorSystem<Block, BlockBehavior>.Bake(validator);

        foreach (Block block in builder.BlocksByID)
        {
            validator.SetScope(block);
            block.Activate(textureIndexProvider, dominantColorProvider, modelProvider, visuals, validator);
        }

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

        LogUnknownStateID(logger, id, Core.Error.ContentID);

        return Core.Error.States.Default;
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

        LogUnknownBlockID(logger, id, Core.Error.ContentID);

        return Core.Error;
    }

    /// <summary>
    ///     Translate a named ID to the block that has that ID.
    /// </summary>
    /// <param name="contentID">The content ID to translate.</param>
    /// <returns>The block, or null if no block with the ID exists.</returns>
    public Block? TranslateContentID(CID contentID)
    {
        return builder.BlocksByContentID.GetValueOrDefault(contentID);
    }

    /// <summary>
    ///     Translate a content ID to the block that has that ID. If the ID is not valid, the error block is returned.
    /// </summary>
    /// <param name="contentID">The content ID of the block to return.</param>
    /// <returns>The block with the ID or error if the ID is not valid.</returns>
    public Block SafelyTranslateContentID(CID? contentID)
    {
        if (contentID is not {} id)
        {
            LogUnknownNamedBlockID(logger, new CID(""), Core.Error.ContentID);

            return Core.Error;
        }

        if (builder.BlocksByContentID.TryGetValue(id, out Block? block))
            return block;

        LogUnknownNamedBlockID(logger, id, Core.Error.ContentID);

        return Core.Error;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Blocks>();

    [LoggerMessage(EventId = LogID.Blocks + 0, Level = LogLevel.Warning, Message = "No Block with ID {ID} could be found, returning {Fallback} instead")]
    private static partial void LogUnknownBlockID(ILogger logger, UInt32 id, CID fallback);

    [LoggerMessage(EventId = LogID.Blocks + 1, Level = LogLevel.Warning, Message = "No Block with named ID {ContentID} could be found, returning {Fallback} instead")]
    private static partial void LogUnknownNamedBlockID(ILogger logger, CID contentID, CID fallback);

    [LoggerMessage(EventId = LogID.Blocks + 2, Level = LogLevel.Warning, Message = "No State with ID {ID} could be found, returning {Fallback} instead")]
    private static partial void LogUnknownStateID(ILogger logger, UInt32 id, CID fallback);

    #endregion LOGGING
}
