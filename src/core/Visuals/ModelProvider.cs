// <copyright file="ModelProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides loaded models.
/// </summary>
public partial class ModelProvider : ResourceProvider<Model>, IModelProvider
{
    private readonly Dictionary<RID, Model[,,]> parts = [];

    /// <inheritdoc />
    public Model GetModel(RID identifier, Vector3i? part = null)
    {
        if (part is not {} position)
            return GetResource(identifier);

        Model[,,]? modelParts = parts.GetValueOrDefault(identifier);

        if (modelParts != null && IsPositionInBounds(position, modelParts))
            return modelParts[position.X, position.Y, position.Z];

        if (part != (0, 0, 0))
            LogPartDoesNotExist(logger, identifier, position);

        return GetResource(identifier);
    }

    private static Boolean IsPositionInBounds(Vector3i position, Model[,,] parts)
    {
        if (position is {X: < 0, Y: < 0, Z: < 0})
            return false;

        return position.X < parts.GetLength(dimension: 0) && position.Y < parts.GetLength(dimension: 1) && position.Z < parts.GetLength(dimension: 2);
    }

    /// <inheritdoc />
    protected override Model CreateFallback()
    {
        return Model.CreateFallback();
    }

    /// <inheritdoc />
    protected override Model Copy(Model resource)
    {
        return resource.Copy();
    }

    /// <inheritdoc />
    protected override void OnSetUp(IResourceContext context)
    {
        parts.Clear();

        foreach ((RID id, Model original) in Resources)
        {
            Box3d bounds = original.ComputeBounds();

            if (bounds.Size is {X: <= 1, Y: <= 1, Z: <= 1})
                continue;

            parts.Add(id, original.PartitionByBlocks());
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ModelProvider>();

    [LoggerMessage(EventId = LogID.GroupProvider + 0, Level = LogLevel.Warning, Message = "Model {Model} does not have a part {Part}, returning full model instead")]
    private static partial void LogPartDoesNotExist(ILogger logger, RID model, Vector3i part);

    #endregion LOGGING
}
