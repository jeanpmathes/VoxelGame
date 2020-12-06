namespace VoxelGame.Client.Rendering
{
    public abstract class OverlayRenderer : Renderer
    {
        public abstract void SetTexture(int number);

        public abstract void Draw();
    }
}