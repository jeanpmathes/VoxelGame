// <copyright file="BindingLayer.cs" company="VoxelGame">
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
using System.Linq;
using VoxelGame.Client.Scenes;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;
using VoxelGame.Toolkit.Utilities;
using GameClient = VoxelGame.Client.Application.Client;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Represents a <see cref="Layer" /> and implements a <see cref="IInputHandler" /> to process the inputs for it.
/// </summary>
internal sealed class BindingLayer : IInputHandler, IDisposable
{
    private readonly GameClient client;
    private readonly Layer layer;
    private readonly IDisposable registration;

    private readonly LookInput? lookInput;

    private readonly IReadOnlyList<Binding> bindings;
    private readonly BindingLookup lookup;

    private KeybindCapture? capture;
    private IInputControl? control;

    /// <summary>
    ///     Create a new binding layer, which will directly register it with the client.
    /// </summary>
    /// <param name="client">The client whose input this layer receives.</param>
    /// <param name="layer">The layer whose bindings are handled.</param>
    /// <param name="allBindings">The runtime bindings from which bindings defined in <paramref name="layer" /> are selected.</param>
    /// <param name="lookInput">
    ///     The look input that receives pointer movement, or <see langword="null" /> if the layer does not handle looking.
    /// </param>
    internal BindingLayer(GameClient client, Layer layer, IReadOnlyList<Binding> allBindings, LookInput? lookInput = null)
    {
        this.client = client;
        this.layer = layer;
        this.lookInput = lookInput;

        bindings = allBindings.Where(binding => binding.Definition.Layer == layer).ToArray();
        lookup = new BindingLookup(bindings);

        registration = client.Input.RegisterHandler(this, layer.InputHandlerLayer);
        client.Input.Reset += OnInputReset;
    }

    private Boolean IsActionPermitted
    {
        get
        {
            if (control == null) return false;

            return layer switch
            {
                Layer.Application => control.CanHandleApplicationInput,
                Layer.Game => control.CanHandleGameInput,
                _ => throw Exceptions.UnsupportedEnumValue(layer)
            };
        }
    }

    /// <inheritdoc />
    public Boolean HandleKeyboardKey(KeyboardKeyEventArgs args)
    {
        if (args.IsPressed && capture != null)
        {
            capture.ReceiveKeyboardKey(args);

            return true;
        }

        Boolean handled = HandleKeyOrButton(args.Key, args.IsPressed, args.IsRepeat, args.Modifiers);

        if (args.IsPressed || capture == null)
            return handled;

        capture.ReceiveKeyboardKey(args);

        return true;
    }

    /// <inheritdoc />
    public Boolean HandleMouseButton(MouseButtonEventArgs args)
    {
        if (args.IsPressed && capture != null)
        {
            capture.ReceivePointerButton(args);

            return true;
        }

        Boolean handled = HandleKeyOrButton(args.Button, args.IsPressed, isRepeat: false, args.Modifiers);

        if (args.IsPressed || capture == null) return handled;

        capture.ReceivePointerButton(args);

        return true;
    }

    /// <inheritdoc />
    public void HandleModifiersLost(ModifierKeys modifiers)
    {
        foreach (ModifierKeys modifier in Modifiers.All)
        {
            if (!modifiers.HasFlag(modifier)) continue;

            foreach (Binding binding in lookup.GetByModifier(modifier))
                binding.Release();
        }
    }

    /// <inheritdoc />
    public Boolean HandleMouseMove(MouseMoveEventArgs args)
    {
        if (lookInput == null) return false;

        if (!IsActionPermitted)
        {
            DiscardPending();

            return false;
        }

        lookInput.Handle(args.Delta);

        return true;
    }

    /// <summary>
    ///     Set the <see cref="IInputControl" /> that determines whether this layer may accept input.
    ///     Pending input is discarded when the layer becomes unavailable.
    /// </summary>
    /// <param name="newControl">The input control, or <see langword="null" /> to disable the layer.</param>
    internal void SetInputControl(IInputControl? newControl)
    {
        control = newControl;

        if (!IsActionPermitted) DiscardPending();
    }

    /// <summary>
    ///     Process all bindings and potentially look input, taking into account the <see cref="IInputControl" />.
    ///     Call this in the respective cycle before consumers query input actions.
    /// </summary>
    internal void Process()
    {
        Boolean isActionPermitted = IsActionPermitted;

        foreach (Binding binding in bindings)
            binding.Process(isActionPermitted);

        lookInput?.Process(client.Size, isActionPermitted);
    }

    /// <summary>
    ///     Begin capturing a new combination for a keybind.
    ///     While active, the capture receives all inputs and no actions will receive input.
    ///     Starting a capture cancels any capture already active on this layer.
    /// </summary>
    /// <param name="targetLayer">The layer whose assignment restrictions the selected combination must satisfy.</param>
    /// <param name="receiveCombination">Receives the combination that completes the selection.</param>
    /// <returns>A handle that cancels this capture when disposed.</returns>
    internal IDisposable BeginCapture(Layer targetLayer, Action<KeyOrButtonCombination> receiveCombination)
    {
        capture?.Dispose();

        KeybindCapture nextCapture = new(targetLayer);
        nextCapture.Ended += OnEnded;

        capture = nextCapture;

        return nextCapture;

        void OnEnded(Object? sender, KeybindCapture.EndedEventArgs args)
        {
            if (!ReferenceEquals(capture, sender)) return;

            capture = null;

            if (args.Combination is {} combination)
                receiveCombination(combination);
        }
    }

    /// <summary>
    ///     Assign a new combination to a binding for later presses.
    ///     Any held state for the previous combination is discarded.
    /// </summary>
    /// <param name="binding">The binding defined in this layer.</param>
    /// <param name="combination">The permitted combination to assign.</param>
    internal void Rebind(Binding binding, KeyOrButtonCombination combination)
    {
        Debug.Assert(binding.Definition.Layer == layer);
        Debug.Assert(layer.Allows(combination));

        lookup.Rebind(binding, combination);
        binding.DiscardPending();
    }

    private Boolean HandleKeyOrButton(VirtualKeys keyOrButton, Boolean isPressed, Boolean isRepeat, ModifierKeys modifiers)
    {
        if (!isPressed)
        {
            Boolean handled = false;

            foreach (Binding binding in lookup.GetByPrimaryKeyOrButton(keyOrButton))
                handled |= binding.Release();

            return handled;
        }

        if (isRepeat) return false;

        if (!IsActionPermitted)
        {
            DiscardPending();

            return false;
        }

        KeyOrButtonCombination combination = new(keyOrButton, modifiers);
        IReadOnlyList<Binding> matches = lookup.GetByCombination(combination);

        foreach (Binding binding in matches)
            binding.Press();

        return matches.Count > 0;
    }

    private void DiscardPending()
    {
        foreach (Binding binding in bindings)
            binding.DiscardPending();

        lookInput?.DiscardPending();
    }

    private void OnInputReset(Object? sender, EventArgs args)
    {
        capture?.Reset();

        DiscardPending();
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed) return;

        capture?.Dispose();
        capture = null;

        client.Input.Reset -= OnInputReset;
        registration.Dispose();

        disposed = true;
    }

    #endregion DISPOSABLE
}
