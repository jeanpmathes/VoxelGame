// <copyright file="Color32Tests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using Xunit;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Core.Tests.Visuals;

[TestSubject(typeof(Color32))]
public class Color32Tests
{
    [Fact]
    public void Color32_FromInt32_ShouldConvertZeroRGBA()
    {
        Color32 color = Color32.FromInt32(value: 0, Image.Format.RGBA);

        Assert.Equal(expected: 0x00, color.R);
        Assert.Equal(expected: 0x00, color.G);
        Assert.Equal(expected: 0x00, color.B);
        Assert.Equal(expected: 0x00, color.A);
    }

    [Fact]
    public void Color32_FromInt32_ShouldConvertOneRGBA()
    {
        Color32 color = Color32.FromInt32(value: -1, Image.Format.RGBA);

        Assert.Equal(expected: 0xFF, color.R);
        Assert.Equal(expected: 0xFF, color.G);
        Assert.Equal(expected: 0xFF, color.B);
        Assert.Equal(expected: 0xFF, color.A);
    }

    [Fact]
    public void Color32_FromInt32_ShouldConvertZeroBGRA()
    {
        Color32 color = Color32.FromInt32(value: 0, Image.Format.BGRA);

        Assert.Equal(expected: 0x00, color.R);
        Assert.Equal(expected: 0x00, color.G);
        Assert.Equal(expected: 0x00, color.B);
        Assert.Equal(expected: 0x00, color.A);
    }

    [Fact]
    public void Color32_FromInt32_ShouldConvertOneBGRA()
    {
        Color32 color = Color32.FromInt32(value: -1, Image.Format.BGRA);

        Assert.Equal(expected: 0xFF, color.R);
        Assert.Equal(expected: 0xFF, color.G);
        Assert.Equal(expected: 0xFF, color.B);
        Assert.Equal(expected: 0xFF, color.A);
    }

    [Fact]
    public void Color32_ToInt32_ShouldBeReversible()
    {
        const Int32 value = 0x12345678;

        var v1 = Color32.FromInt32(value, Image.Format.RGBA).ToInt32(Image.Format.RGBA);
        var v2 = Color32.FromInt32(value, Image.Format.BGRA).ToInt32(Image.Format.BGRA);

        Assert.Equal(value, v1);
        Assert.Equal(value, v2);
    }

    [Fact]
    public void Color32_FromColorS_ShouldConvertNegativeValuesToZero()
    {
        ColorS negative = ColorS.FromRGBA(red: -1, green: -1, blue: -1, alpha: -1);

        Color32 color = Color32.FromColorS(negative);

        Assert.Equal(expected: 0x00, color.R);
        Assert.Equal(expected: 0x00, color.G);
        Assert.Equal(expected: 0x00, color.B);
        Assert.Equal(expected: 0x00, color.A);
    }

    [Fact]
    public void Color32_FromColorS_ShouldConvertOnesToByteMax()
    {
        ColorS max = ColorS.FromRGBA(red: 1, green: 1, blue: 1, alpha: 1);

        Color32 color = Color32.FromColorS(max);

        Assert.Equal(expected: 0xFF, color.R);
        Assert.Equal(expected: 0xFF, color.G);
        Assert.Equal(expected: 0xFF, color.B);
        Assert.Equal(expected: 0xFF, color.A);
    }

    [Fact]
    public void Color32_FromColorS_ShouldConvertOutOfRangeValuesToByteMax()
    {
        ColorS over = ColorS.FromRGBA(red: 2, green: 2, blue: 2, alpha: 2);

        Color32 color = Color32.FromColorS(over);

        Assert.Equal(expected: 0xFF, color.R);
        Assert.Equal(expected: 0xFF, color.G);
        Assert.Equal(expected: 0xFF, color.B);
        Assert.Equal(expected: 0xFF, color.A);
    }

    [Fact]
    public void Color32_FromRGBA_ShouldHandleOutOfRangeValues()
    {
        Vector4i outOfRange = new(Int32.MaxValue);

        Color32 color = Color32.FromRGBA(outOfRange);

        Assert.Equal(expected: 0xFF, color.R);
        Assert.Equal(expected: 0xFF, color.G);
        Assert.Equal(expected: 0xFF, color.B);
        Assert.Equal(expected: 0xFF, color.A);
    }

