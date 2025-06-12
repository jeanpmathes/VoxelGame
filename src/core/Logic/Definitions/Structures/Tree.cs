// <copyright file="Tree.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Legacy.Blocks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Utilities;
using Blocks = VoxelGame.Core.Logic.Elements.Legacy.Blocks;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
///     A dynamically created tree structure.
/// </summary>
public class Tree : DynamicStructure
{
    private readonly Content trunk;
    private readonly Content leaf;
    private readonly Content roots;

    private readonly Int32 trunkHeight;

    private readonly Shape3D crownShape;
    private readonly Double crownRandomization;

    private readonly Vector3d crownOffset;

    /// <summary>
    /// Creates a new tree.
    /// </summary>
    /// <param name="trunkHeight">The height of the trunk.</param>
    /// <param name="crownRandomization">The randomization factor of the crown, a smaller factor causes a more dense crown.</param>
    /// <param name="crownShape">The shape of the crown, will be centered on the X-Z-plane.</param>
    /// <param name="log">The log block.</param>
    /// <param name="leaves">The leaves block.</param>
    public Tree(Int32 trunkHeight, Double crownRandomization, Shape3D crownShape, Block log, Block leaves)
    {
        this.trunkHeight = trunkHeight;
        this.crownRandomization = crownRandomization;
        this.crownShape = crownShape;

        trunk = new Content(log);
        leaf = new Content(leaves);
        roots = new Content(Blocks.Instance.Roots);

        if (log is RotatedBlock rotatedBlock) trunk = new Content(rotatedBlock.GetInstance(Axis.Y), FluidInstance.Default);

        Box3d box = crownShape.BoundingBox;
        Vector3i min = box.Min.Floor();
        Vector3i max = box.Max.Ceiling();
        Vector3i size = (max - min).Abs();

        Extents = new Vector3i(
            Math.Max(size.X, val2: 1),
            Math.Max(size.Y, trunkHeight) + 1,
            Math.Max(size.Z, val2: 1)
        );

        crownOffset = new Vector3d(
            Math.Floor(size.X / 2.0),
            y: 0.0,
            Math.Floor(size.Z / 2.0)
        );
    }

    /// <inheritdoc />
    public override Vector3i Extents { get; }

    /// <inheritdoc />
    protected override (Content content, Boolean overwrite)? GetContent(Vector3i offset, Single random)
    {
        Int32 center = Extents.X / 2;
        Debug.Assert(Extents.X == Extents.Z);

        if (offset.X == center && offset.Z == center)
        {
            if (offset.Y == 0)
                return (roots, overwrite: true);

            if (offset.Y < trunkHeight)
                return (trunk, overwrite: true);

            if (offset.Y == trunkHeight)
                return (leaf, overwrite: true);
        }

        if (!crownShape.Contains(offset - crownOffset, out Double closeness)) return null;
        if (closeness < crownRandomization * random) return null;

        return (leaf, overwrite: false);
    }
}
