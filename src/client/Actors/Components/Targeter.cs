// <copyright file="Targeter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Physics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Implements targeting functionality based on an actor's head position and orientation.
///     If an actor does not have a head, no targeting will be performed.
/// </summary>
public class Targeter : ActorComponent, IConstructible<Actor, Targeter>
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;

    private Targeter(Actor subject) : base(subject)
    {
        targeting = Subject.GetRequiredComponent<Targeting>();
    }

    /// <inheritdoc />
    public static Targeter Construct(Actor input)
    {
        return new Targeter(input);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        Update();
    }

    /// <summary>
    ///     Update the targeting. This method will be called every logic update, but can be called manually if needed.
    /// </summary>
    public void Update()
    {
        World world = Subject.World;
        IOrientable? start = Subject.Head;

        if (start != null)
        {
            var ray = new Ray(start.Position, start.Forward, length: 6f);
            (Vector3i, Side)? hit = Raycast.CastBlockRay(world, ray);

            if (hit is var (hitPosition, hitSide) && world.GetContent(hitPosition) is var (block, fluid))
            {
                targeting.HasTarget = true;
                targeting.Position = hitPosition;
                targeting.Side = hitSide;
                targeting.Block = block;
                targeting.Fluid = fluid;

                return;
            }
        }

        targeting.HasTarget = false;
        targeting.Position = null;
        targeting.Side = Side.All;
        targeting.Block = null;
        targeting.Fluid = null;
    }
}
