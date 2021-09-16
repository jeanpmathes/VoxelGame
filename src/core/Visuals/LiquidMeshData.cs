// <copyright file="LiquidMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals
{
    public sealed class LiquidMeshData
    {
        private LiquidMeshData(int textureIndex, TintColor tint)
        {
            TextureIndex = textureIndex;
            Tint = tint;
        }

        public int TextureIndex { get; }
        public TintColor Tint { get; }

        public static LiquidMeshData Empty { get; } = new(textureIndex: 0, TintColor.None);

        public static LiquidMeshData Basic(int textureIndex, TintColor tint)
        {
            return new(textureIndex, tint);
        }
    }
}