// <copyright file="Spawning.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
///     Places an actor at the spawn point of a world when it is added to the world.
/// </summary>
public partial class Spawning : ActorComponent
{
    [Constructible]
    private Spawning(Actor subject) : base(subject) {}

    /// <inheritdoc />
    public override void OnAdd()
    {
        if (Subject.GetComponent<Transform>() is {} transform)
        {
            transform.Position = Subject.World.SpawnPosition;
        }
    }
}
