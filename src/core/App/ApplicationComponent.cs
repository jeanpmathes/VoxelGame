// <copyright file="ApplicationComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.App;

/// <summary>
///     Base class for all components used in the <see cref="Application" /> class.
/// </summary>
public partial class ApplicationComponent(Application application) : Component<Application>(application);
