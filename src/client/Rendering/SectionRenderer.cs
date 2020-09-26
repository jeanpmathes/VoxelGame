// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System.Diagnostics;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// A renderer for <see cref="Logic.Section"/>.
    /// </summary>
    public abstract class SectionRenderer : Renderer
    {
        private protected bool disposed;

        public abstract void SetData(ref SectionMeshData meshData);

        public void PrepareStage(int stage)
        {
            Matrix4 view = Client.Player.GetViewMatrix();
            Matrix4 projection = Client.Player.GetProjectionMatrix();

            switch (stage)
            {
                case 0: PrepareSimpleBuffer(view, projection); break;
                case 1: PrepareComplexBuffer(view, projection); break;
                case 2: PrepareLiquidBuffer(view, projection); break;
            }
        }

        protected abstract void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection);

        protected abstract void PrepareComplexBuffer(Matrix4 view, Matrix4 projection);

        protected abstract void PrepareLiquidBuffer(Matrix4 view, Matrix4 projection);

        public void DrawStage(int stage, Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);

            switch (stage)
            {
                case 0: DrawSimpleBuffer(model); break;
                case 1: DrawComplexBuffer(model); break;
                case 2: DrawLiquidBuffer(model); break;
            }
        }

        protected abstract void DrawSimpleBuffer(Matrix4 model);

        protected abstract void DrawComplexBuffer(Matrix4 model);

        protected abstract void DrawLiquidBuffer(Matrix4 model);

        public void FinishStage(int stage)
        {
            switch (stage)
            {
                // case 2:
                // case 1:
                case 2: FinishLiqduiBuffer(); break;
            }
        }

        protected abstract void FinishLiqduiBuffer();

        public override void Draw(Vector3 position)
        {
            Debug.Fail("Sections should be drawn using DrawStage.");

            for (int stage = 0; stage < 3; stage++)
            {
                PrepareStage(stage);
                DrawStage(stage, position);
                FinishStage(stage);
            }
        }
    }
}