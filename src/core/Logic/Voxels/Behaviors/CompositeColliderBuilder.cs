// <copyright file="CompositeColliderBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
/// Helps with building a composite collider for a <see cref="Composite"/> block behavior.
/// </summary>
public class CompositeColliderBuilder
{
    private readonly Composite composite;
    private readonly State state;
    private readonly Func<State, Vector3i, State> setPartPosition;
    
    private readonly Vector3i size;
    private readonly List<BoundingVolume> volumes;
    
    /// <summary>
    /// Create a new composite collider builder.
    /// </summary>
    /// <param name="composite">The composite behavior.</param>
    /// <param name="state">The state of the composite block.</param>
    /// <param name="setPartPosition">A function that sets the part position in a given state.</param>
    public CompositeColliderBuilder(Composite composite, State state, Func<State, Vector3i, State> setPartPosition)
    {
        this.composite = composite;
        this.state = state;
        this.setPartPosition = setPartPosition;
        
        size = composite.GetSize(state);
        volumes =  new List<BoundingVolume>(size.X * size.Y * size.Z);
    }

    /// <summary>
    /// Build the composite collider.
    /// </summary>
    /// <param name="position">The position of the block with the passed state.</param>
    /// <returns>The built box collider.</returns>
    public BoxCollider Build(Vector3i position)
    {
        Vector3i rootPosition = position - composite.GetPartPosition(state);
        
        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            Vector3i currentPart = (x, y, z);
            
            State partState = setPartPosition(state, currentPart);
            
            volumes.Add(composite.Subject.GetBoundingVolume(partState).Translated(currentPart));
        }
        
        BoundingVolume combinedVolume = BoundingVolume.Combine(volumes);

        return new BoxCollider(combinedVolume, new Vector3d(rootPosition.X, rootPosition.Y, rootPosition.Z));
    }
}
