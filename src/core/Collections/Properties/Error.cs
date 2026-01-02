// <copyright file="Error.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A property that represents an error or warning.
/// </summary>
public sealed class Error : Property
{
    /// <summary>
    ///     Creates a new error with the given name and message.
    /// </summary>
    public Error(String name, String message, Boolean isCritical) : base(name)
    {
        Message = message;
        IsCritical = isCritical;
    }

    /// <summary>
    ///     Gets the message of the error.
    /// </summary>
    public String Message { get; }

    /// <summary>
    ///     Whether the error is critical or just a warning.
    /// </summary>
    public Boolean IsCritical { get; }

    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}
