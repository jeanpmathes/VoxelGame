// <copyright file="SizesTests.cs" company="VoxelGame">
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

using System.Drawing;
using JetBrains.Annotations;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Utilities;

[TestSubject(typeof(Sizes))]
public class SizesTests
{
    [Fact]
    public void Sizes_Clamp_ShouldConstrainAllComponents()
    {
        SizeF result = Sizes.Clamp(new SizeF(width: 50, height: 2), new SizeF(width: 10, height: 5), new SizeF(width: 30, height: 40));

        Assert.Equal(new SizeF(width: 30, height: 5), result);
    }

    [Fact]
    public void Sizes_Max_ShouldReturnComponentWiseMaximum()
    {
        SizeF result = Sizes.Max(new SizeF(width: 10, height: 50), new SizeF(width: 20, height: 40));

        Assert.Equal(new SizeF(width: 20, height: 50), result);
    }
}
