// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.UserInterfaces
{
    internal class Context
    {
        internal Context(FontHolder fonts, InputListener input)
        {
            Fonts = fonts;
            Input = input;
        }

        internal FontHolder Fonts { get; }
        internal InputListener Input { get; }
    }
}