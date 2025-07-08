// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Actors;

/// <summary>
///     A player that can interact with the world.
/// </summary>
public abstract class Player : Actor
{
    /// <summary>
    /// The body of the player.
    /// </summary>
    public Body Body { get; }
    
    /// <summary>
    ///     Create a new player.
    /// </summary>
    /// <param name="mass">The mass the player has.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    protected Player(Double mass, BoundingVolume boundingVolume)
    {
        Body = AddComponent<Body, Body.Characteristics>(new Body.Characteristics(mass, boundingVolume));
        
        AddComponent<Spawning>();
        AddComponent<ChunkLoader>();
    }
}
