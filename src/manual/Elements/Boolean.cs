﻿// <copyright file="Boolean.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual.Elements
{
    /// <summary>
    ///     A piece of text that represents a boolean value.
    /// </summary>
    internal class Boolean : IElement
    {
        internal Boolean(bool value)
        {
            Value = value;
        }

        internal bool Value { get; }

        void IElement.Generate(StreamWriter writer)
        {
            if (Value) writer.Write(@" \Checkmark ");
            else writer.Write(@" \XSolidBrush ");
        }
    }
}