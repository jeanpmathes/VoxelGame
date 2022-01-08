// <copyright file="TrueRatioImagePanel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;

namespace VoxelGame.UI.Controls
{
    /// <summary>
    ///     An image panel that keeps the ratio of the image, by scaling and cutting off parts of the image.
    /// </summary>
    internal class TrueRatioImagePanel : ControlBase
    {
        private readonly ImagePanel imagePanel;

        public TrueRatioImagePanel(ControlBase parent) : base(parent)
        {
            imagePanel = new ImagePanel(this);
            FitImage();
        }

        public string ImageName
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

        private void FitImage()
        {
            imagePanel.SetUV(u1: 0, v1: 0, u2: 1, v2: 1);

            Size imageSize = imagePanel.TextureRect.Size;
            Size availableSize = Bounds.Size;

            if (imageSize == Size.Zero || availableSize == Size.Zero) return;

            float desiredRatio = imageSize.Width / (float) imageSize.Height;
            float availableRatio = availableSize.Width / (float) availableSize.Height;

            float fittedWidth, fittedHeight;

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

            var fittedSize = new Size((int) Math.Ceiling(fittedWidth), (int) Math.Round(fittedHeight));
            int offset = imageSize.Height - fittedSize.Height;

            imagePanel.TextureRect = new Rectangle(x: 0, offset, fittedSize);
        }
    }
}