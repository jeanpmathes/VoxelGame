// <copyright file="CrosshairDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Displays the crosshair for the player.
/// </summary>
public class CrosshairDisplay : ActorComponent, IConstructible<Player, Engine, CrosshairDisplay>
{
    private readonly Player player;
    private readonly Engine engine;
    
    private CrosshairDisplay(Player player, Engine engine) : base(player) 
    {
        this.player = player;
        this.engine = engine;
    }

    /// <inheritdoc />
    public static CrosshairDisplay Construct(Player input1, Engine input2)
    {
        return new CrosshairDisplay(input1, input2);
    }

    /// <inheritdoc />
    public override void OnActivate()
    {
        engine.CrosshairPipeline.IsEnabled = true;
    }
    
    /// <inheritdoc />
    public override void OnDeactivate()
    {
        engine.CrosshairPipeline.IsEnabled = false;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        engine.CrosshairPipeline.LogicUpdate();
    }
}
