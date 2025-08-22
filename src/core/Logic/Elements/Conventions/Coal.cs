// <copyright file="Coal.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
/// A coal type, as defined by the <see cref="CoalConvention"/>.
/// </summary>
public class Coal(String namedID, BlockBuilder builder) : Convention<Coal>(namedID, builder)
{
    /// <summary>
    /// The block that represents this coal type.
    /// </summary>
    public required Block Block { get; init; }
}

/// <summary>
/// A convention for coal types.
/// </summary>
public static class CoalConvention
{
    /// <summary>
    /// Builds a new coal type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="name">The name of the coal, used for display purposes.</param>
    /// <param name="namedID">The named ID of the coal, used to create the block IDs.</param>
    /// <returns>The created coal type.</returns>
    public static Coal BuildCoal(this BlockBuilder b, String name, String namedID)
    {
        return b.BuildConvention<Coal>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            return new Coal(namedID, builder)
            {
                Block = builder
                    .BuildSimpleBlock(name, namedID)
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block(texture)))
                    .Complete()
            };
        });
    }
}
