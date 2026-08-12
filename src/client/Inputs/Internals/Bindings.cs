// <copyright file="Bindings.cs" company="VoxelGame">
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Inputs.Actions;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Creates the runtime bindings for all defined keybinds and provides their actions.
/// </summary>
internal sealed partial class Bindings
{
    private readonly Dictionary<Keybind, Binding> bindings = new();

    /// <summary>
    ///     Create the bindings for all definitions made before the <see cref="KeybindManager" /> was created.
    /// </summary>
    internal Bindings()
    {
        List<Binding> all = [];

        foreach (Keybind definition in Keybind.TakeDefinitions())
        {
            Binding binding = new(definition);

            bindings.Add(definition, binding);
            all.Add(binding);

            LogCreatedKeybind(logger, definition);
        }

        All = all;
    }

    /// <summary>
    ///     Get all bindings in their definition order.
    /// </summary>
    internal IReadOnlyList<Binding> All { get; }

    /// <summary>
    ///     Get all keybind definitions in their definition order.
    /// </summary>
    internal IEnumerable<Keybind> Definitions => All.Select(binding => binding.Definition);

    /// <summary>
    ///     Create an independent action for a keybind definition.
    /// </summary>
    /// <typeparam name="TAction">The type of action associated with the keybind.</typeparam>
    /// <param name="definition">The definition for which an action is created.</param>
    /// <returns>The created action, which must be disposed when its consumer stops using it.</returns>
    internal TAction Use<TAction>(Keybind<TAction> definition) where TAction : Action, IConstructible<Binding, TAction>
    {
        return bindings[definition].CreateAction<TAction>();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Bindings>();

    [LoggerMessage(EventId = LogID.Bindings + 0, Level = LogLevel.Debug, Message = "Created keybind: {Bind}")]
    private static partial void LogCreatedKeybind(ILogger logger, Keybind bind);

    #endregion LOGGING
}
