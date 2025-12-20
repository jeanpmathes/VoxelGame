// <copyright file="ModelProvider.cs" company="VoxelGame">
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
