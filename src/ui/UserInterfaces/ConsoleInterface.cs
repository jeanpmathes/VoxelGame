// <copyright file="ConsoleInterface.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces
{
     #pragma warning disable CA1001

    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    public class ConsoleInterface
    {
        private const int MaxConsoleLogLength = 200;
        private static readonly Color inputColor = Color.Gray;
        private static readonly Color responseColor = Color.White;
        private static readonly Color errorColor = Color.Red;
        private readonly IConsoleProvider console;

        private readonly LinkedList<(string input, Color color)> consoleLog = new();
        private readonly Context context;

        private readonly ControlBase root;
        private ListBox? consoleOutput;

        private Window? consoleWindow;

        internal ConsoleInterface(ControlBase root, IConsoleProvider console, Context context)
        {
            this.root = root;
            this.console = console;
            this.context = context;
        }

        internal bool IsOpen => consoleWindow != null;

        internal void OpenWindow()
        {
            consoleWindow = new Window(root)
            {
                StartPosition = StartPosition.Manual,
                Position = new Point(x: 0, y: 0),
                Size = new Size(width: 900, height: 400),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Resizing = Resizing.None,
                IsDraggingEnabled = false
            };

            consoleWindow.Closed += (_, _) => CleanupAfterClose();
            consoleWindow.MakeModal(dim: true, new Color(a: 170, r: 40, g: 40, b: 40));

            GridLayout layout = new(consoleWindow)
            {
                Dock = Dock.Fill,
                Margin = Margin.Ten
            };

            layout.SetColumnWidths(1f);
            layout.SetRowHeights(0.9f, 0.1f);

            consoleOutput = new ListBox(layout)
            {
                AlternateColor = false,
                CanScrollH = false,
                CanScrollV = true,
                Dock = Dock.Fill,
                Margin = Margin.One
            };

            DockLayout bottomBar = new(layout)
            {
                Margin = Margin.One
            };

            TextBox consoleInput = new(bottomBar)
            {
                Dock = Dock.Fill
            };

            Button consoleSubmit = new(bottomBar)
            {
                Dock = Dock.Right,
                Text = Language.Submit
            };

            consoleInput.SubmitPressed += (_, _) => Submit();
            consoleSubmit.Pressed += (_, _) => Submit();

            foreach ((string entry, Color color) in consoleLog) consoleOutput.AddRow(entry).SetTextColor(color);

            consoleOutput.ScrollToBottom();

            void Submit()
            {
                string input = consoleInput.Text;
                consoleInput.SetText("");

                Write(input, inputColor);
                console.ProcessInput(input);
            }
        }

        public void Write(string message, Color color)
        {
            if (IsOpen)
            {
                Debug.Assert(consoleOutput != null);
                consoleOutput.AddRow(message).SetTextColor(color);
                consoleOutput.ScrollToBottom();
            }

            consoleLog.AddLast((message, color));
            while (consoleLog.Count > MaxConsoleLogLength) consoleLog.RemoveFirst();
        }

        public void WriteResponse(string message)
        {
            Write(message, responseColor);
        }

        public void WriteError(string message)
        {
            Write(message, errorColor);
        }

        internal void CloseWindow()
        {
            Debug.Assert(consoleWindow != null);
            consoleWindow.Close();
        }

        private void CleanupAfterClose()
        {
            consoleWindow = null;
            consoleOutput = null;

            context.Input.AbsorbMousePress();
        }
    }
     #pragma warning restore CA1001
}