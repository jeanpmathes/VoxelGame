// <copyright file="SessionComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Client.Sessions;

/// <summary>
///     Base class for all components used in the <see cref="Session"/> class.
/// </summary>
public class SessionComponent(Session subject) : Component<Session>(subject)
{
    /// <inheritdoc cref="Session.LogicUpdate"/>
    public virtual void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        
    }
    
    /// <inheritdoc cref="Session.RenderUpdate"/>
    public virtual void OnRenderUpdate(Double deltaTime, Timer? timer)
    {
        
    }
}
