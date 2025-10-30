// <copyright file="WorldComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Base class for all components used in the <see cref="World" /> class.
/// </summary>
public partial class WorldComponent(World subject) : Component<World>(subject);
