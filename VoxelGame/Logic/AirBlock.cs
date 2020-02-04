namespace VoxelGame.Logic
{
    public class AirBlock : Block
    {
        public AirBlock(string name) : base(name, false, false)
        {

        }

        public override uint GetMesh(BlockSide side, out float[] vertecies, out uint[] indicies)
        {
            vertecies = null;
            indicies = null;

            return 0;
        }
    }
}
