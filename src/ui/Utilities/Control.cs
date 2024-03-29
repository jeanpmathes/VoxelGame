﻿// <copyright file="Control.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Control;

namespace VoxelGame.UI.Utilities;

/// <summary>
///     Utility methods for controls.
/// </summary>
public static class Control
{
    /// <summary>
    ///     Indicate a control as used. This is purely for linting purposes, as controls are always used by their parent.
    /// </summary>
    /// <param name="control">The control to use.</param>
    public static void Used(ControlBase control)
    {
        // Intentionally left empty.
    }
}
