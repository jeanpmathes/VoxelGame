namespace VoxelGame.Client.Rendering
{
    public abstract class OverlayRenderer : Renderer
    {
        public abstract void SetBlockTexture(int number);

        public abstract void SetLiquidTexture(int number);

        public abstract void Draw();
    }
}