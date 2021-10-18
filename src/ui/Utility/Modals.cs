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
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    public static class Modals
    {
        public static void OpenBooleanModal(ControlBase parent, string query, Action yes, Action no)
        {
            MessageBox messageBox = new(parent, query, buttons: MessageBoxButtons.YesNo)
            {
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            Color background = Color.Black;
            background.A = 100;

            messageBox.MakeModal(dim: true, background);

            messageBox.Dismissed += (_, args) =>
            {
                switch (args.Result)
                {
                    case MessageBoxResult.Yes:
                        yes();

                        break;
                    case MessageBoxResult.No:
                        no();

                        break;
                }
            };
        }
    }
}