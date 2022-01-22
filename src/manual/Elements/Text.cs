﻿// <copyright file="Text.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual.Elements
{
    /// <summary>
    ///     A simple text element.
    /// </summary>
    internal class Text : IElement
    {
        public Text(string text)
        {
            Content = text;
        }

        private string Content { get; }

        void IElement.Generate(StreamWriter writer)
        {
            writer.WriteLine(Content);
        }
    }
}
