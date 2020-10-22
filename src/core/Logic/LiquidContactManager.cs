// <copyright file="LiquidContactManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Core.Logic
{
    public static class LiquidContactManager
    {
        public static bool HandleContact(Liquid a, Vector3i posA, Liquid b, Vector3i posB, bool isStaticB)
        {
            Console.WriteLine($"Contact between {a.NamedId} and {b.NamedId}.");

            return false;
        }
    }
}