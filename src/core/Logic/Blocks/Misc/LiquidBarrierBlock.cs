﻿// <copyright file="LiquidBarrierBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that lets liquids through but can be closed by interacting with it.
    ///     Data bit usage: <c>-----o</c>
    /// </summary>
    // o: open
    public class LiquidBarrierBlock : BasicBlock, IFillable, IFlammable
    {
        private readonly TextureLayout open;
        private int[] openTextureIndices = null!;

        internal LiquidBarrierBlock(string name, string namedId, TextureLayout closed, TextureLayout open) :
            base(
                name,
                namedId,
                BlockFlags.Basic with { IsInteractable = true },
                closed)
        {
            this.open = open;
        }

        /// <inheritdoc />
        public bool AllowInflow(World world, Vector3i position, BlockSide side, Liquid liquid)
        {
            BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

            return (block.Data & 0b00_0001) == 1;
        }

        /// <inheritdoc />
        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            base.Setup(indexProvider);

            openTextureIndices = open.GetTexIndexArray();
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMeshData mesh = base.GetMesh(info);

            if ((info.Data & 0b00_0001) == 1)
                mesh = mesh.SwapTextureIndex(openTextureIndices[(int) info.Side]);

            return mesh;
        }

        /// <inheritdoc />
        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            entity.World.SetBlock(this.AsInstance(data ^ 0b00_0001), position);
        }
    }
}