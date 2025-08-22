// <copyright file="ThinConnecting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
/// A thin block that connects to other blocks along its lateral sides.
/// </summary>
public class ThinConnecting : BlockBehavior, IBehavior<ThinConnecting, BlockBehavior, Block>
{
    private readonly Connecting connecting;
    
    /// <summary>
    /// The models used for the block.
    /// </summary>
    public (RID post, RID side, RID extension) Models { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Models"/> property.
    /// </summary>
    public Aspect<(RID post, RID side, RID extension), Block> ModelsInitializer { get; }
    
    private ThinConnecting(Block subject) : base(subject)
    {
        connecting = subject.Require<Connecting>();
        subject.Require<Connectable>().StrengthInitializer.ContributeConstant(Connectable.Strengths.Thin);
        
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);

        ModelsInitializer = Aspect<(RID, RID, RID), Block>.New<Exclusive<(RID, RID, RID), Block>>(nameof(ModelsInitializer), this);
    }

    /// <inheritdoc/>
    public static ThinConnecting Construct(Block input)
    {
        return new ThinConnecting(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        Models = ModelsInitializer.GetValue(original: default, Subject);
    }
    
    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration _) = context;

        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);
        
        BlockModel post = blockModelProvider.GetModel(Models.post);
        
        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) sides =
            blockModelProvider.GetModel(Models.side).CreateAllOrientations(rotateTopAndBottomTexture: false);

        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions =
            blockModelProvider.GetModel(Models.extension).CreateAllOrientations(rotateTopAndBottomTexture: false);
        
        // todo: why no locking here? maybe do the locking in the model provider, or remove it completely?

        return BlockModel.GetCombinedMesh(textureIndexProvider,
            post,
            north ? extensions.north : sides.north,
            east ? extensions.east : sides.east,
            south ? extensions.south : sides.south,
            west ? extensions.west : sides.west);
    }
    
    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        List<BoundingVolume> connectors = new(capacity: 4);
        
        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);

        if (north)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.21875f),
                    new Vector3d(x: 0.0625f, y: 0.5f, z: 0.21875f)));

        if (east)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.78125f, y: 0.5f, z: 0.5f),
                    new Vector3d(x: 0.21875f, y: 0.5f, z: 0.0625f)));

        if (south)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.5f, y: 0.5f, z: 0.78125f),
                    new Vector3d(x: 0.0625f, y: 0.5f, z: 0.21875f)));

        if (west)
            connectors.Add(
                new BoundingVolume(
                    new Vector3d(x: 0.21875f, y: 0.5f, z: 0.5f),
                    new Vector3d(x: 0.21875f, y: 0.5f, z: 0.0625f)));

        return new BoundingVolume(
            new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3d(x: 0.0625f, y: 0.5f, z: 0.0625f),
            connectors.ToArray());
    }
}
