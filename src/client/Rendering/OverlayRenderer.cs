namespace VoxelGame.Client.Rendering
{
    public abstract class OverlayRenderer : Renderer
    {
        private protected int textureId;
        private protected int samplerId;

        public void SetBlockTexture(int number)
        {
            samplerId = (number / 2048) + 1;
            textureId = number % 2048;
        }

        public void SetLiquidTexture(int number)
        {
            samplerId = 5;
            textureId = number;
        }

        public abstract void Draw();
    }
}