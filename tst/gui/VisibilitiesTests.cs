// <copyright file="VisibilitiesTests.cs" company="VoxelGame">
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
using Xunit;

namespace VoxelGame.GUI.Tests;

[TestSubject(typeof(Visibilities))]
public sealed class VisibilitiesTests
{
    [Fact]
    public void Visibility_IsVisible_ShouldWork()
    {
        Assert.True(Visibility.Visible.IsVisible);
        Assert.False(Visibility.Hidden.IsVisible);
        Assert.False(Visibility.Collapsed.IsVisible);
    }

    [Fact]
    public void Visibility_IsLayouted_ShouldWork()
    {
        Assert.True(Visibility.Visible.IsLayouted);
        Assert.True(Visibility.Hidden.IsLayouted);
        Assert.False(Visibility.Collapsed.IsLayouted);
    }

    [Fact]
    public void Visibility_FromBoolean_ShouldWork()
    {
        Assert.Equal(Visibility.Visible, Visibilities.FromBoolean(true));
        Assert.Equal(Visibility.Collapsed, Visibilities.FromBoolean(false));
    }
}
