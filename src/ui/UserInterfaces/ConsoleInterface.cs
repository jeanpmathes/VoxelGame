// <copyright file="ConsoleInterface.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Controls;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.UserInterfaces;
     #pragma warning disable CA1001
/// <summary>
///     Allows accessing the ui game console.
/// </summary>
[SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
[SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
#pragma warning disable S2931 // Controls are disposed by their parent.
public class ConsoleInterface
{
    private const int MaxConsoleLogLength = 200;
    private static readonly Color echoColor = Color.Gray;
    private static readonly Color responseColor = Color.White;
    private static readonly Color errorColor = Color.Red;
    private readonly IConsoleProvider console;

    private readonly LinkedList<Entry> consoleLog = new();
    private readonly LinkedList<string> consoleMemory = new();

    private readonly Context context;

    private readonly ControlBase root;

    private MemorizingTextBox? consoleInput;
    private ListBox? consoleOutput;
    private Window? consoleWindow;
    private ControlBase? content;

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

        content = new EmptyControl(consoleWindow);

        GridLayout layout = new(content)
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
            Margin = Margin.One,
            ColumnCount = 2
        };

        DockLayout bottomBar = new(layout)
        {
            Margin = Margin.One
        };

        consoleInput = new MemorizingTextBox(bottomBar)
        {
            LooseFocusOnSubmit = false,
            Dock = Dock.Fill,
            Font = context.Fonts.Console
        };

        consoleInput.SetMemory(consoleMemory);

        Button consoleSubmit = new(bottomBar)
        {
            Dock = Dock.Right,
            Text = Language.Submit,
            Font = context.Fonts.Console
        };

        consoleInput.SubmitPressed += (_, _) => Submit();
        consoleSubmit.Pressed += (_, _) => Submit();

        consoleInput.Focus();

        foreach (Entry entry in consoleLog) AddEntry(entry);

        consoleOutput.ScrollToBottom();

        void Submit()
        {
            string input = consoleInput.Text;
            consoleInput.Memorize();

            if (input.Length == 0) return;

            Write(input, echoColor, context.Fonts.Console, Array.Empty<FollowUp>());
            console.ProcessInput(input);
        }
    }

    /// <summary>
    ///     Write a colored message to the console.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="color">The message color.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="followUp">A group of follow-up actions that can be executed.</param>
    private void Write(string message, Color color, Font font, FollowUp[] followUp)
    {
        Entry entry = new(message, color, font, followUp);

        if (IsOpen)
        {
            Debug.Assert(consoleOutput != null);

            AddEntry(entry);
            consoleOutput.ScrollToBottom();
        }

        consoleLog.AddLast(entry);
        while (consoleLog.Count > MaxConsoleLogLength) consoleLog.RemoveFirst();
    }

    private void AddEntry(Entry entry)
    {
        Debug.Assert(consoleOutput != null);
        Debug.Assert(content != null);

        ListBoxRow row = new(consoleOutput);

        void SetText(int column, string text)
        {
            row.SetCellText(column, text);
            ((Label) row.GetCellContents(column)).Font = entry.Font;
            row.SetTextColor(entry.Color);
        }

        SetText(column: 0, "[ ]");
        SetText(column: 1, entry.Text);

        consoleOutput.AddRow(row);

        if (entry.FollowUp.Length <= 0) return;

        SetText(column: 0, "[a]");

        Menu menu = new(content);

        foreach (FollowUp followUp in entry.FollowUp)
        {
            MenuItem item = new(menu)
            {
                Text = followUp.Description,
                Font = context.Fonts.Console,
                Alignment = Alignment.Left
            };

            item.Pressed += (_, _) => followUp.Action();
        }

        row.RightClicked += (_, arguments) =>
        {
            menu.Position = content.CanvasPosToLocal(new Point(arguments.X, arguments.Y));
            menu.Show();
        };
    }

    /// <summary>
    ///     Write a response message to the console.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="followUp">A group of follow-up actions that can be executed.</param>
    public void WriteResponse(string message, FollowUp[] followUp)
    {
        Write(message, responseColor, context.Fonts.Console, followUp);
    }

    /// <summary>
    ///     Write an error message to the console.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="followUp">A group of follow-up actions that can be executed.</param>
    public void WriteError(string message, FollowUp[] followUp)
    {
        Write(message, errorColor, context.Fonts.ConsoleError, followUp);
    }

    internal void CloseWindow()
    {
        Debug.Assert(consoleWindow != null);
        consoleWindow.Close();
    }

    internal event EventHandler WindowClosed = delegate {};

    private void CleanupAfterClose()
    {
        Debug.Assert(consoleInput != null);
        consoleInput.Blur();

        root.RemoveChild(consoleWindow, dispose: true);

        consoleWindow = null;
        consoleInput = null;
        consoleOutput = null;

        context.Input.AbsorbMousePress();

        WindowClosed(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Clear all console messages.
    /// </summary>
    public void Clear()
    {
        consoleOutput?.Clear();
        consoleLog.Clear();
    }

    private sealed record Entry(string Text, Color Color, Font Font, FollowUp[] FollowUp);
}
     #pragma warning restore CA1001
