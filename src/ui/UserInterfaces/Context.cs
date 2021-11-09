// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Input;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.UserInterfaces
{
    internal sealed class Context : IDisposable
    {
        internal Context(FontHolder fonts, InputListener input)
        {
            Fonts = fonts;
            Input = input;
        }

        internal FontHolder Fonts { get; }
        internal InputListener Input { get; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) Fonts.Dispose();
        }

        ~Context()
        {
            Dispose(disposing: false);
        }
    }
}