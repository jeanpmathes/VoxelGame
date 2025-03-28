// <copyright file="SubBiomeDefinitionProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

/// <summary>
///     Implementation of the <see cref="ISubBiomeDefinitionProvider" /> interface.
/// </summary>
#pragma warning disable CA1001 // SubBiomeDefinitionProvider is safe to not dispose.
#pragma warning disable S2931 // SubBiomeDefinition is safe to not dispose.
public class SubBiomeDefinitionProvider : ResourceProvider<SubBiomeDefinition>, ISubBiomeDefinitionProvider

{
    private SubBiomeDefinition? fallback;

    /// <inheritdoc />
    public SubBiomeDefinition GetSubBiomeDefinition(RID identifier)
    {
        return GetResource(identifier);
    }

    /// <inheritdoc />
    protected override void OnSetUp(IResourceContext context)
    {
        context.Require<Palette>(palette =>
        {
            CreateFallback(palette);

            return [];
        });
    }

    [MemberNotNull(nameof(fallback))]
    private void CreateFallback(Palette palette)
    {
        fallback = new SubBiomeDefinition("Fallback", palette)
        {
            Layers =
            [
                Layer.CreateStone(width: 1),
                Layer.CreateStonyDampen(maxWidth: 99)
            ]
        };
    }

    /// <inheritdoc />
    protected override SubBiomeDefinition CreateFallback()
    {
        if (fallback == null)
        {
            Context?.ReportWarning(this, "Fallback sub-biome definition creation failed, using alternative palette");

            CreateFallback(new Palette());
        }

        return fallback;
    }
}
