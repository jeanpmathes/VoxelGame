// <copyright file="MockControl.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Internals;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Tests.Visuals;

namespace VoxelGame.GUI.Tests.Controls;

public sealed class MockControl(String tag = "") : Control<MockControl>
{
    public String Tag { get; } = tag;

    protected override ControlTemplate<MockControl> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<MockControl>(_ => new MockVisual());
    }

    public override String ToString()
    {
        return String.IsNullOrEmpty(Tag) ? $"{nameof(MockControl)}()" : $"{nameof(MockControl)}(\"{Tag}\")";
    }
}

public sealed class MockMultiChildControl : MultiChildControl<MockMultiChildControl>
{
    protected override ControlTemplate<MockMultiChildControl> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<MockMultiChildControl>(_ => new MockVisual());
    }
}

public sealed class MockSingleChildControl : SingleChildControl<MockSingleChildControl>
{
    protected override ControlTemplate<MockSingleChildControl> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<MockSingleChildControl>(_ => new MockVisual());
    }
}
