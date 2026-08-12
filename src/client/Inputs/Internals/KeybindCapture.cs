// <copyright file="KeybindCapture.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input;
using VoxelGame.Graphics.Input.Events;
using VoxelGame.GUI.Input;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs.Internals;

/// <summary>
///     Captures the next key or button combination.
/// </summary>
internal sealed class KeybindCapture : IDisposable
{
    private readonly Layer targetLayer;

    private Boolean disposed;
    private VirtualKeys? modifierCandidate;

    /// <summary>
    ///     Begin waiting for a combination that may be assigned in the given layer.
    /// </summary>
    /// <param name="targetLayer">The layer whose assignment restrictions the captured combination must satisfy.</param>
    internal KeybindCapture(Layer targetLayer)
    {
        this.targetLayer = targetLayer;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        End(combination: null);
    }

    /// <summary>
    ///     Raised when the capture ends.
    ///     The argument is the captured combination, or <see langword="null" /> if the capture was canceled.
    /// </summary>
    internal event EventHandler<EndedEventArgs>? Ended;

    /// <summary>
    ///     Receive a keyboard event.
    /// </summary>
    /// <param name="args">The received keyboard event.</param>
    internal void ReceiveKeyboardKey(KeyboardKeyEventArgs args)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (args.Key.IsModifier())
        {
            ReceiveModifier(args);

            return;
        }

        if (args is {IsPressed: true, IsRepeat: false})
            Complete(new KeyOrButtonCombination(args.Key, args.Modifiers));
    }

    /// <summary>
    ///     Receive a pointer-button event.
    /// </summary>
    /// <param name="args">The received pointer-button event.</param>
    internal void ReceivePointerButton(MouseButtonEventArgs args)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        if (args.IsPressed)
            Complete(new KeyOrButtonCombination(args.Button, args.Modifiers));
    }

    /// <summary>
    ///     Continue waiting after client input is reset.
    ///     Modifiers held before the reset are not considered part of the combination captured afterward.
    /// </summary>
    internal void Reset()
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        modifierCandidate = null;
    }

    private void ReceiveModifier(KeyboardKeyEventArgs args)
    {
        if (args.IsRepeat) return;

        if (args.IsPressed)
        {
            modifierCandidate = modifierCandidate == null && args.Modifiers == ModifierKeys.None
                ? args.Key
                : null;

            return;
        }

        Boolean useModifierAsKey = modifierCandidate == args.Key && args.Modifiers == ModifierKeys.None;
        modifierCandidate = null;

        if (useModifierAsKey) Complete(new KeyOrButtonCombination(args.Key));
    }

    private void Complete(KeyOrButtonCombination combination)
    {
        if (!targetLayer.Allows(combination)) return;

        End(combination);
    }

    private void End(KeyOrButtonCombination? combination)
    {
        if (disposed) return;
        disposed = true;

        modifierCandidate = null;

        EventHandler<EndedEventArgs>? ended = Ended;
        Ended = null;

        ended?.Invoke(this, new EndedEventArgs(combination));
    }

    /// <summary>
    ///     Provides the result of a completed or canceled capture.
    /// </summary>
    internal sealed class EndedEventArgs(KeyOrButtonCombination? combination) : EventArgs
    {
        /// <summary>
        ///     Get the captured combination, or <see langword="null" /> if the capture was canceled.
        /// </summary>
        internal KeyOrButtonCombination? Combination { get; } = combination;
    }
}
