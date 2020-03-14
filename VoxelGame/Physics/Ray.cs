// <copyright file="Ray.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;

namespace VoxelGame.Physics
{
    public struct Ray
    {
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Lenght { get; }

        public Ray(Vector3 origin, Vector3 direction, float lenght)
        {
            Origin = origin;
            Direction = direction.Normalized();
            Lenght = lenght;
        }

        public Vector3 EndPoint
        {
            get
            {
                return Origin + Direction * Lenght;
            }
        }
    }
}
