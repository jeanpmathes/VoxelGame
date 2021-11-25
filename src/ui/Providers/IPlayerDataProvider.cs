// <copyright file="IPlayerDataProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.UI.Providers
{
    public interface IPlayerDataProvider
    {
        public string Mode { get; }
        public string Selection { get; }
    }
}
