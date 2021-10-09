// <copyright file="UserInterfaceControl.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Gwen.Net.Control;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    internal abstract class UserInterfaceControl : ControlBase
    {
        internal UserInterfaceControl(UserInterface userInterface) : base(userInterface.Root)
        {
            UI = userInterface;
        }

        public UserInterface UI { get; }

        protected FontHolder Fonts => UI.Fonts!;
    }
}