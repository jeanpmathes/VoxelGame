// <copyright file="Repeat.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using JetBrains.Annotations;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures.Modifiers;

/// <summary>
///     Repeats each image of a sheet the specified amount of times.
///     Makes more sense to be applied to single images.
/// </summary>
[UsedImplicitly]
public class Repeat() : Modifier("repeat", [xParameter, yParameter])
{
    private static readonly Parameter<Int32> xParameter = CreateIntegerParameter("x", fallback: 1);
    private static readonly Parameter<Int32> yParameter = CreateIntegerParameter("y", fallback: 1);

    /// <inheritdoc />
    protected override Sheet Modify(Image image, Parameters parameters, IContext context)
    {
        Int32 xRepeat = parameters.Get(xParameter);
        Int32 yRepeat = parameters.Get(yParameter);

        if (xRepeat < 1 || yRepeat < 1)
            context.ReportWarning("Repeat parameters are not positive");

        if (xRepeat > Byte.MaxValue || yRepeat > Byte.MaxValue)
            context.ReportWarning("Repeat parameters exceed maximum value");

        Sheet result = new((Byte) xRepeat, (Byte) yRepeat);

        for (Byte x = 0; x < xRepeat; x++)
        for (Byte y = 0; y < yRepeat; y++)
            result[x, y] = image.CreateCopy();

        return result;
    }
}
