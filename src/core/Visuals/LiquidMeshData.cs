// <copyright file="LiquidMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals
{
    public class LiquidMeshData
    {
        public int TextureIndex { get; }
        public TintColor Tint { get; }

        public LiquidMeshData(int textureIndex)
            : this(textureIndex, TintColor.None) { }

        public LiquidMeshData(int textureIndex, TintColor tint)
        {
            TextureIndex = textureIndex;
            Tint = tint;
        }

        public static LiquidMeshData Empty { get; } = new LiquidMeshData(0, TintColor.None);
    }
}