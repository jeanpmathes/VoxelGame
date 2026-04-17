// <copyright file="MockVisual.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Drawing;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Tests.Visuals;

public class MockVisual(String tag = "") : Visual
{
    public String Tag { get; } = tag;

    public override String ToString()
    {
        return String.IsNullOrEmpty(Tag) ? $"{nameof(MockVisual)}()" : $"{nameof(MockVisual)}(\"{Tag}\")";
    }

    #region INPUT

    public Action<InputEvent>? OnInputPreviewHandler { get; set; }
    public Action<InputEvent>? OnInputHandler { get; set; }

    public void SetChildVisual(Visual? child)
    {
        SetChild(child);
    }

    public void AddChildVisual(Visual child)
    {
        AddChild(child);
    }

    public MockVisual CreateDeepChildHierarchy(Int32 depth, Action<MockVisual, Int32> initializer)
    {
        MockVisual current = this;

        for (Int32 currentDepth = 0; currentDepth < depth; currentDepth++)
        {
            MockVisual next = new();

            current.AddChildVisual(next);
            current = next;

            initializer(current, currentDepth);
        }

        return current;
    }

    public MockVisual CreateDeepChildHierarchy(Int32 depth)
    {
        return CreateDeepChildHierarchy(depth, (_, _) => {});
    }

    public void CreateWideChildHierarchy(Int32 width, Action<MockVisual, Int32> initializer)
    {
        for (Int32 index = 0; index < width; index++)
        {
            MockVisual next = new();

            AddChildVisual(next);
            initializer(next, index);
        }
    }

    public void CreateWideChildHierarchy(Int32 width)
    {
        CreateWideChildHierarchy(width, (_, _) => {});
    }

    public override void OnInputPreview(InputEvent inputEvent)
    {
        OnInputPreviewHandler?.Invoke(inputEvent);
    }

    public override void OnInput(InputEvent inputEvent)
    {
        OnInputHandler?.Invoke(inputEvent);
    }

    #endregion INPUT

    #region LAYOUT

    public Int32 MeasureCalls { get; private set; }
    public Int32 ArrangeCalls { get; private set; }

    public override SizeF OnMeasure(SizeF availableSize)
    {
        MeasureCalls++;
        return base.OnMeasure(availableSize);
    }

    public override void OnArrange(RectangleF finalRectangle)
    {
        ArrangeCalls++;
        base.OnArrange(finalRectangle);
    }

    #endregion LAYOUT
}
