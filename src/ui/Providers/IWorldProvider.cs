// <copyright file="IWorldProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.UI.Providers
{
    public interface IWorldProvider
    {
        IEnumerable<(WorldInformation info, string path)> Worlds { get; }

        void Refresh();

        void LoadWorld(WorldInformation information, string path);

        void CreateWorld(string name);

        bool IsWorldNameValid(string name);
    }
}