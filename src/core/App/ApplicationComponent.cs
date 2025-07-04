// <copyright file="ApplicationComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.App;

/// <summary>
///     Base class for all components used in the <see cref="Application"/> class.
/// </summary>
public class ApplicationComponent(Application application) : Component<Application>(application)
{
    /// <inheritdoc cref="Application.OnInitialization" />
    public virtual void OnInitialization(Timer? timer) {}

    /// <inheritdoc cref="Application.OnLogicUpdate" />
    public virtual void OnLogicUpdate(Double delta, Timer? timer) {}

    /// <inheritdoc cref="Application.OnRenderUpdate" />
    public virtual void OnRenderUpdate(Double delta, Timer? timer) {}

    /// <inheritdoc cref="Application.OnDestroy" />
    public virtual void OnDestroy(Timer? timer) {}
}
