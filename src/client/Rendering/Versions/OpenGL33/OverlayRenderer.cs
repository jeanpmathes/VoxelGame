using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    public class OverlayRenderer : Rendering.OverlayRenderer
    {
        public override void Draw(Vector3 position)
        {
            Draw();
        }

        public override void SetBlockTexture(int number)
        {
            throw new NotImplementedException();
        }

        public override void SetLiquidTexture(int number)
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}