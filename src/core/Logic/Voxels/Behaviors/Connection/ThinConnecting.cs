// <copyright file="ThinConnecting.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

/// <summary>
///     A thin block that connects to other blocks along its lateral sides.
/// </summary>
public partial class ThinConnecting : BlockBehavior, IBehavior<ThinConnecting, BlockBehavior, Block>
{
    private readonly Connecting connecting;

    [Constructible]
    private ThinConnecting(Block subject) : base(subject)
    {
        connecting = subject.Require<Connecting>();
        subject.Require<Connectable>().Strength.Initializer.ContributeConstant(Connectable.Strengths.Thin);

        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
    }

    /// <summary>
    ///     The models used for the block.
    /// </summary>
    public ResolvedProperty<(RID post, RID side, RID extension)> Models { get; } = ResolvedProperty<(RID, RID, RID)>.New<Exclusive<(RID, RID, RID), Void>>(nameof(Models));

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Models.Initialize(this);
    }

    private Mesh GetMesh(Mesh original, MeshContext context)
    {
        (Boolean north, Boolean east, Boolean south, Boolean west) = connecting.GetConnections(context.State);

        Model post = context.ModelProvider.GetModel(Models.Get().post);

        (Model north, Model east, Model south, Model west) sides = Core.Visuals.Models.CreateModelsForAllOrientations(
            context.ModelProvider.GetModel(Models.Get().side),
            Model.TransformationMode.Reshape);

        (Model north, Model east, Model south, Model west) extensions = Core.Visuals.Models.CreateModelsForAllOrientations(
            context.ModelProvider.GetModel(Models.Get().extension),
            Model.TransformationMode.Reshape);

        return Model.Combine(post,
                north ? extensions.north : sides.north,
                east ? extensions.east : sides.east,
                south ? extensions.south : sides.south,
                west ? extensions.west : sides.west)
            .CreateMesh(context.TextureIndexProvider);
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