    [Fact]
    public void Color32_FromRGBA_ShouldBeEquivalentToUsingSetters()
    {
        const Byte r = 0x12;
        const Byte g = 0x34;
        const Byte b = 0x56;
        const Byte a = 0x78;

        Color32 color1 = Color32.FromRGBA(r, g, b, a);

        Color32 color2 = new()
        {
            R = r,
            G = g,
            B = b,
            A = a
        };

        Assert.Equal(color1, color2);
    }

    [Fact]
    public void Color32_FromColor_ShouldPreserveValues()
    {
        Color color = Color.Fuchsia;

        Color32 color32 = Color32.FromColor(color);

        Assert.Equal(color.R, color32.R);
        Assert.Equal(color.G, color32.G);
        Assert.Equal(color.B, color32.B);
        Assert.Equal(color.A, color32.A);
    }

    [Fact]
    public void Color32_ToColorS_ShouldConvertMinValueToZero()
    {
        Color32 color = new()
        {
            R = 0x00,
            G = 0x00,
            B = 0x00,
            A = 0x00
        };

        var colorS = color.ToColorS();

        Assert.Equal(expected: 0, colorS.R);
        Assert.Equal(expected: 0, colorS.G);
        Assert.Equal(expected: 0, colorS.B);
        Assert.Equal(expected: 0, colorS.A);
    }

    [Fact]
    public void Color32_ToColorS_ShouldConvertMaxValueToOne()
    {
        Color32 color = new()
        {
            R = 0xFF,
            G = 0xFF,
            B = 0xFF,
            A = 0xFF
        };

        var colorS = color.ToColorS();

        Assert.Equal(expected: 1, colorS.R);
        Assert.Equal(expected: 1, colorS.G);
        Assert.Equal(expected: 1, colorS.B);
        Assert.Equal(expected: 1, colorS.A);
    }

    [Fact]
    public void Color32_ToVector4i_ShouldPreserveValues()
    {
        Color32 color = new()
        {
            R = 0x12,
            G = 0x34,
            B = 0x56,
            A = 0x78
        };

        var vector = color.ToVector4i();

        Assert.Equal(expected: 0x12, vector.X);
        Assert.Equal(expected: 0x34, vector.Y);
        Assert.Equal(expected: 0x56, vector.Z);
        Assert.Equal(expected: 0x78, vector.W);
    }

    [Fact]
    public void Color32_FromVector4i_ShouldClampValues()
    {
        Vector4i vector = new(Int32.MaxValue, Int32.MinValue, Int32.MaxValue, Int32.MinValue);

        Color32 color = Color32.FromVector4i(vector);

        Assert.Equal(expected: 0xFF, color.R);
        Assert.Equal(expected: 0x00, color.G);
        Assert.Equal(expected: 0xFF, color.B);
        Assert.Equal(expected: 0x00, color.A);
    }

    [Fact]
    public void Color32_ShouldPreserveValuesAfterSetting()
    {
        Color32 color = new()
        {
            R = 0x00,
            G = 0x00,
            B = 0x00,
            A = 0x00
        };

        color.R = 0x12;
        color.G = 0x34;
        color.B = 0x56;
        color.A = 0x78;

        Assert.Equal(expected: 0x12, color.R);
        Assert.Equal(expected: 0x34, color.G);
        Assert.Equal(expected: 0x56, color.B);
        Assert.Equal(expected: 0x78, color.A);
    }

    [Fact]
    public void Color32_Equals_ShouldReturnTrueForEqualColors()
    {
        Color32 color1 = new()
        {
            R = 0x12,
            G = 0x34,
            B = 0x56,
            A = 0x78
        };

        Color32 color2 = new()
        {
            R = 0x12,
            G = 0x34,
            B = 0x56,
            A = 0x78
        };

        Assert.Equal(color1, color2);
        Assert.True(color1.Equals(color2));
        Assert.True(color1 == color2);
        Assert.False(color1 != color2);
    }

    [Fact]
    public void Color32_Equals_ShouldReturnFalseForDifferentColors()
    {
        Color32 color1 = new()
        {
            R = 0x12,
            G = 0x34,
            B = 0x56,
            A = 0x78
        };

        Color32 color2 = new()
        {
            R = 0x12,
            G = 0x34,
            B = 0x56,
            A = 0x79
        };

        Assert.NotEqual(color1, color2);
        Assert.False(color1.Equals(color2));
        Assert.False(color1 == color2);
        Assert.True(color1 != color2);
    }
}
