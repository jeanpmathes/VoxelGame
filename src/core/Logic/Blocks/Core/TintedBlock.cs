﻿// <copyright file="TintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that has differently colored versions. Animation can be activated.
    ///     Data bit usage: <c>-ccccc</c>
    /// </summary>
    // c: color
    public class TintedBlock : BasicBlock, IWideConnectable
    {
        private readonly bool isAnimated;

        internal TintedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout,
            bool isAnimated = false) :
            base(
                name,
                namedId,
                flags with { IsInteractable = true },
                layout)
        {
            this.isAnimated = isAnimated;
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(((BlockColor) (0b01_1111 & info.Data)).ToTintColor(), isAnimated);
        }

        /// <inheritdoc />
        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            entity.World.SetBlock(this.AsInstance((data + 1) & 0b01_1111), position);
        }
    }
}
