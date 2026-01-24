// <copyright file="UserInterfaceHook.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
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
    public override void OnLogicUpdate(Delta delta, Timer? timer)
    {
        using (logger.BeginTimedSubScoped("UI-Hook LogicUpdate", timer))
        {
            ui.LogicUpdate();
        }
    }

    /// <inheritdoc />
    public override void OnRenderUpdate(Delta delta, Timer? timer)
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
        if (disposing) ui.Dispose();

        base.Dispose(disposing);
    }

    #endregion DISPOSABLE
}
