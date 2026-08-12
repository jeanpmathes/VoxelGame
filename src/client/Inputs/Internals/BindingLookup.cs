// <copyright file="BindingLookup.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input;
using VoxelGame.GUI.Input;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Allows lookup of bindings by their input combination, primary key or button, and by modifiers.
/// </summary>
internal sealed class BindingLookup
{
    private static readonly List<Binding> noBindings = [];

    private readonly Dictionary<KeyOrButtonCombination, List<Binding>> bindingsByCombination = new();
    private readonly Dictionary<VirtualKeys, List<Binding>> bindingsByPrimaryKeyOrButton = new();
    private readonly Dictionary<ModifierKeys, List<Binding>> bindingsByModifiers = new();

    /// <summary>
    ///     Create a lookup containing the given bindings.
    /// </summary>
    /// <param name="bindings">The bindings that may be found through this lookup.</param>
    internal BindingLookup(IReadOnlyList<Binding> bindings)
    {
        foreach (Binding binding in bindings)
            Add(binding);
    }

    /// <summary>
    ///     Get the bindings activated when the combination is pressed.
    /// </summary>
    /// <param name="combination">The pressed combination.</param>
    /// <returns>The matching bindings, or an empty list if no binding uses the combination.</returns>
    internal List<Binding> GetByCombination(KeyOrButtonCombination combination)
    {
        return bindingsByCombination.GetValueOrDefault(combination, noBindings);
    }

    /// <summary>
    ///     Get the bindings that use the given key or pointer button as their primary input.
    /// </summary>
    /// <param name="primaryInput">The primary key or pointer button.</param>
    /// <returns>The bindings using the primary input, or an empty list if no binding uses it.</returns>
    internal List<Binding> GetByPrimaryKeyOrButton(VirtualKeys primaryInput)
    {
        return bindingsByPrimaryKeyOrButton.GetValueOrDefault(primaryInput, noBindings);
    }

    /// <summary>
    ///     Get the bindings that use and require the given modifier.
    /// </summary>
    /// <param name="modifier">The required modifier.</param>
    /// <returns>The bindings requiring the modifier, or an empty list if no binding requires it.</returns>
    internal List<Binding> GetByModifier(ModifierKeys modifier)
    {
        return bindingsByModifiers.GetValueOrDefault(modifier, noBindings);
    }

    /// <summary>
    ///     Assign a new combination to a binding and use it for later lookups.
    /// </summary>
    /// <param name="binding">The binding to update.</param>
    /// <param name="combination">The new combination.</param>
    internal void Rebind(Binding binding, KeyOrButtonCombination combination)
    {
        Remove(binding);
        binding.Combination = combination;
        Add(binding);
    }

    private void Add(Binding binding)
    {
        AddToLookup(bindingsByCombination, binding.Combination, binding);
        AddToLookup(bindingsByPrimaryKeyOrButton, binding.Combination.KeyOrButton, binding);

        foreach (ModifierKeys modifier in Modifiers.All)
            if (binding.Combination.Modifiers.HasFlag(modifier))
                AddToLookup(bindingsByModifiers, modifier, binding);
    }

    private void Remove(Binding binding)
    {
        RemoveFromLookup(bindingsByCombination, binding.Combination, binding);
        RemoveFromLookup(bindingsByPrimaryKeyOrButton, binding.Combination.KeyOrButton, binding);

        foreach (ModifierKeys modifier in Modifiers.All)
            if (binding.Combination.Modifiers.HasFlag(modifier))
                RemoveFromLookup(bindingsByModifiers, modifier, binding);
    }

    private static void AddToLookup<TKey>(Dictionary<TKey, List<Binding>> lookup, TKey key, Binding binding) where TKey : notnull
    {
        if (!lookup.TryGetValue(key, out List<Binding>? matches))
        {
            matches = [];
            lookup.Add(key, matches);
        }

        matches.Add(binding);
    }

    private static void RemoveFromLookup<TKey>(Dictionary<TKey, List<Binding>> lookup, TKey key, Binding binding) where TKey : notnull
    {
        List<Binding> matches = lookup[key];
        matches.Remove(binding);

        if (matches.Count == 0)
            lookup.Remove(key);
    }
}
