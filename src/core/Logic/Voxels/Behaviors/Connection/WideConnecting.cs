// <copyright file="WideConnecting.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
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
        (Boolean north, Boolean east, Boolean south, Boolean west) connections = connecting.GetConnections(context.State);

        Model post = context.ModelProvider.GetModel(Models.Get().post);
        Model extension = context.ModelProvider.GetModel(Models.Get().extension);

        (Model north, Model east, Model south, Model west) extensions = Core.Visuals.Models.CreateModelsForAllOrientations(extension, Model.TransformationMode.Reshape);

        List<Model> models = new(capacity: 5);

        Boolean useStraightX = IsStraightOnX(connections);
        Boolean useStraightZ = IsStraightOnZ(connections);

        if (Models.Get().straight is {} straight && (useStraightX || useStraightZ))
        {
            Model straightModel = context.ModelProvider.GetModel(straight);

            if (useStraightX) straightModel = straightModel.CreateModelForSide(Side.Left, Model.TransformationMode.Reshape);

            models.Add(straightModel);
        }
        else
        {
            models.Add(post);

            AddExtensionsBasedOnConnections(models, connections, extensions);
        }

        return Model.Combine(models).CreateMesh(context.TextureIndexProvider, Subject.Get<TextureOverride>()?.Textures.Get());
    }

    private static Boolean IsStraightOnX((Boolean north, Boolean east, Boolean south, Boolean west) connections)
    {
        return connections is {north: false, east: true, south: false, west: true};
    }

    private static Boolean IsStraightOnZ((Boolean north, Boolean east, Boolean south, Boolean west) connections)
    {
        return connections is {north: true, east: false, south: true, west: false};
    }

    private static void AddExtensionsBasedOnConnections(
        List<Model> models,
        (Boolean north, Boolean east, Boolean south, Boolean west) connections,
        (Model north, Model east, Model south, Model west) extensions)
    {
        if (connections.north) models.Add(extensions.north);
        if (connections.east) models.Add(extensions.east);
        if (connections.south) models.Add(extensions.south);
        if (connections.west) models.Add(extensions.west);
    }
}
