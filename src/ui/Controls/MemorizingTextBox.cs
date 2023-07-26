// <copyright file="MemorizingTextBox.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics;
using Gwen.Net.Control;

namespace VoxelGame.UI.Controls;

/// <summary>
///     A text box that memorizes inputs, allowing the user to navigate through them using the up and down arrow keys.
/// </summary>
public class MemorizingTextBox : TextBox
{
    private const int MaxMemorySize = 99;

    private LinkedListNode<string>? current;
    private string? edited;

    /// <summary>
    ///     Creates a new <see cref="MemorizingTextBox" />.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    public MemorizingTextBox(ControlBase parent) : base(parent) {}

    /// <summary>
    ///     Get or set the collection used as memory.
    ///     A lower index indicates more recent input.
    /// </summary>
    public LinkedList<string> Memory { get; private set; } = new();

    /// <summary>
    ///     Set the backing memory to a new collection.
    /// </summary>
    public void SetMemory(LinkedList<string> memory)
    {
        Memory = memory;
        current = null;
    }


    /// <summary>
    ///     Memorizes the current input and clears the text box.
    /// </summary>
    public void Memorize()
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        Memory.AddFirst(Text);

        if (Memory.Count > MaxMemorySize) Memory.RemoveLast();

        current = null;
        Text = string.Empty;
    }

    private void SetText(string text)
    {
        Text = text;

        CursorPos = text.Length;
        CursorEnd = text.Length;
    }

    /// <inheritdoc />
    protected override bool OnKeyUp(bool down)
    {
        if (!down) return false;

        if (Memory.Count == 0) return true;

        if (current == null)
        {
            edited = Text;
            current = Memory.First;
        }
        else
        {
            current = current.Next ?? current;
        }

        Debug.Assert(current != null);
        SetText(current.Value);

        return true;
    }

    /// <inheritdoc />
    protected override bool OnKeyDown(bool down)
    {
        if (!down) return false;

        if (current == null) return true;

        if (current == Memory.First)
        {
            current = null;
            SetText(edited ?? string.Empty);
        }
        else
        {
            current = current.Previous ?? current;
            Debug.Assert(current != null);
            SetText(current.Value);
        }

        return true;
    }
}
