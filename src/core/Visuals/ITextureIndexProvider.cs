// <copyright file="ITextureIndexProvider.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals
{
    public interface ITextureIndexProvider
    {
        int GetTextureIndex(string name);
    }
}