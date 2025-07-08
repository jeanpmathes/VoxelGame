// <copyright file="PlayerRotator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Client.Inputs;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Rotates the player head and camera based on mouse input.
/// </summary>
public class PlayerRotator : ActorComponent, IConstructible<Player, PlayerRotator>
{
    private readonly Player player;
    
    private readonly LookInput input;
    private readonly Camera camera;
    
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Transform transform;
    
    private PlayerRotator(Player player) : base(player)
    {
        this.player = player;
        
        input = player.Scene.Client.Keybinds.LookBind;
        camera = player.Camera;
        
        transform = player.GetRequiredComponent<Transform>();
    }

    /// <inheritdoc />
    public static PlayerRotator Construct(Player input)
    {
        return new PlayerRotator(input);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        if (!player.Scene.CanHandleGameInput)
            return;
        
        (Double yaw, Double pitch) = input.Value;
        
        // The pitch is clamped in the camera class.
        camera.Yaw += yaw;
        camera.Pitch += pitch;

        transform.Rotation = Quaterniond.FromAxisAngle(Vector3d.UnitY, MathHelper.DegreesToRadians(-camera.Yaw));
    }
}
