// <copyright file="Modals.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    internal static void SetupLanguage()
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
    /// Open a model that asks whether to delete something.
    /// </summary>
    internal static void OpenDeletionModal(ControlBase parent, DeletionBox.Parameters parameters, DeletionBox.Actions actions)
    {
        DeletionBox deletionBox = new(parent, parameters, actions);

        Context.MakeModal(deletionBox);
    }

    /// <summary>
    ///     Open a model that asks for a (new) name.
    /// </summary>
    internal static void OpenNameModal(ControlBase parent, NameBox.Parameters parameters, NameBox.Actions actions)
    {
        NameBox nameBox = new(parent, parameters, actions);

        Context.MakeModal(nameBox);
    }

    /// <summary>
    ///     Opens a modal that blocks access to the ui, until it is closed by code.
    /// </summary>
    internal static CloseHandel OpenBlockingModal(ControlBase parent, string message)
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

        Context.MakeModal(modal);

        Label label = new(modal)
        {
            Text = message
        };

        Control.Used(label);

        return new CloseHandel(modal);
    }
}
