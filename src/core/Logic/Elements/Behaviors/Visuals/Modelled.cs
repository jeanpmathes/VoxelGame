// <copyright file="Modelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
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
        TextureOverrideInitializer = Aspect<TID?, Block>.New<Exclusive<TID?, Block>>(nameof(TextureOverrideInitializer), this);

        Selector = Aspect<Selector, State>.New<Chaining<Selector, State>>(nameof(Selector), this);
        Model = Aspect<BlockModel, State>.New<Exclusive<BlockModel, State>>(nameof(Model), this);
        
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        
        subject.RequireIfPresent<CompositeModelled, Composite>();
        subject.RequireIfPresent<FourWayRotatableModelled, LateralRotatable>();
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
    
    /// <summary>
    /// Optional texture to override the texture of the provided models.
    /// </summary>
    public TID? TextureOverride { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="TextureOverride"/> property.
    /// </summary>
    public Aspect<TID?, Block> TextureOverrideInitializer { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Layers = LayersInitializer.GetValue(new List<RID>(), Subject);
        TextureOverride = TextureOverrideInitializer.GetValue(original: null, Subject);
    }

    /// <inheritdoc />
    protected override void OnValidate(IResourceContext context)
    {
        if (Layers.Count == 0)
            context.ReportWarning(this, "No layers defined for modelled block");
        
        if (Layers.Count > Visuals.Selector.MaxLayerCount)
            context.ReportWarning(this, $"Too many layers defined for modelled block (max {Visuals.Selector.MaxLayerCount}, got {Layers.Count})");
        
        Layers = Layers.Take(Visuals.Selector.MaxLayerCount).ToArray();
    }

    /// <summary>
    /// Selector to choose which layer and model part is used for a specific state of the block.
    /// </summary>
    public Aspect<Selector, State> Selector { get; }
    
    /// <summary>
    /// The actually used block model used for a given state of the block.
    /// </summary>
    public Aspect<BlockModel, State> Model { get; }
    
    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration _) = context;

        Selector selector = Selector.GetValue(original: default, state);
        
        RID layer = Layers[selector.Layer]; // todo: handle out of bounds access
        
        BlockModel model = blockModelProvider.GetModel(layer, selector.Part);

        if (TextureOverride is {} textureOverride)
        {
            model.OverwriteTexture(textureOverride);      
        }
        
        model = Model.GetValue(model, state);
        
        return model.CreateMesh(textureIndexProvider);
    }
}

/// <summary>
/// Serves to select a specific part of a model and the layer to use.
/// </summary>
public readonly struct Selector(Byte x, Byte y, Byte z, Byte layer)
{
    /// <summary>
    /// The maximum number of layers that can be used.
    /// </summary>
    public const Int32 MaxLayerCount = Byte.MaxValue;

    /// <summary>
    /// Get the selected layer.
    /// </summary>
    public Int32 Layer => layer;
    
    /// <summary>
    /// Get the selected part coordinates.
    /// </summary>
    public Vector3i Part => new(x, y, z);
    
    /// <summary>
    /// Get a modified copy of this selector with the given layer.
    /// </summary>
    public Selector WithLayer(Byte newLayer) => new(x, y, z, newLayer);
    
    /// <inheritdoc cref="WithLayer(Byte)"/>
    public Selector WithLayer(Int32 newLayer)
    {
        return new Selector(x, y, z, (Byte) newLayer);
    }

    /// <summary>
    /// Get a modified copy of this selector with the given part.
    /// </summary>
    public Selector WithPart(Vector3i newPart) => new((Byte) newPart.X, (Byte) newPart.Y, (Byte) newPart.Z, layer);
}
