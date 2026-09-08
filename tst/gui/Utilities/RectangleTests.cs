// <copyright file="RectangleTests.cs" company="VoxelGame">
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

using JetBrains.Annotations;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Utilities;

[TestSubject(typeof(Rectangle))]
public class RectangleTests
{
    [Fact]
    public void Rectangle_ClampSize_ShouldClampSize()
    {
        Rectangle result = Rectangle.ClampSize(new Rectangle(Point.Empty, new Size(Width: 50, Height: 2)), new Size(Width: 10, Height: 5), new Size(Width: 30, Height: 40));

        Assert.Equal(new Rectangle(Point.Empty, new Size(Width: 30, Height: 5)), result);
    }

    [Fact]
    public void Rectangle_ClampSize_ShouldNotChangePosition()
    {
        Rectangle result = Rectangle.ClampSize(new Rectangle(new Point(X: 5, Y: 10), new Size(Width: 50, Height: 2)), new Size(Width: 10, Height: 5), new Size(Width: 30, Height: 40));

        Assert.Equal(new Rectangle(new Point(X: 5, Y: 10), new Size(Width: 30, Height: 5)), result);
    }
}
