// <copyright file="AtlasPosition.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;

namespace VoxelGame.Rendering
{
    public struct AtlasPosition
    {
        public float bottomLeftU { get; private set; }
        public float bottomLeftV { get; private set; }
        public float topRightU { get; private set; }
        public float topRightV { get; private set; }

        public AtlasPosition(float bottomLeftU, float bottomLeftV, float topRightU, float topRightV)
        {
            this.bottomLeftU = bottomLeftU;
            this.bottomLeftV = bottomLeftV;
            this.topRightU = topRightU;
            this.topRightV = topRightV;
        }
    }
}
