// <copyright file="Setting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net.Control;

namespace VoxelGame.UI.Providers.Settings
{
    public abstract class Setting
    {
        public abstract string Name { get; }
        internal abstract ControlBase CreateControl(ControlBase parent);
    }
}