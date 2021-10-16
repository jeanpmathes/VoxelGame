// <copyright file="FontHolder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net;
using Gwen.Net.Skin;

namespace VoxelGame.UI.Utility
{
    public class FontHolder
    {
        private const string FontName = "Arial";

        private readonly SkinBase skin;

        internal FontHolder(SkinBase skin)
        {
            this.skin = skin;
            skin.DefaultFont.Size = 15;

            Title = Font.Create(skin.Renderer, FontName, size: 30);
            Subtitle = Font.Create(skin.Renderer, FontName, size: 10);
            Small = Font.Create(skin.Renderer, FontName, size: 12);
            Path = Font.Create(skin.Renderer, FontName, size: 10, FontStyle.Italic);
        }

        public Font Default => skin.DefaultFont;

        public Font Title { get; }
        public Font Subtitle { get; }
        public Font Small { get; }
        public Font Path { get; }
    }
}