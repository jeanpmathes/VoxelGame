// <copyright file="Keybind.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using VoxelGame.Client.Inputs.Internals;
using VoxelGame.Graphics.Input;
using VoxelGame.Toolkit.Utilities;
using Action = VoxelGame.Client.Inputs.Actions.Action;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     Represents the definition of a configurable keybind, associating an <see cref="Actions.Action" /> type with a
///     default input combination and a <see cref="Layer" />.
///     Keybinds must be defined before the <see cref="KeybindManager" /> is created.
/// </summary>
public abstract class Keybind
{
    private static readonly List<Keybind> definitions = [];

    private protected Keybind(String id, String name, KeyOrButtonCombination defaultCombination, Layer layer)
    {
        ID = id;
        Name = name;
        Default = defaultCombination;
        Layer = layer;
    }

    private String ID { get; }

    /// <summary>
    ///     Get the name shown when the input combination is configured.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     Get the input combination used when no custom setting exists.
    /// </summary>
    public KeyOrButtonCombination Default { get; }

    /// <summary>
    ///     Get the layer that determines when the action is updated and which input it may receive.
    /// </summary>
    public Layer Layer { get; }

    /// <inheritdoc />
    public override String ToString()
    {
        return ID;
    }

    /// <summary>
    ///     Define a configurable keybind for an action type.
    /// </summary>
    /// <typeparam name="TAction">The type of action created for consumers of the keybind.</typeparam>
    /// <param name="id">The unique, stable identifier used to store the configured combination.</param>
    /// <param name="name">The name shown when the combination is configured.</param>
    /// <param name="defaultCombination">The combination used when no custom setting exists.</param>
    /// <param name="layer">The layer that determines when the action is updated and which input it may receive.</param>
    /// <returns>The keybind definition for <typeparamref name="TAction" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="defaultCombination" /> may not be assigned in <paramref name="layer" />.
    /// </exception>
    public static Keybind<TAction> Define<TAction>(String id, String name, KeyOrButtonCombination defaultCombination, Layer layer)
        where TAction : Action, IConstructible<Binding, TAction>
    {
        if (!layer.Allows(defaultCombination))
            throw Exceptions.ArgumentNotAllowed(nameof(defaultCombination), defaultCombination);

        Keybind<TAction> bind = new(id, name, defaultCombination, layer);

        Debug.Assert(!definitions.Exists(definition => definition.ID == id), $"The binding '{id}' is already defined.");
        definitions.Add(bind);

        return bind;
    }

    /// <summary>
    ///     Remove all pending keybind definitions and return them in definition order.
    /// </summary>
    /// <returns>The pending definitions in the order in which they were defined.</returns>
    internal static IReadOnlyList<Keybind> TakeDefinitions()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Keybinds).TypeHandle);

        Keybind[] result = definitions.ToArray();
        definitions.Clear();

        return result;
    }
}

/// <summary>
///     Represents a keybind associated with a <typeparamref name="TAction" />.
/// </summary>
/// <typeparam name="TAction">The type of action associated with the keybind.</typeparam>
public sealed class Keybind<TAction> : Keybind where TAction : Action, IConstructible<Binding, TAction>
{
    /// <summary>
    ///     Create a keybind for an action type and its default input combination.
    /// </summary>
    /// <param name="id">The unique, stable identifier used to store the configured combination.</param>
    /// <param name="name">The name shown when the combination is configured.</param>
    /// <param name="defaultCombination">The combination used when no custom setting exists.</param>
    /// <param name="layer">The layer that determines when the action is updated and which input it may receive.</param>
    internal Keybind(String id, String name, KeyOrButtonCombination defaultCombination, Layer layer)
        : base(id, name, defaultCombination, layer) {}
}
