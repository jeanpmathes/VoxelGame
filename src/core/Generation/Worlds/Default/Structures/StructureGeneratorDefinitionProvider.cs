﻿// <copyright file="StructureGeneratorDefinitionProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default.Structures;

/// <summary>
/// Implementation of <see cref="IStructureGeneratorDefinitionProvider"/>.
/// </summary>
public class StructureGeneratorDefinitionProvider : ResourceProvider<StructureGeneratorDefinition>, IStructureGeneratorDefinitionProvider
{
    private static readonly StructureGeneratorDefinition fallback
        = new(
            "Fallback",
            StructureGeneratorDefinition.Kind.Surface,
            StaticStructure.CreateFallback(),
            rarity: Single.PositiveInfinity,
            offset: Vector3i.Zero);

    /// <summary>
    /// Create a new structure generator definition provider.
    /// </summary>
    public StructureGeneratorDefinitionProvider() : base(() => fallback, structure => structure) {}

    /// <inheritdoc />
    public StructureGeneratorDefinition GetStructure(RID identifier)
    {
        return GetResource(identifier);
    }
}
