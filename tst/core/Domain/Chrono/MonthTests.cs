// <copyright file="MonthTests.cs" company="VoxelGame">
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
using System.Linq;
using JetBrains.Annotations;
using VoxelGame.Core.Domain.Chrono;
using Xunit;

namespace VoxelGame.Core.Tests.Domain.Chrono;

[TestSubject(typeof(Month))]
public class MonthTests
{
    [Fact]
    public void Month_ToNumber_ShouldBeOneBasedEnumValue()
    {
        foreach (Month month in Enum.GetValues<Month>())
        {
            Int32 expected = (Int32) month + 1;
            Assert.Equal(expected, month.ToNumber());
        }
    }

    [Fact]
    public void Months_FromNumber_ShouldInvertToNumber()
    {
        foreach (Month month in Enum.GetValues<Month>())
        {
            Int32 number = month.ToNumber();
            Assert.Equal(month, Months.FromNumber(number));
        }
    }

    [Fact]
    public void Month_Next_ShouldAdvanceWithWrap()
    {
        foreach (Month month in Enum.GetValues<Month>())
        {
            var current = (Int32) month;
            Int32 expectedValue = (current + 1) % Calendar.MonthsPerYear;

            var expected = (Month) expectedValue;
            Assert.Equal(expected, month.Next());
        }
    }

    [Fact]
    public void Month_Previous_ShouldDecrementWithWrap()
    {
        foreach (Month month in Enum.GetValues<Month>())
        {
            var current = (Int32) month;
            Int32 expectedValue = (current + Calendar.MonthsPerYear - 1) % Calendar.MonthsPerYear;

            var expected = (Month) expectedValue;
            Assert.Equal(expected, month.Previous());
        }
    }

    [Fact]
    public void Month_NextAndPrevious_ShouldBeInverse()
    {
        foreach (Month month in Enum.GetValues<Month>())
        {
            Assert.Equal(month, month.Next().Previous());
            Assert.Equal(month, month.Previous().Next());
        }
    }

    [Fact]
    public void Month_Next_ShouldCycleAfterAYear()
    {
        foreach (Month start in Enum.GetValues<Month>())
        {
            Month month = start;

            for (var index = 0; index < Calendar.MonthsPerYear; index++)
            {
                month = month.Next();
            }

            Assert.Equal(start, month);
        }
    }

    [Fact]
    public void Months_All_ShouldContainAllMonthsExactlyOnce()
    {
        Month[] expected = Enum.GetValues<Month>();
        Month[] actual = Months.All.ToArray();

        Assert.Equal(expected.Length, actual.Length);

        foreach (Month month in expected)
        {
            Assert.Contains(month, actual);
        }
    }
}
