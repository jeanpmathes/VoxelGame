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
///     A thin block that connects to other blocks along its lateral sides.
/// </summary>
public class WideConnecting : BlockBehavior, IBehavior<WideConnecting, BlockBehavior, Block>
{
    private readonly Connecting connecting;

    private WideConnecting(Block subject) : base(subject)
    {
        connecting = subject.Require<Connecting>();
        subject.Require<Connectable>().StrengthInitializer.ContributeConstant(Connectable.Strengths.Wide);

        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);

        ModelsInitializer = Aspect<(RID, RID, RID?), Block>.New<Exclusive<(RID, RID, RID?), Block>>(nameof(ModelsInitializer), this);
    }

    /// <summary>
    ///     The models used for the block.
    ///     An optional straight extension can be provided, which is used in the case if and only if there are exactly two
    ///     opposite connections - the post will not be used then.
    /// </summary>
    public (RID post, RID extension, RID? straight) Models { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="Models" /> property.
    /// </summary>
    public Aspect<(RID post, RID extension, RID? straight), Block> ModelsInitializer { get; }

    /// <inheritdoc />
    public static WideConnecting Construct(Block input)
    {
        return new WideConnecting(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Models = ModelsInitializer.GetValue(original: default, Subject);
    }

    private Mesh GetMesh(Mesh original, (State state, ITextureIndexProvider textureIndexProvider, IModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IModelProvider blockModelProvider, VisualConfiguration _) = context;

        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(state);

        Model post = blockModelProvider.GetModel(Models.post);
        Model extension = blockModelProvider.GetModel(Models.extension);

        // todo: when doing caching on model provider, the returned model should be read only (interface)

        (Model north, Model east, Model south, Model west) extensions =
            extension.CreateAllOrientations(rotateTopAndBottomTexture: false);

        List<Model> models = new(capacity: 5);

        Boolean useStraightZ = north && south && !east && !west;
        Boolean useStraightX = !north && !south && east && west;

        if (Models.straight is {} straight && (useStraightX || useStraightZ))
        {
            Model straightZ = blockModelProvider.GetModel(straight);

            if (useStraightZ)
            {
                models.Add(straightZ);
            }
            else if (useStraightX)
            {
                Model straightX = straightZ.Copy();
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

        return Model.GetCombinedMesh(textureIndexProvider, models.ToArray()); // todo: use Subject.Get<TextureOverride>()?.Textures
    }
}
