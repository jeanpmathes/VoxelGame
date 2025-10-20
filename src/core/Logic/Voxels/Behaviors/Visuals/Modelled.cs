// <copyright file="Modelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     A <see cref="Complex" /> block which uses <see cref="VoxelGame.Core.Visuals.Model" />s to define its mesh.
/// </summary>
public partial class Modelled : BlockBehavior, IBehavior<Modelled, BlockBehavior, Block>
{
    [Constructible]
    private Modelled(Block subject) : base(subject)
    {
        Selector = Aspect<Selector, State>.New<Chaining<Selector, State>>(nameof(Selector), this);
        Model = Aspect<Model, State>.New<Exclusive<Model, State>>(nameof(Model), this);

        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);

        subject.RequireIfPresent<CompositeModelled, Composite>();
        subject.RequireIfPresent<RotatableModelled, Rotatable>();
    }

    /// <summary>
    ///     The resource IDs of the models that define the mesh of this block.
    /// </summary>
    public ResolvedProperty<IReadOnlyList<RID>> Layers { get; } = ResolvedProperty<IReadOnlyList<RID>>.New<Exclusive<IReadOnlyList<RID>, Void>>(nameof(Layers), []);

    /// <summary>
    ///     Selector to choose which layer and model part is used for a specific state of the block.
    /// </summary>
    public Aspect<Selector, State> Selector { get; }

    /// <summary>
    ///     The actually used model used for a given state of the block.
    /// </summary>
    public Aspect<Model, State> Model { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Layers.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Layers.Get().Count == 0)
            validator.ReportWarning("No layers defined for modelled block");

        if (Layers.Get().Count > Visuals.Selector.MaxLayerCount)
            validator.ReportWarning($"Too many layers defined for modelled block (max {Visuals.Selector.MaxLayerCount}, got {Layers.Get().Count})");

        Layers.Override(Layers.Get().Take(Visuals.Selector.MaxLayerCount).ToArray());
    }

    private Mesh GetMesh(Mesh original, MeshContext context)
    {
        State state = context.State;

        Selector selector = Selector.GetValue(original: default, state);

        IReadOnlyList<RID> layers = Layers.Get();

        if (layers.Count == 0)
        {
            // No layers defined; fallback to original mesh, see validation.
            
            return original;
        }

        Int32 layerIndex = selector.Layer;

        if (layerIndex >= layers.Count)
        {
            const Int32 fallbackIndex = 0;
            
            // Note that GetMesh is called during initialization as all meshes are precomputed, so doing validation is fine.
            context.Validator.ReportWarning($"Selected layer {layerIndex} out of range for modelled block (max {layers.Count - 1}), using layer {fallbackIndex} instead");

            layerIndex = fallbackIndex;
        }

        RID layer = layers[layerIndex];

        Model model = context.ModelProvider.GetModel(layer, selector.Part);

        model = Model.GetValue(model, state);

        return model.CreateMesh(context.TextureIndexProvider, Subject.Get<TextureOverride>()?.Textures.Get());
    }
}

/// <summary>
///     Serves to select a specific part of a model and the layer to use.
/// </summary>
public readonly struct Selector(Byte x, Byte y, Byte z, Byte layer)
{
    /// <summary>
    ///     The maximum number of layers that can be used.
    /// </summary>
    public const Int32 MaxLayerCount = Byte.MaxValue;

    /// <summary>
    ///     Get the selected layer.
    /// </summary>
    public Int32 Layer => layer;

    /// <summary>
    ///     Get the selected part coordinates.
    /// </summary>
    public Vector3i Part => new(x, y, z);

    /// <summary>
    ///     Get a modified copy of this selector with the given layer.
    /// </summary>
    public Selector WithLayer(Byte newLayer)
    {
        return new Selector(x, y, z, newLayer);
    }

    /// <inheritdoc cref="WithLayer(Byte)" />
    public Selector WithLayer(Int32 newLayer)
    {
        return new Selector(x, y, z, (Byte) newLayer);
    }

    /// <summary>
    ///     Get a modified copy of this selector with the given part.
    /// </summary>
    public Selector WithPart(Vector3i newPart)
    {
        return new Selector((Byte) newPart.X, (Byte) newPart.Y, (Byte) newPart.Z, layer);
    }
}
