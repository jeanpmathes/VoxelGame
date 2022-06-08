// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

// ReSharper disable CommentTypo

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A section of the world, specifically for the client.
///     Sections do not know their exact position in the world.
/// </summary>
[Serializable]
public class ClientSection : Section
{
    [NonSerialized] private bool hasMesh;
    [NonSerialized] private SectionRenderer? renderer;

    /// <summary>
    ///     Create a new client section.
    /// </summary>
    /// <param name="world">The world containing the client section.</param>
    public ClientSection(World world) : base(world) {}

    /// <inheritdoc />
    protected override void Setup()
    {
        renderer = new SectionRenderer();

        hasMesh = false;
        disposed = false;
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    public void CreateAndSetMesh(SectionPosition position)
    {
        SectionMeshData meshData = CreateMeshData(position);
        SetMeshData(meshData);
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The created mesh data.</returns>
    [SuppressMessage(
        "Blocker Code Smell",
        "S2437:Silly bit operations should not be performed",
        Justification = "Improves readability.")]
    public SectionMeshData CreateMeshData(SectionPosition position)
    {
        MeshingContext context = new(this, position, World)
        {
            BlockTint = TintColor.Green,
            FluidTint = TintColor.Blue
        };

        // Loop through the section
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            uint val = blocks[(x << SizeExp2) + (y << SizeExp) + z];

            Decode(
                val,
                out Block currentBlock,
                out uint data,
                out Fluid currentFluid,
                out FluidLevel level,
                out bool isStatic);

            IBlockMeshable meshable = currentBlock;
            meshable.CreateMesh((x, y, z), new BlockMeshInfo(BlockSide.All, data, currentFluid), context);

            currentFluid.CreateMesh(
                (x, y, z),
                FluidMeshInfo.Fluid(currentBlock, level, BlockSide.All, isStatic),
                context);
        }

        SectionMeshData meshData = context.GenerateMeshData();
        hasMesh = meshData.IsFilled;

        context.ReturnToPool();

        return meshData;
    }

    /// <summary>
    ///     Set the mesh data for this section. The mesh must be generated from this section.
    /// </summary>
    /// <param name="meshData">The mesh data to use and activate.</param>
    public void SetMeshData(SectionMeshData meshData)
    {
        Debug.Assert(renderer != null);
        Debug.Assert(hasMesh == meshData.IsFilled);

        renderer.SetData(meshData);
    }

    /// <summary>
    ///     Render this section.
    /// </summary>
    /// <param name="stage">The current render stage.</param>
    /// <param name="position">The position of this section in world coordinates.</param>
    public void Render(int stage, Vector3d position)
    {
        if (hasMesh) renderer?.DrawStage(stage, position);
    }

    #region IDisposable Support

    [NonSerialized] private bool disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing) renderer?.Dispose();

            disposed = true;
        }
    }

    #endregion IDisposable Support
}