// <copyright file="Foliage.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Meshables;

/// <summary>
///     Corresponds to <see cref="Meshable.Foliage" />.
/// </summary>
public class Foliage : BlockBehavior, IBehavior<Foliage, BlockBehavior, Block>, IMeshable
{
    /// <summary>
    ///     Defines the layout of the foliage mesh.
    /// </summary>
    public enum LayoutType
    {
        /// <summary>
        ///     The foliage uses a two planes forming a cross.
        /// </summary>
        Cross,

        /// <summary>
        ///     The foliage uses two times two parallel planes.
        ///     Used primarily for double block crops.
        /// </summary>
        Crop,

        /// <summary>
        ///     The foliage uses two times three parallel planes, essentially a denser version of <see cref="Crop" />.
        ///     Used primarily for single block crops.
        /// </summary>
        DenseCrop
    }

    /// <summary>
    ///     Foliage blocks can occupy one or two block positions.
    ///     This enum defines which part of a plant the meshed block represents.
    /// </summary>
    public enum PartType
    {
        /// <summary>
        ///     The block occupies a single position.
        /// </summary>
        Single,

        /// <summary>
        ///     The block occupies the lower part of a double plant which occupies two positions.
        /// </summary>
        DoubleLower,

        /// <summary>
        ///     The block occupies the upper part of a double plant which occupies two positions.
        /// </summary>
        DoubleUpper
    }

    private readonly Meshed meshed;
    private readonly SingleTextured textured;

    private Foliage(Block subject) : base(subject)
    {
        meshed = subject.Require<Meshed>();
        textured = subject.Require<SingleTextured>();

        LayoutInitializer = Aspect<LayoutType, Block>.New<Exclusive<LayoutType, Block>>(nameof(LayoutInitializer), this);
        Part = Aspect<PartType, State>.New<Exclusive<PartType, State>>(nameof(Part), this);
        IsLowered = Aspect<Boolean, State>.New<Exclusive<Boolean, State>>(nameof(IsLowered), this);
    }

    /// <summary>
    ///     The mesh layout of the foliage.
    /// </summary>
    public LayoutType Layout { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="Layout" /> property.
    /// </summary>
    public Aspect<LayoutType, Block> LayoutInitializer { get; } // todo: add to code gen note that initializer aspects could also be generated 

    /// <summary>
    ///     The part of the foliage a block in a certain state represents.
    /// </summary>
    public Aspect<PartType, State> Part { get; }

    /// <summary>
    ///     Whether the block is lowered towards the ground, so it aligns with a partial ground that is lowered by one partial
    ///     height unit.
    ///     See <see cref="Environment.Farmland" /> as an example of a block that allows plant growth and is lowered, not
    ///     filling a full block position.
    /// </summary>
    public Aspect<Boolean, State> IsLowered { get; }

    /// <inheritdoc />
    public static Foliage Construct(Block input)
    {
        return new Foliage(input);
    }

    /// <inheritdoc />
    public Meshable Type => Meshable.Foliage;

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsOpaque.ContributeConstant(value: false);

        Layout = LayoutInitializer.GetValue(LayoutType.Cross, Subject);
    }

    /// <summary>
    ///     Get the mesh data for a given state of the block.
    /// </summary>
    /// <param name="state">The state to get the mesh data for.</param>
    /// <param name="textureIndexProvider">Provides texture indices for given texture IDs.</param>
    /// <param name="visuals">The visual configuration to use.</param>
    /// <returns>The mesh data for the given state.</returns>
    public MeshData GetMeshData(State state, ITextureIndexProvider textureIndexProvider, VisualConfiguration visuals)
    {
        PartType part = Part.GetValue(PartType.Single, state);
        Boolean isLowered = IsLowered.GetValue(original: false, state);
        ColorS tint = meshed.Tint.GetValue(ColorS.None, state);
        Boolean isAnimated = meshed.IsAnimated.GetValue(original: false, state);

        Int32 textureIndex = textured.GetTextureIndex(state, textureIndexProvider, isBlock: true);

        BlockMesh mesh = Layout switch
        {
            LayoutType.Cross => BlockMeshes.CreateCrossPlantMesh(visuals.FoliageQuality, textureIndex, isLowered),
            LayoutType.Crop => BlockMeshes.CreateCropPlantMesh(visuals.FoliageQuality, createMiddlePiece: false, textureIndex, isLowered),
            LayoutType.DenseCrop => BlockMeshes.CreateCropPlantMesh(visuals.FoliageQuality, createMiddlePiece: true, textureIndex, isLowered),
            _ => throw Exceptions.UnsupportedEnumValue(Layout)
        };

        BlockMesh.Quad[] quads = mesh.GetMeshData(out UInt32 quadCount);

        return new MeshData(quads, quadCount, tint, part, isAnimated && textureIndex != ITextureIndexProvider.MissingTextureIndex);
    }

    /// <summary>
    ///     The mesh data for a foliage block.
    /// </summary>
    /// <param name="Quads">The quads that make up the mesh.</param>
    /// <param name="QuadCount">Number of quads in the mesh.</param>
    /// <param name="Tint">The tint color to apply to the mesh.</param>
    /// <param name="IsAnimated">Whether the texture is animated.</param>
    /// <param name="Part">What part of a foliage block this mesh represents.</param>
    public readonly record struct MeshData(BlockMesh.Quad[] Quads, UInt32 QuadCount, ColorS Tint, PartType Part, Boolean IsAnimated);
}
