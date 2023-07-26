// <copyright file="FluidBarrierBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that lets fluids through but can be closed by interacting with it.
///     Data bit usage: <c>-----o</c>
/// </summary>
// o: open
public class FluidBarrierBlock : BasicBlock, IFillable, ICombustible
{
    private readonly TextureLayout open;
    private int[] openTextureIndices = null!;

    internal FluidBarrierBlock(string name, string namedId, TextureLayout closed, TextureLayout open) :
        base(
            name,
            namedId,
            BlockFlags.Basic with {IsInteractable = true},
            closed)
    {
        this.open = open;
    }

    /// <inheritdoc />
    public bool IsInflowAllowed(World world, Vector3i position, BlockSide side, Fluid fluid)
    {
        if (fluid.IsGas) return true;

        BlockInstance block = world.GetBlock(position) ?? BlockInstance.Default;

        return (block.Data & 0b00_0001) == 1;
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        base.OnSetup(indexProvider);

        openTextureIndices = open.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        entity.World.SetBlock(this.AsInstance(data ^ 0b00_0001), position);
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        ISimple.MeshData mesh = base.GetMeshData(info);

        if ((info.Data & 0b00_0001) == 1)
            mesh = mesh with {TextureIndex = openTextureIndices[(int) info.Side]};

        return mesh;
    }
}
