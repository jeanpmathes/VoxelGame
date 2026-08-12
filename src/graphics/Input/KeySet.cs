// <copyright file="KeySet.cs" company="OpenTK">
//      Copyright (c) 2006-2019 Stefanos Apostolopoulos for the Open Toolkit project.
//      
//      Permission is hereby granted, free of charge, to any person obtaining a copy
//      of this software and associated documentation files (the "Software"), to deal
//      in the Software without restriction, including without limitation the rights
//      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//      copies of the Software, and to permit persons to whom the Software is
//      furnished to do so, subject to the following conditions:
//      
//      - The above copyright notice and this permission notice shall be included in all
//        copies or substantial portions of the Software.
//      
//      - THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//        SOFTWARE.
// </copyright>
// <author>OpenTK</author>

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using VoxelGame.Graphics.Definition;
using VoxelGame.GUI.Input;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     An efficient representation of a key (and button) state.
/// </summary>
public class KeySet
{
    private static readonly VirtualKeys[] allKeys = Enum.GetValues<VirtualKeys>();

    private readonly BitArray keys = new(length: 0xFF);

    static KeySet()
    {
        Debug.Assert((Int32) VirtualKeys.LastKey < 0xFF);
    }

    /// <summary>
    /// Gets the key state of the <paramref name="key" />.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> to get the state of.</param>
    /// <returns>The state of the key.</returns>
    public Boolean Contains(VirtualKeys key)
    {
        return keys[(Int32) key];
    }

    /// <summary>
    ///     Sets the key state of the <paramref name="key" /> depending on the given <paramref name="state" /> value.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> which state should be changed.</param>
    /// <param name="state">The new state the key should be changed to.</param>
    public void Set(VirtualKeys key, Boolean state)
    {
        keys[(Int32) key] = state;
    }

    /// <summary>
    ///     Adds the specified <paramref name="key" /> to the key state by setting it to active.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys" /> to add to the key state.</param>
    public void Add(VirtualKeys key)
    {
        Set(key, state: true);
    }

    /// <summary>
    ///     Clears the key state of the <paramref name="key" />.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> which state should be cleared.</param>
    /// <returns>The previous state of the key.</returns>
    public Boolean Remove(VirtualKeys key)
    {
        Boolean previous = keys[(Int32) key];
        keys[(Int32) key] = false;
        return previous;
    }

    /// <summary>
    ///     Wipes the key state.
    /// </summary>
    public void Reset()
    {
        keys.SetAll(value: false);
    }

    /// <summary>
    ///     Get the active keyboard modifiers, optionally as if one key had already been released.
    /// </summary>
    public ModifierKeys GetModifiers(VirtualKeys? releasedKey = null)
    {
        ModifierKeys modifiers = ModifierKeys.None;

        if (GetExcept(VirtualKeys.LeftControl, releasedKey) || GetExcept(VirtualKeys.RightControl, releasedKey))
            modifiers |= ModifierKeys.Control;

        if (GetExcept(VirtualKeys.LeftMenu, releasedKey) || GetExcept(VirtualKeys.RightMenu, releasedKey))
            modifiers |= ModifierKeys.Alt;

        if (GetExcept(VirtualKeys.LeftShift, releasedKey) || GetExcept(VirtualKeys.RightShift, releasedKey))
            modifiers |= ModifierKeys.Shift;

        return modifiers;
    }

    private Boolean GetExcept(VirtualKeys key, VirtualKeys? excluded)
    {
        return key != excluded && Contains(key);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        StringBuilder builder = new();
        builder.Append(value: '{');
        Boolean first = true;

        foreach (VirtualKeys key in allKeys)
            if (Contains(key))
            {
                builder.Append(CultureInfo.InvariantCulture, $"{(!first ? ", " : String.Empty)}{key}");
                first = false;
            }

        builder.Append(value: '}');

        return builder.ToString();
    }
}
