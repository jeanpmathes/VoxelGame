// <copyright file="Renderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using OpenTK;

namespace VoxelGame.Rendering
{
    public abstract class Renderer : IDisposable
    {
        public abstract void Draw(Vector3 position);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Renderer()
        {
            Dispose(false);
        }

        protected abstract void Dispose(bool disposing);
    }
}