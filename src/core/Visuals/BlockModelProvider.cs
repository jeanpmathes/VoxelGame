// <copyright file="BlockModelProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides loaded block models.
/// </summary>
public partial class BlockModelProvider : ResourceProvider<BlockModel>, IBlockModelProvider
{
    private readonly Dictionary<RID, BlockModel[,,]> parts = [];

    /// <inheritdoc />
    public BlockModel GetModel(RID identifier, Vector3i? part = null)
    {
        if (part is not {} position)
            return GetResource(identifier);

        BlockModel[,,]? modelParts = parts.GetValueOrDefault(identifier);

        if (modelParts != null &&
            position.X >= 0 && position.X < modelParts.GetLength(0) &&
            position.Y >= 0 && position.Y < modelParts.GetLength(1) &&
            position.Z >= 0 && position.Z < modelParts.GetLength(2))
            return modelParts[position.X, position.Y, position.Z];

        if (part != (0, 0, 0))
            LogPartDoesNotExist(logger, identifier, position);

        return GetResource(identifier);

    }

    /// <inheritdoc />
    protected override BlockModel CreateFallback()
    {
        return BlockModels.CreateFallback();
    }

    /// <inheritdoc />
    protected override BlockModel Copy(BlockModel resource)
    {
        return resource.Copy();
    }

    /// <inheritdoc />
    protected override void OnSetUp(IResourceContext context)
    {
        parts.Clear();

        foreach ((RID id, BlockModel original) in GetAllResources())
        {
            Box3d bounds = original.GetBounds();

            if (bounds.Size is {X: <= 1, Y: <= 1, Z: <= 1})
                continue;

            parts.Add(id, original.PartitionByBlocks());
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<BlockModelProvider>();

    [LoggerMessage(EventId = LogID.GroupProvider + 0, Level = LogLevel.Warning, Message = "Model {Model} does not have a part {Part}, returning full model instead")]
    private static partial void LogPartDoesNotExist(ILogger logger, RID model, Vector3i part);

    #endregion LOGGING
}
