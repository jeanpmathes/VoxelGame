// <copyright file="Modals.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Controls.Common;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Utilities;

/// <summary>
///     Utility class for modal windows.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
internal static class Modals
{
    /// <summary>
    ///     Set up the language (text) for the modals.
    /// </summary>
    internal static void SetUpLanguage()
    {
        MessageBoxButtonTexts.Shared = new MessageBoxButtonTexts
        {
            Abort = Language.Abort,
            Cancel = Language.Cancel,
            Ignore = Language.Ignore,
            No = Language.No,
            Ok = Language.Ok,
            Retry = Language.Retry,
            Yes = Language.Yes
        };
    }

    /// <summary>
    ///     Open a model that asks whether to delete something.
    /// </summary>
    internal static void OpenDeletionModal(ControlBase parent, DeletionBox.Parameters parameters, DeletionBox.Actions actions, Context context)
    {
        DeletionBox deletionBox = new(parent, parameters, actions);

        context.MakeModal(deletionBox);
    }

    /// <summary>
    ///     Open a model that asks for a (new) name.
    /// </summary>
    internal static void OpenNameModal(ControlBase parent, NameBox.Parameters parameters, NameBox.Actions actions, Context context)
    {
        NameBox nameBox = new(parent, parameters, actions);

        context.MakeModal(nameBox);
    }

    /// <summary>
    ///     Opens a modal that blocks access to the ui, until it is closed by code.
    /// </summary>
    internal static CloseHandel OpenBlockingModal(ControlBase parent, String message, Context context)
    {
        Window modal = new(parent)
        {
            IsClosable = false,
            DeleteOnClose = true,
            StartPosition = StartPosition.CenterCanvas,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Resizing = Resizing.None,
            IsDraggingEnabled = false
        };

        context.MakeModal(modal);

        Label label = new(modal)
        {
            Text = message
        };

        Control.Used(label);

        return new CloseHandel(modal);
    }
}
