// <copyright file="ColorSTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using JetBrains.Annotations;
using VoxelGame.Core.Visuals;
using Xunit;

namespace VoxelGame.Core.Tests.Visuals;

[TestSubject(typeof(ColorS))]
public class ColorSTests
{
    [Fact]
    public void ColorS_Constructor_ShouldInitializeToZero()
    {
        ColorS color = new();

        Assert.Equal(expected: 0f, color.R);
        Assert.Equal(expected: 0f, color.G);
        Assert.Equal(expected: 0f, color.B);
        Assert.Equal(expected: 0f, color.A);
        Assert.False(color.IsNeutral);
    }

    [Fact]
    public void ColorS_Neutral_ShouldBeNeutral()
    {
        ColorS color = ColorS.Neutral;

        Assert.True(color.IsNeutral);
    }

    [Fact]
    public void ColorS_FromRGB_ShouldSetRGBWithDefaultAlpha()
    {
        ColorS color = ColorS.FromRGB(red: 0.5f, green: 0.6f, blue: 0.7f);

        Assert.Equal(expected: 0.5f, color.R);
        Assert.Equal(expected: 0.6f, color.G);
        Assert.Equal(expected: 0.7f, color.B);
        Assert.Equal(expected: 1.0f, color.A);
        Assert.False(color.IsNeutral);
    }

    [Fact]
    public void ColorS_FromRGBA_ShouldSetAllChannels()
    {
        ColorS color = ColorS.FromRGBA(red: 0.1f, green: 0.2f, blue: 0.3f, alpha: 0.4f);

        Assert.Equal(expected: 0.1f, color.R);
        Assert.Equal(expected: 0.2f, color.G);
        Assert.Equal(expected: 0.3f, color.B);
        Assert.Equal(expected: 0.4f, color.A);
        Assert.False(color.IsNeutral);
    }

    [Fact]
    public void ColorS_ShouldBeConvertibleToAndFromColor32()
    {
        ColorS original = ColorS.Amber;

        ColorS result = ColorS.FromColor32(original.ToColor32());

        Assert.Equal(original.R, result.R, tolerance: 0.01f);
        Assert.Equal(original.G, result.G, tolerance: 0.01f);
        Assert.Equal(original.B, result.B, tolerance: 0.01f);
        Assert.Equal(original.A, result.A, tolerance: 0.01f);
    }

    [Fact]
    public void ColorS_FromString_ValidHex_ShouldParseCorrectly()
    {
        ColorS? color = ColorS.FromString("#FF00FF");

        Assert.NotNull(color);
        Assert.Equal(expected: 1f, color.Value.R);
        Assert.Equal(expected: 0f, color.Value.G);
        Assert.Equal(expected: 1f, color.Value.B);
        Assert.Equal(expected: 1f, color.Value.A);
    }

    [Fact]
    public void ColorS_FromString_Invalid_ShouldReturnNull()
    {
        ColorS? color = ColorS.FromString("#X");

        Assert.Null(color);
    }

    [Fact]
    public void ColorS_Select_ShouldReturnCorrectColor()
    {
        ColorS neutralColor = ColorS.Neutral;
        ColorS defaultColor = ColorS.Red;

        Assert.Equal(defaultColor, neutralColor.Select(defaultColor));
        Assert.Equal(ColorS.Blue, ColorS.Blue.Select(defaultColor));
    }

    [Fact]
    public void ColorS_Scaling_ShouldMultiplyAllChannels()
    {
        ColorS color = ColorS.FromRGB(red: 0.5f, green: 0.6f, blue: 0.7f);
        ColorS result = color * 2f;

        Assert.Equal(expected: 1.0f, result.R);
        Assert.Equal(expected: 1.2f, result.G);
        Assert.Equal(expected: 1.4f, result.B);
        Assert.Equal(expected: 1.0f, result.A);
    }

    [Fact]
    public void ColorS_Multiplication_ShouldMultiplyAllChannels()
    {
        ColorS color1 = ColorS.FromRGB(red: 0.5f, green: 0.6f, blue: 0.7f);
        ColorS color2 = ColorS.FromRGB(red: 0.1f, green: 0.2f, blue: 0.3f);
        ColorS result = color1 * color2;

        Assert.Equal(expected: 0.05f, result.R, tolerance: 0.01f);
        Assert.Equal(expected: 0.12f, result.G, tolerance: 0.01f);
        Assert.Equal(expected: 0.21f, result.B, tolerance: 0.01f);
        Assert.Equal(expected: 1.0f, result.A, tolerance: 0.01f);
    }

    [Fact]
    public void ColorS_Addition_ShouldAddAllChannels()
    {
        ColorS color1 = ColorS.FromRGB(red: 0.5f, green: 0.6f, blue: 0.7f);
        ColorS color2 = ColorS.FromRGB(red: 0.1f, green: 0.2f, blue: 0.3f);
        ColorS result = color1 + color2;

        Assert.Equal(expected: 0.6f, result.R);
        Assert.Equal(expected: 0.8f, result.G);
        Assert.Equal(expected: 1.0f, result.B);
        Assert.Equal(expected: 1.0f, result.A);
    }

    [Fact]
    public void ColorS_Equals_ShouldCorrectlyCompareColors()
    {
        ColorS a = ColorS.Salmon;
        ColorS b = ColorS.Salmon;
        ColorS c = ColorS.Amber;

        Assert.True(a == b);
        Assert.False(a == c);
        Assert.True(a != c);
        Assert.False(a != b);
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }

    [Fact]
    public void ColorS_Equals_ShouldHandleNeutralColors()
    {
        ColorS neutral1 = ColorS.Neutral with {R = 0.5f};
        ColorS neutral2 = ColorS.Neutral with {G = 0.7f};

        Assert.True(neutral1 == neutral2);
        Assert.True(neutral1.Equals(neutral2));
        Assert.Equal(neutral1.GetHashCode(), neutral2.GetHashCode());
    }
}
