// <copyright file="SceneComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Toolkit.Components;

namespace VoxelGame.Client.Scenes;

/// <summary>
///     Base class for all components of the <see cref="Scene" /> class.
/// </summary>
public partial class SceneComponent(Scene subject) : Component<Scene>(subject);
