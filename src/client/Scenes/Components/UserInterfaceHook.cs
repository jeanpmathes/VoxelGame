// <copyright file="UserInterfaceHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
/// Attaches a <see cref="UserInterface"/> to a <see cref="Scene"/>.
/// </summary>
public class UserInterfaceHook : SceneComponent, IConstructible<Scene, UserInterface, UserInterfaceHook>
{
    private readonly UserInterface ui;
    
    private UserInterfaceHook(Scene subject, UserInterface ui) : base(subject)
    {
        this.ui = ui;
    }

    /// <inheritdoc />
    public static UserInterfaceHook Construct(Scene input1, UserInterface input2)
    {
        return new UserInterfaceHook(input1, input2);
    }
    
    /// <inheritdoc />
    public override void OnLoad()
    {
        ui.Load();
        ui.Resize(Subject.Client.Size);

        ui.CreateControl();
    }

    /// <inheritdoc />
    public override void OnResize(Vector2i size)
    {
        ui.Resize(size);
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime, Timer? timer)
    {
        using (logger.BeginTimedSubScoped("UI-Hook LogicUpdate", timer))
        {
            ui.LogicUpdate();
        }
    }
    
    /// <inheritdoc />
    public override void OnRenderUpdate(Double deltaTime, Timer? timer)
    {
        using (logger.BeginTimedSubScoped("UI-Hook RenderUpdate", timer))
        {
            ui.RenderUpdate();
        }
    }
    
    #region LOGGING
    
    private static readonly ILogger logger = LoggingHelper.CreateLogger<UserInterfaceHook>();
    
    #endregion LOGGING

    #region DISPOSABLE
    
    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposing)
        {
            ui.Dispose();
        }
        
        base.Dispose(disposing);
    }
    
    #endregion DISPOSABLE
}
