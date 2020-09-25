// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Diagnostics;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// A renderer for <see cref="Logic.Section"/>.
    /// </summary>
    public abstract class SectionRenderer : Renderer
    {
        public abstract void SetData(ref SectionMeshData meshData);

        public abstract void PrepareStage(int stage);

        public abstract void DrawStage(int stage, Vector3 position);

        public abstract void FinishStage(int stage);
    }
}