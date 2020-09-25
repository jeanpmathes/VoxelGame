// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// A renderer that renders instances of the <see cref="BoundingBox"/> struct.
    /// </summary>
    public abstract class BoxRenderer : Renderer
    {
        public abstract void SetBoundingBox(BoundingBox boundingBox);
    }
}