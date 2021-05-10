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