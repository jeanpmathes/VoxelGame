﻿// <copyright file="PlayerMovement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Implements the movement for the player.
/// </summary>
public class PlayerMovement : ActorComponent, IConstructible<Player, PlayerMovement>
{
    private readonly Player player;
    
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlayerInput input;

    private MovementStrategy strategy;

    private Targeter? targeter;
    
    private PlayerMovement(Player player) : base(player)
    {
        this.player = player;
        
        input = player.GetRequiredComponent<PlayerInput, Player>();
        
        strategy = new DefaultMovement(player, input, flyingSpeed: 1.0);
    }

    /// <inheritdoc />
    public static PlayerMovement Construct(Player input)
    {
        return new PlayerMovement(input);
    }
    
    /// <summary>
    /// Set the flying speed of the player.
    /// </summary>
    public void SetFlyingSpeed(Double speed)
    {
        strategy.FlyingSpeed = speed;
    }
    
    /// <summary>
    /// Set whether freecam mode is enabled or not.
    /// </summary>
    /// <param name="enabled"><c>true</c> to enable freecam mode, <c>false</c> to disable it.</param>
    public void SetFreecamMode(Boolean enabled)
    {
        strategy = enabled 
            ? new FreecamMovement(player, input, strategy.FlyingSpeed) 
            : new DefaultMovement(player, input, strategy.FlyingSpeed);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        player.Body.Movement = Vector3d.Zero;
        player.Camera.Position = strategy.GetCameraPosition();
        
        // The targeter is acquired here to ensure it is ordered after this component.
        // Targeting is update twice in total, as both camera movement and world manipulation can change the target.
        targeter ??= player.GetComponent<Targeter>();
        targeter?.Update();

        if (player.Scene.CanHandleGameInput)
        {
            player.Body.Movement = strategy.ApplyMovement(deltaTime);
        }
    }
}
