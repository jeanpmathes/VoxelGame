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
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input.Actions;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     Represents a keybind, that associates a key with a binding.
/// </summary>
public readonly struct Keybind : IEquatable<Keybind>
{
    private readonly String id;
    private readonly Binding type;

    /// <summary>
    ///     Get the name of the keybind.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     Get the key used by the keybind per default.
    /// </summary>
    public VirtualKeys Default { get; }

    private enum Binding
    {
        PushButton,
        ToggleButton,
        SimpleButton
    }

    private Keybind(String id, String name, Binding type, VirtualKeys defaultKeyOrButton)
    {
        this.id = id;
        Name = name;
        this.type = type;

        Default = defaultKeyOrButton;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is Keybind other) return this == other;

        return false;
    }

    /// <inheritdoc />
    public Boolean Equals(Keybind other)
    {
        return id.Equals(other.id, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return id.GetHashCode(StringComparison.InvariantCulture);
    }

    /// <summary>
    ///     Check equality of two keybinds.
    /// </summary>
    /// <param name="left">The first keybind.</param>
    /// <param name="right">The second keybind.</param>
    /// <returns>True if both keybinds are equal.</returns>
    public static Boolean operator ==(Keybind left, Keybind right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Check inequality of two keybinds.
    /// </summary>
    /// <param name="left">The first keybind.</param>
    /// <param name="right">The second keybind.</param>
    /// <returns>True if both keybinds are not equal.</returns>
    public static Boolean operator !=(Keybind left, Keybind right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return id;
    }

    /// <summary>
    ///     Register a keybind that is bound to a button.
    /// </summary>
    /// <param name="id">The id of the keybind. Must be unique.</param>
    /// <param name="name">The display name of the keybind. Can be localized.</param>
    /// <param name="defaultKey">The default key to use initially.</param>
    /// <returns>The registered keybind.</returns>
    public static Keybind RegisterButton(String id, String name, VirtualKeys defaultKey)
    {
        return Register(
            id,
            name,
            Binding.SimpleButton,
            defaultKey);
    }

    /// <summary>
    ///     Register a keybind that is bound to a toggle.
    /// </summary>
    /// <param name="id">The id of the keybind. Must be unique.</param>
    /// <param name="name">The display name of the keybind. Can be localized.</param>
    /// <param name="defaultKey">The default key to use initially.</param>
    /// <returns>The registered keybind.</returns>
    public static Keybind RegisterToggle(String id, String name, VirtualKeys defaultKey)
    {
        return Register(
            id,
            name,
            Binding.ToggleButton,
            defaultKey);
    }

    /// <summary>
    ///     Register a keybind that is bound to a push button.
    /// </summary>
    /// <param name="id">The id of the keybind. Must be unique.</param>
    /// <param name="name">The display name of the keybind. Can be localized.</param>
    /// <param name="defaultKey">The default key to use initially.</param>
    /// <returns>The registered keybind.</returns>
    public static Keybind RegisterPushButton(String id, String name, VirtualKeys defaultKey)
    {
        return Register(
            id,
            name,
            Binding.PushButton,
            defaultKey);
    }

    private static Keybind Register(String id, String name, Binding type, VirtualKeys defaultKey)
    {
        var bind = new Keybind(id, name, type, defaultKey);

        Debug.Assert(!bindings.Contains(bind), $"The binding '{bind.id}' is already defined.");
        bindings.Add(bind);

        return bind;
    }

    private static readonly HashSet<Keybind> bindings = new();

    internal static void RegisterWithManager(KeybindManager manager)
    {
        foreach (Keybind bind in bindings) bind.AddToManager(manager);

        bindings.Clear();
    }

    private void AddToManager(KeybindManager manager)
    {
        switch (type)
        {
            case Binding.PushButton:
                manager.Add(this, new PushButton(Default, manager.Input));

                break;

            case Binding.ToggleButton:
                manager.Add(this, new ToggleButton(Default, manager.Input));

                break;

            case Binding.SimpleButton:
                manager.Add(this, new SimpleButton(Default, manager.Input));

                break;

            default:
                Debug.Fail("Add missing cases.");

                break;
        }
    }
}
