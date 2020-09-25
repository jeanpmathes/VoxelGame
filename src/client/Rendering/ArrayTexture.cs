// <copyright file="TextureAtlas.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using System;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Rendering
{
    public abstract class ArrayTexture : IDisposable, ITextureIndexProvider
    {
        public abstract int Count { get; }

        public abstract void Use();

        internal abstract void SetWrapMode(TextureWrapMode mode);

        public abstract int GetTextureIndex(string name);

        #region IDisposalbe Support

        protected abstract void Dispose(bool disposing);

        ~ArrayTexture()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposalbe Support
    }
}