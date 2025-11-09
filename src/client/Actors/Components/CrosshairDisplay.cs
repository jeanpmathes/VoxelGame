// <copyright file="CrosshairDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Displays the crosshair for the player.
/// </summary>
public partial class CrosshairDisplay : ActorComponent
{
    private readonly Engine engine;

    [Constructible]
    private CrosshairDisplay(Player player, Engine engine) : base(player)
    {
        this.engine = engine;
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
