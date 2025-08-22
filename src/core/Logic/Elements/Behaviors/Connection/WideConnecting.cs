// <copyright file="WideConnecting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
/// A thin block that connects to other blocks along its lateral sides.
/// </summary>
public class WideConnecting : BlockBehavior, IBehavior<WideConnecting, BlockBehavior, Block>
{
    private readonly Connecting connecting;
    
    /// <summary>
    /// The models used for the block.
    /// An optional straight extension can be provided, which is used in the case if and only if there are exactly two opposite connections - the post will not be used then.
    /// </summary>
    public (RID post, RID extension, RID? straight) Models { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Models"/> property.
    /// </summary>
    public Aspect<(RID post, RID extension, RID? straight), Block> ModelsInitializer { get; }
    
    /// <summary>
    /// Optional texture to override the texture of the provided models.
    /// </summary>
    public TID? TextureOverride { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="TextureOverride"/> property.
    /// </summary>
    public Aspect<TID?, Block> TextureOverrideInitializer { get; }
    
    private WideConnecting(Block subject) : base(subject)
    {
        connecting = subject.Require<Connecting>();
        subject.Require<Connectable>().StrengthInitializer.ContributeConstant(Connectable.Strengths.Wide);
        
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);

        ModelsInitializer = Aspect<(RID, RID, RID?), Block>.New<Exclusive<(RID, RID, RID?), Block>>(nameof(ModelsInitializer), this);
        TextureOverrideInitializer = Aspect<TID?, Block>.New<Exclusive<TID?, Block>>(nameof(TextureOverrideInitializer), this);
    }

    /// <inheritdoc/>
    public static WideConnecting Construct(Block input)
    {
        return new WideConnecting(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        Models = ModelsInitializer.GetValue(original: default, Subject);
        TextureOverride = TextureOverrideInitializer.GetValue(original: null, Subject);
    }
    
    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration _) = context;

        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);
        
        BlockModel post = blockModelProvider.GetModel(Models.post);
        BlockModel extension = blockModelProvider.GetModel(Models.extension);

        {
            if (TextureOverride is {} textureOverride)
            {
                post.OverwriteTexture(textureOverride);
                extension.OverwriteTexture(textureOverride);
            
                // todo: when doing caching on model provider, the returned model should be read only (interface)
                // todo: maybe doing the overrides as a parameter to the CreateMesh method would be better
            }
        }
        
        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) extensions =
            extension.CreateAllOrientations(rotateTopAndBottomTexture: false);

        List<BlockModel> models = new(capacity: 5);
        
        Boolean useStraightZ = north && south && !east && !west;
        Boolean useStraightX = !north && !south && east && west;

        if (Models.straight is {} straight && (useStraightX || useStraightZ))
        {
            BlockModel straightZ = blockModelProvider.GetModel(straight);
            
            if (TextureOverride is {} textureOverride)
            {
                straightZ.OverwriteTexture(textureOverride);
            }
            
            if (useStraightZ)
            {
                models.Add(straightZ);
            }
            else if (useStraightX)
            {
                BlockModel straightX = straightZ.Copy();
                straightX.RotateY(rotations: 1, rotateTopAndBottomTexture: false);
                
                models.Add(straightX);
            }
        }
        else
        {
            models.Add(post);
            
            if (north) models.Add(extensions.north);
            if (east) models.Add(extensions.east);
            if (south) models.Add(extensions.south);
            if (west) models.Add(extensions.west);
        }
        
        return BlockModel.GetCombinedMesh(textureIndexProvider, models.ToArray());
    }
}
