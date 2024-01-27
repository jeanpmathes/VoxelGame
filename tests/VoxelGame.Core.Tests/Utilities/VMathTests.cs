// <copyright file="VMathTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities;
using Xunit;

namespace VoxelGame.Core.Tests.Utilities;

public class VMathTests
{
    [Fact]
    public void TestSelectByWeight()
    {
        const int a = 0;
        const int b = 1;
        const int c = 2;
        const int d = 3;

        int selected = VMath.SelectByWeight(a, b, c, d, (0.0, 0.0));
        Assert.Equal(a, selected);

        selected = VMath.SelectByWeight(a, b, c, d, (1.0, 0.0));
        Assert.Equal(b, selected);

        selected = VMath.SelectByWeight(a, b, c, d, (0.0, 1.0));
        Assert.Equal(c, selected);

        selected = VMath.SelectByWeight(a, b, c, d, (1.0, 1.0));
        Assert.Equal(d, selected);
    }
}
