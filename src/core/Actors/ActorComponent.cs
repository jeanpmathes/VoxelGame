// <copyright file="ActorComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.Actors;

/// <summary>
///     Base class for all components used in the <see cref="Actor" /> class.
/// </summary>
public partial class ActorComponent(Actor subject) : Component<Actor>(subject);
