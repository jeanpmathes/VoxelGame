// <copyright file="Modelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// A <see cref="Complex"/> block which uses <see cref="BlockModel"/>s to define its mesh.
/// </summary>
public class Modelled : BlockBehavior, IBehavior<Modelled, BlockBehavior, Block>
{
    private Modelled(Block subject) : base(subject)
    {
        LayersInitializer = Aspect<IReadOnlyList<RID>, Block>.New<Exclusive<IReadOnlyList<RID>, Block>>(nameof(LayersInitializer), this);
        
        Selector = Aspect<Vector4i, State>.New<Exclusive<Vector4i, State>>(nameof(Selector), this); // todo: maybe do not use Vector4i but a custom struct and a custom strategy that allows to combine compatible selectors
        Model = Aspect<BlockModel, State>.New<Exclusive<BlockModel, State>>(nameof(Model), this);
        
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        
        subject.RequireIfPresent<CompositeModelled, Composite>();
        subject.RequireIfPresent<FourWayRotatableModelled, FourWayRotatable>();
    }

    /// <inheritdoc />
    public static Modelled Construct(Block input)
    {
        return new Modelled(input);
    }

    /// <summary>
    /// The resource IDs of the models that define the mesh of this block.
    /// </summary>
    public IReadOnlyList<RID> Layers { get; private set; } = [];
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Layers"/> property.
    /// </summary>
    public Aspect<IReadOnlyList<RID>, Block> LayersInitializer { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Layers = LayersInitializer.GetValue(new List<RID>(), Subject);
    }

    /// <summary>
    /// Selector to choose which layer and model part is used for a specific state of the block.
    /// </summary>
    public Aspect<Vector4i, State> Selector { get; }
    
    /// <summary>
    /// The actually used block model used for a given state of the block.
    /// </summary>
    public Aspect<BlockModel, State> Model { get; }
    
    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration _) = context;

        Vector4i selector = Selector.GetValue(Vector4i.Zero, state);
        // todo: create selector type with four bytes
        
        RID layer = Layers[selector.W]; // todo: handle out of bounds access
        
        // todo: add a new MID struct similar to TID, contains a RID and additional optional part selection
        // todo: the block model loader should go through all models and split them if needed, store all parts - so each model is split once and not on every call here

        BlockModel model = blockModelProvider.GetModel(layer); // todo: adapt to also take selector.Xyz

        model = Model.GetValue(model, state);
        
        return model.CreateMesh(textureIndexProvider); // todo: the block model loader should by default always split models greater then 1x1x1 into parts, for all models so the block specific model processing code should not be needed anymore
    }
}
