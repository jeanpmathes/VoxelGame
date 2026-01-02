// <copyright file="ImageTests.cs" company="VoxelGame">
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
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using Xunit;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Core.Tests.Visuals;

[TestSubject(typeof(Image))]
public class ImageTests
{
    [Fact]
    public void Image_Constructor_ShouldCopyContent()
    {
        Int32[] content = [-1, -1, -1, -1];

        Image image = new(content, Image.Format.RGBA, width: 2, height: 2);

        Assert.Equal(expected: 2, image.Width);
        Assert.Equal(expected: 2, image.Height);
        Assert.Equal(ColorS.White, image.GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.White, image.GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.White, image.GetPixel(x: 0, y: 1).ToColorS());
        Assert.Equal(ColorS.White, image.GetPixel(x: 1, y: 1).ToColorS());
    }

    [Fact]
    public void Image_Constructor_ShouldFillEmptyWithZero()
    {
        ColorS zero = ColorS.Black with {A = 0.0f};
        Image image = new(width: 1, height: 1);

        Assert.Equal(expected: 1, image.Width);
        Assert.Equal(expected: 1, image.Height);
        Assert.Equal(zero, image.GetPixel(x: 0, y: 0).ToColorS());
        Assert.True(image.IsEmpty());
    }

    [Fact]
    public void Image_Constructor_ShouldPreserveBitmapContent()
    {
        using Bitmap bitmap = new(width: 2, height: 2);
        bitmap.SetPixel(x: 0, y: 0, Color.White);
        bitmap.SetPixel(x: 1, y: 0, Color.Black);
        bitmap.SetPixel(x: 0, y: 1, Color.Red);
        bitmap.SetPixel(x: 1, y: 1, Color.Blue);

        Image image = new(bitmap);

        Assert.Equal(expected: 2, image.Width);
        Assert.Equal(expected: 2, image.Height);
        Assert.Equal(ColorS.White, image.GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.Black, image.GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.Red, image.GetPixel(x: 0, y: 1).ToColorS());
        Assert.Equal(ColorS.Blue, image.GetPixel(x: 1, y: 1).ToColorS());
    }

    [Fact]
    public void Image_Size_ShouldMatchWidthAndHeight()
    {
        Image image = new(width: 15, height: 31);

        Assert.Equal(expected: 15, image.Width);
        Assert.Equal(expected: 31, image.Height);
        Assert.Equal((15, 31), image.Size);
    }

    [Fact]
    public void Image_CreateCopy_ShouldCopyFullSourceIfNoAreaSpecified()
    {
        Image source = new(width: 2, height: 2);
        source.SetPixel(x: 0, y: 0, ColorS.White);
        source.SetPixel(x: 1, y: 0, ColorS.Black);
        source.SetPixel(x: 0, y: 1, ColorS.Red);
        source.SetPixel(x: 1, y: 1, ColorS.Blue);

        Image copy = source.CreateCopy();

        Assert.Equal(expected: 2, copy.Width);
        Assert.Equal(expected: 2, copy.Height);
        Assert.Equal(ColorS.White, copy.GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.Black, copy.GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.Red, copy.GetPixel(x: 0, y: 1).ToColorS());
        Assert.Equal(ColorS.Blue, copy.GetPixel(x: 1, y: 1).ToColorS());
    }

    [Fact]
    public void Image_CreateCopy_ShouldOnlyCopySpecifiedArea()
    {
        Image source = new(width: 2, height: 2);
        source.SetPixel(x: 0, y: 0, ColorS.White);
        source.SetPixel(x: 1, y: 0, ColorS.Black);
        source.SetPixel(x: 0, y: 1, ColorS.Red);
        source.SetPixel(x: 1, y: 1, ColorS.Blue);

        Image copy = source.CreateCopy(new Box2i(minX: 0, minY: 0, maxX: 0, maxY: 0));

        Assert.Equal(expected: 1, copy.Width);
        Assert.Equal(expected: 1, copy.Height);
        Assert.Equal(ColorS.White, copy.GetPixel(x: 0, y: 0).ToColorS());
    }

    [Fact]
    public void Image_CreateFallback_ShouldHaveSpecifiedSize()
    {
        var fallback = Image.CreateFallback(size: 13);

        Assert.Equal(expected: 13, fallback.Width);
        Assert.Equal(expected: 13, fallback.Height);
    }

