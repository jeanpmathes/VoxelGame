// <copyright file="LookInput.cs" company="VoxelGame">
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
using OpenTK.Mathematics;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     Accumulates pointer movement and provides a smoothed, sensitivity-adjusted value for looking.
/// </summary>
public class LookInput
{
    private readonly Accumulator accumulator = new();

    private Single sensitivity;

    /// <summary>
    ///     Create look input with the supplied initial sensitivity multiplier.
    /// </summary>
    /// <param name="sensitivity">The initial sensitivity multiplier.</param>
    internal LookInput(Single sensitivity)
    {
        this.sensitivity = sensitivity;
    }

    /// <summary>
    ///     Get the look movement calculated by the most recent call to <see cref="Process" />.
    /// </summary>
    public Vector2d Value { get; private set; }

    /// <summary>
    ///     Handle received pointer movement to be used by the next call to <see cref="Process" />.
    /// </summary>
    /// <param name="movement">The received pointer movement.</param>
    internal void Handle(Vector2d movement)
    {
        accumulator.Accumulate(movement);
    }

    /// <summary>
    ///     Process all received pointer movement, taking into account whether input is permitted.
    ///     Call this in the respective cycle before consumers query <see cref="Value" />.
    /// </summary>
    /// <param name="viewportSize">The viewport size used to normalize pointer movement.</param>
    /// <param name="permitted">Whether input is permitted.</param>
    internal void Process(Vector2i viewportSize, Boolean permitted)
    {
        if (!permitted)
        {
            DiscardPending();
            Value = Vector2d.Zero;

            return;
        }

        Value = accumulator.Consume(viewportSize) * sensitivity;
    }

    /// <summary>
    ///     Discard all received pointer movement without changing <see cref="Value" />.
    ///     The next call to <see cref="Process" /> starts from a neutral state.
    /// </summary>
    internal void DiscardPending()
    {
        accumulator.Discard();
    }

    /// <summary>
    ///     Set the sensitivity applied when pointer movement is next processed.
    /// </summary>
    /// <param name="newSensitivity">The new sensitivity multiplier.</param>
    internal void SetSensitivity(Single newSensitivity)
    {
        sensitivity = newSensitivity;
    }

    private sealed class Accumulator
    {
        private const Single Blend = 0.7f;
        private const Double Scale = 1000.0;

        private Boolean hasPending;
        private Vector2d pending;

        private Vector2d previousResult;

        internal void Accumulate(Vector2d movement)
        {
            pending += movement;
            hasPending = true;
        }

        internal Vector2d Consume(Vector2i viewportSize)
        {
            if (!hasPending || viewportSize.X <= 0 || viewportSize.Y <= 0)
            {
                Discard();

                return Vector2d.Zero;
            }

            Vector2d normalized = Vector2d.Multiply(
                pending,
                (1.0 / viewportSize.X, -1.0 / viewportSize.Y)) * Scale;

            Vector2d current = Vector2d.Lerp(previousResult, normalized, Blend);
            previousResult = current;

            pending = Vector2d.Zero;
            hasPending = false;

            return current;
        }

        internal void Discard()
        {
            pending = Vector2d.Zero;
            hasPending = false;

            previousResult = Vector2d.Zero;
        }
    }
}
