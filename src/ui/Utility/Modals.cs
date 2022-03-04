// <copyright file="Modals.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;

namespace VoxelGame.UI.Utility
{
    /// <summary>
    ///     Utility class for modal windows.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal static class Modals
    {
        private static readonly Color background = new(a: 100, r: 0, g: 0, b: 0);

        /// <summary>
        ///     Opens a modal window with the options yes/no.
        /// </summary>
        internal static void OpenBooleanModal(ControlBase parent, string query, Action yes, Action no)
        {
            MessageBox messageBox = new(parent, query, buttons: MessageBoxButtons.YesNo)
            {
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            messageBox.MakeModal(dim: true, background);

            messageBox.Dismissed += (_, args) => { (args.Result == MessageBoxResult.Yes ? yes : no)(); };
        }

        /// <summary>
        ///     Opens a modal that blocks access to the ui, until it is closed.
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

            modal.MakeModal(dim: true, background);

            Label label = new(modal)
            {
                Text = message
            };

            return new CloseHandel(modal);
        }
    }
}