    [Fact]
    public void Image_CalculateAverage_ShouldReturnAverageColor()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.White);
        image.SetPixel(x: 1, y: 0, ColorS.Black);
        image.SetPixel(x: 0, y: 1, ColorS.White);
        image.SetPixel(x: 1, y: 1, ColorS.Black);

        Color32 average = image.CalculateAverage();

        Assert.Equal(expected: 127, average.R);
        Assert.Equal(expected: 127, average.G);
        Assert.Equal(expected: 127, average.B);
        Assert.Equal(expected: 255, average.A);
    }

    [Fact]
    public void Image_CalculateAverage_ShouldIgnoreFullyTransparentPixels()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.White);
        image.SetPixel(x: 1, y: 0, ColorS.Black with {A = 0.0f});
        image.SetPixel(x: 0, y: 1, ColorS.White);
        image.SetPixel(x: 1, y: 1, ColorS.Black with {A = 0.0f});

        Color32 average = image.CalculateAverage();

        Assert.Equal(Byte.MaxValue, average.R);
        Assert.Equal(Byte.MaxValue, average.G);
        Assert.Equal(Byte.MaxValue, average.B);
        Assert.Equal(Byte.MaxValue, average.A);
    }

    [Fact]
    public void Image_RecolorTransparency_ShouldNotChangeOpaquePixels()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.White);
        image.SetPixel(x: 1, y: 0, ColorS.Black with {A = 0.0f});
        image.SetPixel(x: 0, y: 1, ColorS.White);
        image.SetPixel(x: 1, y: 1, ColorS.Black with {A = 0.0f});

        image.RecolorTransparency();

        Assert.Equal(ColorS.White, image.GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.White, image.GetPixel(x: 0, y: 1).ToColorS());
    }

    [Fact]
    public void Image_RecolorTransparency_ShouldChangeTransparentPixels()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.Red);
        image.SetPixel(x: 1, y: 0, ColorS.Black with {A = 0.0f});
        image.SetPixel(x: 0, y: 1, ColorS.Red);
        image.SetPixel(x: 1, y: 1, ColorS.Black with {A = 0.0f});

        image.RecolorTransparency();

        Assert.Equal(ColorS.Red with {A = 0.0f}, image.GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.Red with {A = 0.0f}, image.GetPixel(x: 1, y: 1).ToColorS());
    }

    [Fact]
    public void Image_Translated_ShouldReturnCopyForZeroTranslation()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.Red);
        image.SetPixel(x: 1, y: 0, ColorS.Green);
        image.SetPixel(x: 0, y: 1, ColorS.Blue);
        image.SetPixel(x: 1, y: 1, ColorS.White);

        Image translated = image.Translated(dx: 0, dy: 0);

        Assert.NotSame(image, translated);
        Assert.Equal(expected: 2, translated.Width);
        Assert.Equal(expected: 2, translated.Height);
        Assert.Equal(ColorS.Red, translated.GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.Green, translated.GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.Blue, translated.GetPixel(x: 0, y: 1).ToColorS());
        Assert.Equal(ColorS.White, translated.GetPixel(x: 1, y: 1).ToColorS());
    }

    [Fact]
    public void Image_Translated_ShouldShiftAllPixels()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.Red);
        image.SetPixel(x: 1, y: 0, ColorS.Green);
        image.SetPixel(x: 0, y: 1, ColorS.Blue);
        image.SetPixel(x: 1, y: 1, ColorS.White);

        Image translated = image.Translated(dx: 1, dy: 1);

        Assert.NotSame(image, translated);
        Assert.Equal(expected: 2, translated.Width);
        Assert.Equal(expected: 2, translated.Height);
        Assert.Equal(ColorS.White, translated.GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.Blue, translated.GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.Green, translated.GetPixel(x: 0, y: 1).ToColorS());
        Assert.Equal(ColorS.Red, translated.GetPixel(x: 1, y: 1).ToColorS());
    }

    [Fact]
    public void Image_GetData_ShouldPreserveInitialContent()
    {
        Int32[] data = [1, 2, 3, 4];

        Image image = new(data, Image.Format.RGBA, width: 2, height: 2);
        Int32[] retrieved = image.GetData(Image.Format.RGBA);

        Assert.Equal(data, retrieved);
        Assert.NotSame(data, retrieved);
    }

    [Fact]
    public void Image_GenerateMipmaps_WithoutTransparency_ShouldNotMixTransparentAndOpaqueColors()
    {
        Image image = new(width: 4, height: 4);

        var counter = 0;

        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
        {
            image.SetPixel(x, y, counter % 2 == 0 ? ColorS.White : ColorS.Black with {A = 0.0f});
            counter += 1;
        }

        Image[] mipmaps = image.GenerateMipmaps(levels: 3, Image.MipmapAlgorithm.AveragingWithoutTransparency).ToArray();

        Assert.Equal(expected: 2, mipmaps.Length);

        Assert.Equal(expected: 2, mipmaps[0].Width);
        Assert.Equal(expected: 2, mipmaps[0].Height);
        Assert.Equal(ColorS.White, mipmaps[0].GetPixel(x: 0, y: 0).ToColorS());
        Assert.Equal(ColorS.White, mipmaps[0].GetPixel(x: 1, y: 0).ToColorS());
        Assert.Equal(ColorS.White, mipmaps[0].GetPixel(x: 0, y: 1).ToColorS());
        Assert.Equal(ColorS.White, mipmaps[0].GetPixel(x: 1, y: 1).ToColorS());

        Assert.Equal(expected: 1, mipmaps[1].Width);
        Assert.Equal(expected: 1, mipmaps[1].Height);
        Assert.Equal(ColorS.White, mipmaps[1].GetPixel(x: 0, y: 0).ToColorS());
    }

    [Fact]
    public void Image_GenerateMipmaps_WithTransparency_ShouldMixAllColors()
    {
        Image image = new(width: 2, height: 2);
        image.SetPixel(x: 0, y: 0, ColorS.White);
        image.SetPixel(x: 1, y: 0, ColorS.Black with {A = 0.5f});
        image.SetPixel(x: 0, y: 1, ColorS.White);
        image.SetPixel(x: 1, y: 1, ColorS.Black with {A = 0.5f});

        Image[] mipmaps = image.GenerateMipmaps(levels: 2, Image.MipmapAlgorithm.AveragingWithTransparency).ToArray();

        Assert.Single(mipmaps);

        Assert.Equal(expected: 1, mipmaps[0].Width);
        Assert.Equal(expected: 1, mipmaps[0].Height);
        Assert.Equal(expected: 191, mipmaps[0].GetPixel(x: 0, y: 0).A);
        Assert.Equal(expected: 170, mipmaps[0].GetPixel(x: 0, y: 0).R);
        Assert.Equal(expected: 170, mipmaps[0].GetPixel(x: 0, y: 0).G);
        Assert.Equal(expected: 170, mipmaps[0].GetPixel(x: 0, y: 0).B);
    }
}
