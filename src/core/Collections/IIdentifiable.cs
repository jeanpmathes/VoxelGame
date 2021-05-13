// <copyright file="IIdentifiable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelGame.Core.Collections
{
    public interface IIdentifiable<out T>
    {
        T Id { get; }
    }
}