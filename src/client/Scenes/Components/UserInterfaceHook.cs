// <copyright file="UserInterfaceHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Attaches a <see cref="UserInterface" /> to a <see cref="Scene" />.
/// </summary>
public partial class UserInterfaceHook : SceneComponent
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<UserInterfaceHook>();

    #endregion LOGGING

    private readonly UserInterface ui;

    [Constructible]
    private UserInterfaceHook(Scene subject, UserInterface ui) : base(subject)
    {
        this.ui = ui;
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
