// <copyright file="Binding.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Input;
using VoxelGame.Toolkit.Utilities;
using Action = VoxelGame.Client.Inputs.Actions.Action;
using ActionStateChange = VoxelGame.Client.Inputs.Actions.ActionStateChange;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Connects a <see cref="Keybind" /> to its actions and retains input transitions until they are processed.
/// </summary>
public sealed class Binding
{
    private readonly List<Action> actions = [];
    private readonly ActionRecorder recorder = new();

    /// <summary>
    ///     Create a binding that initially uses the combination specified by its definition.
    /// </summary>
    /// <param name="definition">The keybind that defines the action type and its settings entry.</param>
    internal Binding(Keybind definition)
    {
        Definition = definition;
        Combination = definition.Default;
    }

    /// <summary>
    ///     Get the keybind of this binding.
    /// </summary>
    internal Keybind Definition { get; }

    /// <summary>
    ///     Get or set the key or pointer-button combination that triggers the action.
    ///     Overrides the default combination defined by the keybind.
    /// </summary>
    internal KeyOrButtonCombination Combination { get; set; }

    /// <summary>
    ///     Create an independent action that receives input from this binding until it is disposed.
    /// </summary>
    /// <typeparam name="TAction">The type of action to create.</typeparam>
    /// <returns>The created action.</returns>
    internal TAction CreateAction<TAction>() where TAction : Action, IConstructible<Binding, TAction>
    {
        TAction action = TAction.Construct(this);

        actions.Add(action);

        return action;
    }

    /// <summary>
    ///     Stop updating an action created by this binding.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    internal void RemoveAction(Action action)
    {
        actions.Remove(action);
    }

    /// <summary>
    ///     Activate the binding and record a press if it was previously released.
    /// </summary>
    internal void Press()
    {
        recorder.Press();
    }

    /// <summary>
    ///     Release the binding and record the transition if it was previously active.
    /// </summary>
    /// <returns><see langword="true" /> if the binding changed from active to released; otherwise, <see langword="false" />.</returns>
    internal Boolean Release()
    {
        return recorder.Release();
    }

    /// <summary>
    ///     Apply the accumulated input to all actions, or neutralize them and discard the input when it is not permitted.
    /// </summary>
    /// <param name="isActionPermitted">Whether the action is currently permitted to receive input.</param>
    internal void Process(Boolean isActionPermitted)
    {
        if (isActionPermitted)
        {
            ActionStateChange change = recorder.Consume();

            foreach (Action action in actions)
                action.Apply(change);
        }
        else
        {
            foreach (Action action in actions)
                action.ApplyNeutral();

            DiscardPending();
        }
    }

    /// <summary>
    ///     Reset the held state and discard all transitions waiting for a later action update.
    /// </summary>
    internal void DiscardPending()
    {
        recorder.Discard();
    }
}
