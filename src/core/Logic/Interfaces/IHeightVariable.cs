// <copyright file="IHeightVariable.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic.Interfaces
{
    public interface IHeightVariable
    {
        public const int MaximumHeight = 15;

        int GetHeight(uint data);
    }
}