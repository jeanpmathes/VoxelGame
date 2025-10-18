// <copyright file="WideConnecting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

/// <summary>
///     A thin block that connects to other blocks along its lateral sides.
/// </summary>
public partial class WideConnecting : BlockBehavior, IBehavior<WideConnecting, BlockBehavior, Block>
{
    private readonly Connecting connecting;

    [Constructible]
    private WideConnecting(Block subject) : base(subject)
    {
        connecting = subject.Require<Connecting>();
        subject.Require<Connectable>().Strength.Initializer.ContributeConstant(Connectable.Strengths.Wide);

        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
    }

    /// <summary>
    ///     The models used for the block.
    ///     An optional straight extension can be provided, which is used in the case if and only if there are exactly two
    ///     opposite connections - the post will not be used then.
    /// </summary>
    public ResolvedProperty<(RID post, RID extension, RID? straight)> Models { get; } = ResolvedProperty<(RID, RID, RID?)>.New<Exclusive<(RID, RID, RID?), Void>>(nameof(Models));

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Models.Initialize(this);
    }

    private Mesh GetMesh(Mesh original, MeshContext context)
    {
        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(context.State);

        Model post = context.ModelProvider.GetModel(Models.Get().post);
        Model extension = context.ModelProvider.GetModel(Models.Get().extension);
        
        (Model north, Model east, Model south, Model west) extensions = VoxelGame.Core.Visuals.Models.CreateModelsForAllOrientations(extension, Model.TransformationMode.Reshape);

        List<Model> models = new(capacity: 5);

        Boolean useStraightZ = north && south && !east && !west;
        Boolean useStraightX = !north && !south && east && west;

        if (Models.Get().straight is {} straight && (useStraightX || useStraightZ))
        {
            Model straightZ = context.ModelProvider.GetModel(straight);

            if (useStraightZ)
            {
                models.Add(straightZ);
            }
            else if (useStraightX)
            {
                Model straightX = straightZ.CreateModelForSide(Side.Left, Model.TransformationMode.Reshape);
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

        return Model.Combine(models).CreateMesh(context.TextureIndexProvider, Subject.Get<TextureOverride>()?.Textures.Get());
    }
}
