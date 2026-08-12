// <copyright file="KeybindManager.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Client.Inputs.Internals;
using VoxelGame.Client.Scenes;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Presentation.Legacy.Providers;
using VoxelGame.Presentation.Legacy.Settings;
using VoxelGame.Toolkit.Utilities;
using Action = VoxelGame.Client.Inputs.Actions.Action;
using GameClient = VoxelGame.Client.Application.Client;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     Provides the client's configurable keybinds and the actions updated from them.
///     Application actions are updated at the start of each variable-rate input update, while game actions and look
///     input are updated at the start of each fixed-rate logic update.
/// </summary>
public sealed class KeybindManager : ISettingsProvider, IInputActionProvider, IDisposable
{
    private readonly Bindings bindings;
    private readonly InputSettings inputSettings;

    private readonly BindingLayer applicationLayer;
    private readonly BindingLayer gameLayer;

    private readonly IDisposable sensitivityBinding;

    /// <summary>
    ///     Create the runtime bindings for all defined keybinds, register their input handlers, and apply saved
    ///     combinations.
    /// </summary>
    /// <param name="client">The client that provides input and keybind settings.</param>
    internal KeybindManager(GameClient client)
    {
        bindings = new Bindings();

        LookBind = new LookInput(client.Settings.MouseSensitivity);
        sensitivityBinding = client.Settings.MouseSensitivity.Bind(args => LookBind.SetSensitivity(args.NewValue));

        applicationLayer = new BindingLayer(client, Layer.Application, bindings.All);
        gameLayer = new BindingLayer(client, Layer.Game, bindings.All, LookBind);

        inputSettings = new InputSettings(bindings, applicationLayer, gameLayer, this);
    }

    /// <summary>
    ///     Get the pointer movement binding.
    /// </summary>
    public LookInput LookBind { get; }

    /// <summary>
    ///     Get all keybind definitions in definition order.
    /// </summary>
    internal IEnumerable<Keybind> Binds => bindings.Definitions;

    /// <inheritdoc />
    public TAction Use<TAction>(Keybind<TAction> definition) where TAction : Action, IConstructible<Binding, TAction>
    {
        return bindings.Use(definition);
    }

    /// <inheritdoc />
    static String ISettingsProvider.Category => Language.Keybinds;

    /// <inheritdoc />
    static String ISettingsProvider.Description => Language.KeybindsSettingsDescription;

    /// <inheritdoc />
    public IEnumerable<Setting> Settings => inputSettings.All;

    /// <summary>
    ///     Set the <see cref="IInputControl" /> that determines whether application and game actions may accept input.
    ///     Input waiting for a layer that becomes unavailable is discarded.
    /// </summary>
    /// <param name="control">The input control, or <see langword="null" /> to disable both layers.</param>
    internal void SetInputControl(IInputControl? control)
    {
        applicationLayer.SetInputControl(control);
        gameLayer.SetInputControl(control);
    }

    /// <summary>
    ///     Apply received input to application actions.
    /// </summary>
    internal void ProcessApplicationInput()
    {
        applicationLayer.Process();
    }

    /// <summary>
    ///     Apply received input to game actions.
    /// </summary>
    internal void ProcessGameInput()
    {
        gameLayer.Process();
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed) return;

        applicationLayer.Dispose();
        gameLayer.Dispose();

        sensitivityBinding.Dispose();

        disposed = true;
    }

    #endregion DISPOSABLE
}
