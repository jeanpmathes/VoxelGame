// <copyright file="MathToolsTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities;

[TestSubject(typeof(MathTools))]
public class MathToolsTests
{
    [Fact]
    public void MathTools_SelectByWeight_ShouldSelectEntryWithHighestWeight()
    {
        const Int32 a = 0;
        const Int32 b = 1;
        const Int32 c = 2;
        const Int32 d = 3;

        Int32 selected = MathTools.SelectByWeight(a, b, c, d, (0.0, 0.0));
        Assert.Equal(a, selected);

        selected = MathTools.SelectByWeight(a, b, c, d, (1.0, 0.0));
        Assert.Equal(b, selected);

        selected = MathTools.SelectByWeight(a, b, c, d, (0.0, 1.0));
        Assert.Equal(c, selected);

        selected = MathTools.SelectByWeight(a, b, c, d, (1.0, 1.0));
        Assert.Equal(d, selected);
    }
}
