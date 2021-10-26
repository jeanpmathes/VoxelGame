// <copyright file="SettingChangedArgs.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Client.Application
{
    public record SettingChangedArgs<T>(T OldValue, T NewValue);
}