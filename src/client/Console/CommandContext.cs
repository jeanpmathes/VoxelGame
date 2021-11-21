// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;

namespace VoxelGame.Client.Console
{
    public record CommandContext(ConsoleWrapper Console, Player Player);
}
