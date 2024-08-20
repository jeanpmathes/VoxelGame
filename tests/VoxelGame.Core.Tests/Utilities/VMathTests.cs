// <copyright file="VMathTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities;

[TestSubject(typeof(VMath))]
public class VMathTests
{
    [Fact]
    public void TestSelectByWeight()
    {
        const Int32 a = 0;
        const Int32 b = 1;
        const Int32 c = 2;
        const Int32 d = 3;

        Int32 selected = VMath.SelectByWeight(a, b, c, d, (0.0, 0.0));
        Assert.Equal(a, selected);

        selected = VMath.SelectByWeight(a, b, c, d, (1.0, 0.0));
        Assert.Equal(b, selected);

        selected = VMath.SelectByWeight(a, b, c, d, (0.0, 1.0));
        Assert.Equal(c, selected);

        selected = VMath.SelectByWeight(a, b, c, d, (1.0, 1.0));
        Assert.Equal(d, selected);
    }
}
