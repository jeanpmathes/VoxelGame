﻿// <copyright file="PreviousPosition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Stores the previous position of an actor, e.g. before a teleportation.
/// Is used only by the command system.
/// </summary>
public class PreviousPosition : ActorComponent, IConstructible<Actor, PreviousPosition>
{
    private PreviousPosition(Actor subject) : base(subject) 
    {
    }
    
    /// <inheritdoc />
    public static PreviousPosition Construct(Actor input)
    {
        return new PreviousPosition(input);
    }
    
    /// <summary>
    /// Get the previous position of the actor, as set by the command system.
    /// </summary>
    public Vector3d Value { get; set; } = Vector3d.Zero;
}
