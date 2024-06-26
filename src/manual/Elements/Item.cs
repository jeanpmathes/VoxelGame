﻿// <copyright file="Item.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     An item for lists.
/// </summary>
internal class Item : IElement
{
    private readonly String? bullet;

    internal Item(String? bullet)
    {
        this.bullet = bullet;
    }

    void IElement.Generate(StreamWriter writer)
    {
        writer.Write(@"\item");
        if (bullet != null) writer.Write($"[{bullet}]");
        writer.Write(" ");
    }
}
