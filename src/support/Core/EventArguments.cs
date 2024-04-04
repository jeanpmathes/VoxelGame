// <copyright file="EventArguments.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Support.Core;

/// <summary>
///     Event arguments for window size change.
/// </summary>
/// <param name="OldSize">The old size.</param>
/// <param name="NewSize">The new size.</param>
public record SizeChangeEventArgs(Vector2i OldSize, Vector2i NewSize);

/// <summary>
///     Event arguments for focus change.
/// </summary>
/// <param name="OldFocus">Whether the window was focused before.</param>
/// <param name="NewFocus">Whether the window is focused now.</param>
public record FocusChangeEventArgs(Boolean OldFocus, Boolean NewFocus);
