// <copyright file="KeyState.cs" company="OpenTK">
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
using System.Globalization;
using System.Text;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     The current key state.
/// </summary>
public class KeyState
{
    private static readonly VirtualKeys[] allKeys = Enum.GetValues<VirtualKeys>();

    private readonly BitArray keys = new(length: 0xFF);
    private readonly BitArray keysPrevious = new(length: 0xFF);

    internal KeyState() {}

    /// <summary>
    ///     Gets a value indicating whether any key is currently down.
    /// </summary>
    /// <value><c>true</c> if any key is down; otherwise, <c>false</c>.</value>
    public Boolean IsAnyKeyDown => GetAnyKeyDown() != null;

    /// <summary>
    ///     Get the first key that is down, or <c>null</c> if no key is down.
    /// </summary>
    public VirtualKeys? Any => GetAnyKeyDown() ?? null;

    private VirtualKeys? GetAnyKeyDown()
    {
        foreach (VirtualKeys key in allKeys)
        {
            if (key == VirtualKeys.Undefined) continue;

            if (IsKeyDown(key)) return key;
        }

        return null;
    }

    /// <summary>
    ///     Sets the key state of the <paramref name="key" /> depending on the given <paramref name="down" /> value.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> which state should be changed.</param>
    /// <param name="down">The new state the key should be changed to.</param>
    internal void SetKeyState(VirtualKeys key, Boolean down)
    {
        keys[(Int32) key] = down;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        StringBuilder builder = new();
        builder.Append(value: '{');
        Boolean first = true;

        for (VirtualKeys key = 0; key <= VirtualKeys.LastKey; key++)
            if (IsKeyDown(key))
            {
                builder.Append(CultureInfo.InvariantCulture, $"{(!first ? ", " : String.Empty)}{key}");
                first = false;
            }

        builder.Append(value: '}');

        return builder.ToString();
    }

    internal void LogicUpdate()
    {
        keysPrevious.SetAll(value: false);
        keysPrevious.Or(keys);
    }

    internal void Wipe()
    {
        keys.SetAll(value: false);
    }

    /// <summary>
    ///     Gets a <see cref="bool" /> indicating whether this key is currently down.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> to check.</param>
    /// <returns><c>true</c> if <paramref name="key" /> is in the down state; otherwise, <c>false</c>.</returns>
    public Boolean IsKeyDown(VirtualKeys key)
    {
        return keys[(Int32) key];
    }

    /// <summary>
    ///     Gets a <see cref="bool" /> indicating whether this key is currently up.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys" /> to check.</param>
    /// <returns><c>true</c> if <paramref name="key" /> is in the up state; otherwise, <c>false</c>.</returns>
    public Boolean IsKeyUp(VirtualKeys key)
    {
        return !keys[(Int32) key];
    }

    /// <summary>
    ///     Gets a <see cref="bool" /> indicating whether this key was down in the previous frame.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys" /> to check.</param>
    /// <returns><c>true</c> if <paramref name="key" /> was in the down state; otherwise, <c>false</c>.</returns>
    public Boolean WasKeyDown(VirtualKeys key)
    {
        return keysPrevious[(Int32) key];
    }

    /// <summary>
    ///     Gets whether the specified key is pressed in the current frame but released in the previous frame.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> to check.</param>
    /// <returns>True if the key is pressed in this frame, but not the last frame.</returns>
    public Boolean IsKeyPressed(VirtualKeys key)
    {
        return IsKeyDown(key) && !keysPrevious[(Int32) key];
    }

    /// <summary>
    ///     Gets whether the specified key is released in the current frame but pressed in the previous frame.
    /// </summary>
    /// <param name="key">The <see cref="VirtualKeys">key</see> to check.</param>
    /// <returns>True if the key is released in this frame, but pressed the last frame.</returns>
    public Boolean IsKeyReleased(VirtualKeys key)
    {
        return !IsKeyDown(key) && keysPrevious[(Int32) key];
    }
}
