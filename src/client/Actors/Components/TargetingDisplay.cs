﻿// <copyright file="TargetingDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Physics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Displays the information in <see cref="Targeting"/> in the world.
/// </summary>
public class TargetingDisplay : ActorComponent, IConstructible<Player, Engine, TargetingDisplay>
{
    private readonly Player player;
    
    private readonly TargetingBoxEffect effect;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;
    
    private TargetingDisplay(Player player, Engine engine) : base(player)
    {
        this.player = player;
        
        effect = engine.TargetingBoxPipeline.CreateEffect();

        targeting = player.GetRequiredComponent<Targeting>();
    }

    /// <inheritdoc />
    public static TargetingDisplay Construct(Player input1, Engine input2)
    {
        return new TargetingDisplay(input1, input2);
    }

    /// <inheritdoc />
    public override void OnActivate()
    {
        SetTarget(world: null, instance: null, position: null);
    }

    /// <inheritdoc />
    public override void OnDeactivate()
    {
        effect.IsEnabled = false;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        SetTarget(player.World, targeting.Block, targeting.Position);
        
        effect.LogicUpdate();
    }

    private void SetTarget(World? world, BlockInstance? instance, Vector3i? position)
    {
        BoxCollider? collider = null;

        if (world != null && instance is {Block: {} block} && position != null)
        {
            Boolean visualized = !block.IsReplaceable;

            if (Core.App.Application.Instance.IsDebug)
                visualized |= block != Blocks.Instance.Air;

            collider = visualized ? block.GetCollider(world, position.Value) : null;
        }

        if (collider != null)
            effect.SetBox(collider.Value);

        effect.IsEnabled = collider != null;
    }
    
    #region DISPOSABLE 
    
    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);
        
        if (disposing)
        {
            effect.Dispose();
        }
    }
    
    #endregion DISPOSABLE
}
