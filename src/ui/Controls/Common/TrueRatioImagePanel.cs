// <copyright file="TrueRatioImagePanel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     An image panel that keeps the ratio of the image, by scaling and cutting off parts of the image.
/// </summary>
internal sealed class TrueRatioImagePanel : ControlBase
{
    private readonly ImagePanel imagePanel;

    internal TrueRatioImagePanel(ControlBase parent) : base(parent)
    {
        imagePanel = new ImagePanel(this)
        {
            Dock = Dock.Fill
        };

        FitImage();
    }

    internal String ImageName
    {
        get => imagePanel.ImageName;
        set
        {
            imagePanel.ImageName = value;
            FitImage();
        }
    }

    protected override void OnBoundsChanged(Rectangle oldBounds)
    {
        FitImage();
    }

    protected override void OnScaleChanged()
    {
        FitImage();
    }

    private void FitImage()
    {
        imagePanel.SetUV(u1: 0, v1: 0, u2: 1, v2: 1);

        Size imageSize = imagePanel.TextureRect.Size;
        Size availableSize = Bounds.Size;

        if (imageSize == Size.Zero || availableSize == Size.Zero) return;

        Single desiredRatio = imageSize.Width / (Single) imageSize.Height;
        Single availableRatio = availableSize.Width / (Single) availableSize.Height;

        Single fittedWidth;
        Single fittedHeight;

        if (desiredRatio > availableRatio)
        {
            fittedHeight = imageSize.Height;
            fittedWidth = imageSize.Height * availableRatio;
        }
        else
        {
            fittedWidth = imageSize.Width;
            fittedHeight = imageSize.Width / availableRatio;
        }

        var fittedSize = new Size((Int32) Math.Ceiling(fittedWidth), (Int32) Math.Ceiling(fittedHeight));

        Int32 offsetX = imageSize.Width - fittedSize.Width;
        Int32 offsetY = imageSize.Height - fittedSize.Height;

        imagePanel.TextureRect = new Rectangle(offsetX, offsetY, fittedSize);
    }
}
