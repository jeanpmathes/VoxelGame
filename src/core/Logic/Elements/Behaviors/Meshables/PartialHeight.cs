// <copyright file="PartialHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.PartialHeight" />.
/// </summary>
public class PartialHeight : BlockBehavior, IBehavior<PartialHeight, BlockBehavior, Block>, IMeshable
{
    private readonly Meshed meshed;
    private readonly CubeTextured textured;

    private PartialHeight(Block subject) : base(subject)
    {
        meshed = subject.Require<Meshed>();
        textured = subject.Require<CubeTextured>();
    }
    
    /// <inheritdoc />
    public Meshable Type => Meshable.PartialHeight;
    
    /// <inheritdoc />
    public static PartialHeight Construct(Block input)
    {
        return new PartialHeight(input);
    }
    
    /// <summary>
    ///     Get the mesh data for a given side and state of the block.
    /// </summary>
    /// <param name="state">The state to get the mesh data for.</param>
    /// <param name="side">The side of the block to get the mesh data for.</param>
    /// <param name="textureIndexProvider">Provides texture indices for given texture IDs.</param>
    /// <returns>The mesh data for the given side and state.</returns>
    public MeshData GetMeshData(State state, Side side, ITextureIndexProvider textureIndexProvider)
    {
        ColorS tint = meshed.Tint.GetValue(ColorS.None, state);
        Boolean isAnimated = meshed.IsAnimated.GetValue(original: false, state);

        Int32 textureIndex = textured.GetTextureIndex(state, side, textureIndexProvider, isBlock: true);

        return new MeshData(textureIndex, tint, isAnimated && textureIndex != ITextureIndexProvider.MissingTextureIndex);
    }
    
    /// <summary>
    ///     Get the size of a face with a given height, in world units.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The size of the face.</returns>
    public static Single GetSize(Int32 height)
    {
        return (height + 1) / (Single) (Height.PartialHeight.MaximumHeight + 1); 
    }

    /// <summary>
    ///     Get the gap of a face, which is the space between the end of the face and the end of the block, in world units.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The gap of the face.</returns>
    public static Single GetGap(Int32 height)
    {
        return 1 - GetSize(height);
    }
    
    /// <summary>
    ///     Get the bounds of a face with a given height.
    ///     The bounds can be used as texture coordinates.
    /// </summary>
    /// <param name="height">The height of the face.</param>
    /// <returns>The bounds of the face.</returns>
    public static (Vector2 min, Vector2 max) GetBounds(Int32 height)
    {
        Single size = GetSize(height);

        return (new Vector2(x: 0, y: 0), new Vector2(x: 1, size));
    }

    /// <summary>
    ///     The mesh data for a partial height block.
    /// </summary>
    /// <param name="TextureIndex">The index of the texture to use.</param>
    /// <param name="Tint">The tint color to apply to the mesh.</param>
    /// <param name="IsAnimated">Whether the texture is animated.</param>
    public readonly record struct MeshData(Int32 TextureIndex, ColorS Tint, Boolean IsAnimated)
    {
        /// <summary>
        /// Whether the texture is rotated.
        /// </summary>
        public Boolean IsTextureRotated => false;
    }
}
