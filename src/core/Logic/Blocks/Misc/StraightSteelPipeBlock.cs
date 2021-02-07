using System;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks.Misc
{
    internal class StraightSteelPipeBlock : Block, IFillable, IIndustrialPipeConnectable
    {
        public StraightSteelPipeBlock(string name, string namedId, bool isFull, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid, bool receiveCollisions, bool isTrigger, bool isReplaceable, bool isInteractable, BoundingBox boundingBox, TargetBuffer targetBuffer) : base(name, namedId, isFull, isOpaque, renderFaceAtNonOpaques, isSolid, receiveCollisions, isTrigger, isReplaceable, isInteractable, boundingBox, targetBuffer)
        {
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices,
            out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            throw new NotImplementedException();
        }
    }
}