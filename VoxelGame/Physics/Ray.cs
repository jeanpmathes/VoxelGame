// <copyright file="Ray.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;

namespace VoxelGame.Physics
{
    public struct Ray
    {
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Length { get; }

        public Ray(Vector3 origin, Vector3 direction, float length)
        {
            Origin = origin;
            Direction = direction.Normalized();
            Length = length;
        }

        public Vector3 EndPoint
        {
            get
            {
                return Origin + Direction * Length;
            }
        }
    }
}