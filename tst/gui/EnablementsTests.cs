// <copyright file="EnablementsTests.cs" company="VoxelGame">
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

using Xunit;

namespace VoxelGame.GUI.Tests;

public class EnablementsTests
{
    [Fact]
    public void Enablement_IsEnabled_ShouldWork()
    {
        Assert.True(Enablement.Enabled.IsEnabled);
        Assert.False(Enablement.ReadOnly.IsEnabled);
        Assert.False(Enablement.Disabled.IsEnabled);
    }

    [Fact]
    public void Enablement_IsFocusable_ShouldWork()
    {
        Assert.True(Enablement.Enabled.IsFocusable);
        Assert.True(Enablement.ReadOnly.IsFocusable);
        Assert.False(Enablement.Disabled.IsFocusable);
    }

    [Fact]
    public void Enablement_CanReceiveInput_ShouldWork()
    {
        Assert.True(Enablement.Enabled.CanReceiveInput);
        Assert.True(Enablement.ReadOnly.CanReceiveInput);
        Assert.False(Enablement.Disabled.CanReceiveInput);
    }

    [Fact]
    public void Enablement_FromBoolean_ShouldWork()
    {
        Assert.Equal(Enablement.Enabled, Enablements.FromBoolean(true));
        Assert.Equal(Enablement.Disabled, Enablements.FromBoolean(false));
    }

    [Fact]
    public void Enablement_IsDisabled_ShouldWork()
    {
        Assert.False(Enablement.Enabled.IsDisabled);
        Assert.False(Enablement.ReadOnly.IsDisabled);
        Assert.True(Enablement.Disabled.IsDisabled);
    }
}
