// <copyright file="BlockModel.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A block model for complex blocks, can be loaded from disk.
/// </summary>
[SuppressMessage(
    "Performance",
    "CA1819:Properties should not return arrays",
    Justification = "This class is meant for data storage.")]
public sealed class BlockModel
{
    private const string BlockModelIsLockedMessage = "This block model is locked and can no longer be modified.";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<BlockModel>();

    private static readonly string path = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Resources",
        "Models");

    private static ITextureIndexProvider blockTextureIndexProvider = null!;

    private bool isLocked;
    private uint[] lockedIndices = null!;
    private int[] lockedTextureIndices = null!;

    private float[] lockedVertices = null!;

    /// <summary>
    ///     Create an empty block model.
    /// </summary>
    public BlockModel() {}

    /// <summary>
    ///     Copy-constructor.
    /// </summary>
    /// <param name="original">The original model to copy.</param>
    private BlockModel(BlockModel original)
    {
        TextureNames = (string[]) original.TextureNames.Clone();
        Quads = (Quad[]) original.Quads.Clone();
    }

    /// <summary>
    ///     The names of the textures used by this model.
    /// </summary>
    public string[] TextureNames { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The quads that make up this model.
    /// </summary>
    public Quad[] Quads { get; set; } = Array.Empty<Quad>();

    /// <summary>
    ///     The vertex count of this model.
    /// </summary>
    public int VertexCount => Quads.Length * 4;

    /// <summary>
    ///     Get the model as a block mesh.
    /// </summary>
    public BlockMesh Mesh
    {
        get
        {
            ToData(out float[] vertices, out int[] textureIndices, out uint[] indices);

            return new BlockMesh((uint) VertexCount, vertices, textureIndices, indices);
        }
    }

    /// <summary>
    ///     Set the texture index provider.
    /// </summary>
    /// <param name="blockTextureProvider">The block texture index provider.</param>
    public static void SetBlockTextureIndexProvider(ITextureIndexProvider blockTextureProvider)
    {
        blockTextureIndexProvider = blockTextureProvider;
    }

    /// <summary>
    ///     Splits the BlockModel into two parts, using a given plane to sort all faces.
    /// </summary>
    /// <param name="position">Position of the plane.</param>
    /// <param name="normal">Normal of the plane.</param>
    /// <param name="a">The first model.</param>
    /// <param name="b">The second model.</param>
    public void PlaneSplit(Vector3 position, Vector3 normal, out BlockModel a, out BlockModel b)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        normal = normal.Normalized();
        List<Quad> quadsA = new();
        List<Quad> quadsB = new();

        foreach (Quad quad in Quads)
            if (Vector3.Dot(quad.Center - position, normal) > 0) quadsA.Add(quad);
            else quadsB.Add(quad);

        a = new BlockModel { TextureNames = TextureNames };
        b = new BlockModel { TextureNames = TextureNames };

        a.Quads = quadsA.ToArray();
        b.Quads = quadsB.ToArray();
    }

    /// <summary>
    ///     Moves all vertices of this model.
    /// </summary>
    /// <param name="movement"></param>
    public void Move(Vector3 movement)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        var xyz = Matrix4.CreateTranslation(movement);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyTranslationMatrix(xyz);
    }

    /// <summary>
    ///     Rotates the model on the y axis in steps of ninety degrees.
    /// </summary>
    /// <param name="rotations">Number of rotations.</param>
    /// <param name="rotateTopAndBottomTexture">Whether the top and bottom texture should be rotated.</param>
    public void RotateY(int rotations, bool rotateTopAndBottomTexture = true)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        if (rotations == 0) return;

        float angle = rotations * MathHelper.PiOver2 * -1f;

        Matrix4 xyz = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * Matrix4.CreateRotationY(angle) *
                      Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        var nop = Matrix4.CreateRotationY(angle);

        rotations = rotateTopAndBottomTexture ? 0 : rotations;

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyRotationMatrixY(xyz, nop, rotations);
    }

    /// <summary>
    ///     Overwrites the textures of the model, replacing them with a single texture.
    /// </summary>
    /// <param name="newTexture">The replacement texture.</param>
    public void OverwriteTexture(string newTexture)
    {
        TextureNames = new[] { newTexture };

        for (var i = 0; i < Quads.Length; i++)
        {
            Quad old = Quads[i];

            Quads[i] = new Quad
            {
                TextureId = 0,
                Vert0 = old.Vert0,
                Vert1 = old.Vert1,
                Vert2 = old.Vert2,
                Vert3 = old.Vert3
            };
        }
    }

    /// <summary>
    ///     Creates six models, one for each block side, from a north oriented model.
    /// </summary>
    /// <returns> The six models.</returns>
    public (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
        CreateAllSides()
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
            result;

        result.front = this;

        result.back = CreateSideModel(BlockSide.Back);
        result.left = CreateSideModel(BlockSide.Left);
        result.right = CreateSideModel(BlockSide.Right);
        result.bottom = CreateSideModel(BlockSide.Bottom);
        result.top = CreateSideModel(BlockSide.Top);

        return result;
    }

    /// <summary>
    ///     Create versions of this model for each axis.
    /// </summary>
    /// <returns>The model versions.</returns>
    public (BlockModel x, BlockModel y, BlockModel z) CreateAllAxis()
    {
        (BlockModel x, BlockModel y, BlockModel z) result;

        result.z = this;

        result.x = CreateSideModel(BlockSide.Left);
        result.y = CreateSideModel(BlockSide.Bottom);

        return result;
    }

    /// <summary>
    ///     Create models for each orientation.
    /// </summary>
    /// <param name="rotateTopAndBottomTexture">Whether the top and bottom textures should be rotated.</param>
    /// <returns>All model versions.</returns>
    public (BlockModel north, BlockModel east, BlockModel south, BlockModel west) CreateAllOrientations(
        bool rotateTopAndBottomTexture)
    {
        BlockModel north = this;

        BlockModel east = new(north);
        east.RotateY(rotations: 1, rotateTopAndBottomTexture);

        BlockModel south = new(east);
        south.RotateY(rotations: 1, rotateTopAndBottomTexture);

        BlockModel west = new(south);
        west.RotateY(rotations: 1, rotateTopAndBottomTexture);

        return (north, east, south, west);
    }

    private BlockModel CreateSideModel(BlockSide side)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        BlockModel copy = new(this);

        Matrix4 rotation;
        Vector3 axis;
        int rotations;

        switch (side)
        {
            case BlockSide.Front:
                return copy;

            case BlockSide.Back:
                rotation = Matrix4.CreateRotationY(MathHelper.Pi);
                axis = Vector3.UnitY;
                rotations = 2;

                break;

            case BlockSide.Left:
                rotation = Matrix4.CreateRotationY(MathHelper.ThreePiOver2);
                axis = Vector3.UnitY;
                rotations = 1;

                break;

            case BlockSide.Right:
                rotation = Matrix4.CreateRotationY(MathHelper.PiOver2);
                axis = Vector3.UnitY;
                rotations = 3;

                break;

            case BlockSide.Bottom:
                rotation = Matrix4.CreateRotationX(MathHelper.PiOver2);
                axis = Vector3.UnitX;
                rotations = 1;

                break;

            case BlockSide.Top:
                rotation = Matrix4.CreateRotationX(MathHelper.ThreePiOver2);
                axis = Vector3.UnitX;
                rotations = 1;

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(side));
        }

        Matrix4 matrix = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * rotation *
                         Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        copy.ApplyMatrix(matrix, rotation);
        copy.RotateTextureCoordinates(axis, rotations);

        return copy;
    }

    private void ApplyMatrix(Matrix4 xyz, Matrix4 nop)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyMatrix(xyz, nop);
    }

    private void RotateTextureCoordinates(Vector3 axis, int rotations)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].RotateTextureCoordinates(axis, rotations);
    }

    /// <summary>
    ///     Get this model as data that can be used for rendering.
    /// </summary>
    public void ToData(out float[] vertices, out int[] textureIndices, out uint[] indices)
    {
        if (isLocked)
        {
            vertices = lockedVertices;
            textureIndices = lockedTextureIndices;
            indices = lockedIndices;

            return;
        }

        var texIndexLookup = new int[TextureNames.Length];

        for (var i = 0; i < TextureNames.Length; i++)
            texIndexLookup[i] = blockTextureIndexProvider.GetTextureIndex(TextureNames[i]);

        vertices = new float[Quads.Length * 32];
        textureIndices = new int[Quads.Length * 4];

        for (var q = 0; q < Quads.Length; q++)
        {
            Quad quad = Quads[q];

            // Vertex 0.
            vertices[q * 32 + 0] = quad.Vert0.X;
            vertices[q * 32 + 1] = quad.Vert0.Y;
            vertices[q * 32 + 2] = quad.Vert0.Z;
            vertices[q * 32 + 3] = MathHelper.Clamp(quad.Vert0.U, min: 0f, max: 1f);
            vertices[q * 32 + 4] = MathHelper.Clamp(quad.Vert0.V, min: 0f, max: 1f);
            vertices[q * 32 + 5] = MathHelper.Clamp(quad.Vert0.N, min: -1f, max: 1f);
            vertices[q * 32 + 6] = MathHelper.Clamp(quad.Vert0.O, min: -1f, max: 1f);
            vertices[q * 32 + 7] = MathHelper.Clamp(quad.Vert0.P, min: -1f, max: 1f);

            textureIndices[q * 4 + 0] = texIndexLookup[quad.TextureId];

            // Vertex 1.
            vertices[q * 32 + 8] = quad.Vert1.X;
            vertices[q * 32 + 9] = quad.Vert1.Y;
            vertices[q * 32 + 10] = quad.Vert1.Z;
            vertices[q * 32 + 11] = MathHelper.Clamp(quad.Vert1.U, min: 0f, max: 1f);
            vertices[q * 32 + 12] = MathHelper.Clamp(quad.Vert1.V, min: 0f, max: 1f);
            vertices[q * 32 + 13] = MathHelper.Clamp(quad.Vert1.N, min: -1f, max: 1f);
            vertices[q * 32 + 14] = MathHelper.Clamp(quad.Vert1.O, min: -1f, max: 1f);
            vertices[q * 32 + 15] = MathHelper.Clamp(quad.Vert1.P, min: -1f, max: 1f);

            textureIndices[q * 4 + 1] = texIndexLookup[quad.TextureId];

            // Vertex 2.
            vertices[q * 32 + 16] = quad.Vert2.X;
            vertices[q * 32 + 17] = quad.Vert2.Y;
            vertices[q * 32 + 18] = quad.Vert2.Z;
            vertices[q * 32 + 19] = MathHelper.Clamp(quad.Vert2.U, min: 0f, max: 1f);
            vertices[q * 32 + 20] = MathHelper.Clamp(quad.Vert2.V, min: 0f, max: 1f);
            vertices[q * 32 + 21] = MathHelper.Clamp(quad.Vert2.N, min: -1f, max: 1f);
            vertices[q * 32 + 22] = MathHelper.Clamp(quad.Vert2.O, min: -1f, max: 1f);
            vertices[q * 32 + 23] = MathHelper.Clamp(quad.Vert2.P, min: -1f, max: 1f);

            textureIndices[q * 4 + 2] = texIndexLookup[quad.TextureId];

            // Vertex 3.
            vertices[q * 32 + 24] = quad.Vert3.X;
            vertices[q * 32 + 25] = quad.Vert3.Y;
            vertices[q * 32 + 26] = quad.Vert3.Z;
            vertices[q * 32 + 27] = MathHelper.Clamp(quad.Vert3.U, min: 0f, max: 1f);
            vertices[q * 32 + 28] = MathHelper.Clamp(quad.Vert3.V, min: 0f, max: 1f);
            vertices[q * 32 + 29] = MathHelper.Clamp(quad.Vert3.N, min: -1f, max: 1f);
            vertices[q * 32 + 30] = MathHelper.Clamp(quad.Vert3.O, min: -1f, max: 1f);
            vertices[q * 32 + 31] = MathHelper.Clamp(quad.Vert3.P, min: -1f, max: 1f);

            textureIndices[q * 4 + 3] = texIndexLookup[quad.TextureId];
        }

        indices = new uint[Quads.Length * 6];

        for (var i = 0; i < Quads.Length; i++)
        {
            var offset = (uint) (i * 4);

            indices[i * 6 + 0] = 0 + offset;
            indices[i * 6 + 1] = 2 + offset;
            indices[i * 6 + 2] = 1 + offset;
            indices[i * 6 + 3] = 0 + offset;
            indices[i * 6 + 4] = 3 + offset;
            indices[i * 6 + 5] = 2 + offset;
        }
    }

    /// <summary>
    ///     Lock the model. This will prevent modifications to the model, but combining with other models will be faster.
    /// </summary>
    public void Lock()
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        ToData(out lockedVertices, out lockedTextureIndices, out lockedIndices);

        isLocked = true;
    }

    /// <summary>
    ///     Save this model to a file.
    /// </summary>
    /// <param name="name">The path to the file.</param>
    public void Save(string name)
    {
        if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

        JsonSerializerOptions options = new() { IgnoreReadOnlyProperties = true, WriteIndented = true };

        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(Path.Combine(path, name + ".json"), json);
    }

    /// <summary>
    ///     Get a copy of this model.
    /// </summary>
    /// <returns>The copy.</returns>
    public BlockModel Copy()
    {
        return new BlockModel(this);
    }

    #region STATIC METHODS

    /// <summary>
    ///     Load a block model from file. All models are loaded from a specific directory.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>The loaded model.</returns>
    public static BlockModel Load(string name)
    {
        try
        {
            string json = File.ReadAllText(Path.Combine(path, name + ".json"));
            BlockModel model = JsonSerializer.Deserialize<BlockModel>(json) ?? new BlockModel();

            logger.LogDebug(Events.ResourceLoad, "Loaded BlockModel: {Name}", name);

            return model;
        }
        catch (Exception e) when (e is IOException or FileNotFoundException or JsonException)
        {
            logger.LogWarning(
                Events.MissingResource,
                e,
                "Could not load the model '{Name}' because an exception occurred, fallback will be used instead",
                name);

            return CreateFallback();
        }
    }

    private static BlockModel CreateFallback()
    {
        const float begin = 0.375f;
        const float size = 0.5f;

        int[][] uvs = BlockModels.GetBlockUVs(isRotated: false);

        Quad BuildQuad(BlockSide side)
        {
            Vector3i normal = side.Direction();

            Vertex BuildVertex(int[] corner, int[] uv)
            {
                return new Vertex
                {
                    X = begin + corner[0] * size,
                    Y = begin + corner[1] * size,
                    Z = begin + corner[2] * size,
                    U = begin + uv[0] * size,
                    V = begin + uv[1] * size,
                    N = normal.X,
                    O = normal.Y,
                    P = normal.Z
                };
            }

            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

            return new Quad
            {
                TextureId = 0,
                Vert0 = BuildVertex(a, uvs[0]),
                Vert1 = BuildVertex(b, uvs[1]),
                Vert2 = BuildVertex(c, uvs[2]),
                Vert3 = BuildVertex(d, uvs[3])
            };
        }

        return new BlockModel
        {
            TextureNames = new[] { "missing_texture" },
            Quads = new[]
            {
                BuildQuad(BlockSide.Front),
                BuildQuad(BlockSide.Back),
                BuildQuad(BlockSide.Left),
                BuildQuad(BlockSide.Right),
                BuildQuad(BlockSide.Bottom),
                BuildQuad(BlockSide.Top)
            }
        };
    }

    /// <summary>
    ///     Combine the data of multiple block models.
    /// </summary>
    /// <param name="vertexCount">The resulting vertex count.</param>
    /// <param name="models">The models to combine.</param>
    /// <returns>The combined data.</returns>
    public static (float[] vertices, int[] textureIndices, uint[] indices) CombineData(out uint vertexCount,
        params BlockModel[] models)
    {
        vertexCount = 0;

        bool locked = models.Aggregate(seed: true, (current, model) => current && model.isLocked);

        if (locked)
        {
            int vertexArrayLength = models.Sum(model => model.lockedVertices.Length);
            int textureIndicesArrayLength = models.Sum(model => model.lockedTextureIndices.Length);
            int indicesArrayLength = models.Sum(model => model.lockedIndices.Length);

            var vertices = new float[vertexArrayLength];
            var textureIndices = new int[textureIndicesArrayLength];
            var indices = new uint[indicesArrayLength];

            var copiedVertices = 0;
            var copiedTextureIndices = 0;
            var copiedIndices = 0;

            foreach (BlockModel model in models)
            {
                Array.Copy(
                    model.lockedVertices,
                    sourceIndex: 0,
                    vertices,
                    copiedVertices,
                    model.lockedVertices.Length);

                Array.Copy(
                    model.lockedTextureIndices,
                    sourceIndex: 0,
                    textureIndices,
                    copiedTextureIndices,
                    model.lockedTextureIndices.Length);

                Array.Copy(model.lockedIndices, sourceIndex: 0, indices, copiedIndices, model.lockedIndices.Length);

                for (int i = copiedIndices; i < copiedIndices + model.lockedIndices.Length; i++)
                    indices[i] += vertexCount;

                copiedVertices += model.lockedVertices.Length;
                copiedTextureIndices += model.lockedTextureIndices.Length;
                copiedIndices += model.lockedIndices.Length;

                vertexCount += (uint) model.VertexCount;
            }

            return (vertices, textureIndices, indices);
        }
        else
        {
            List<float> vertices = new();
            List<int> textureIndices = new();
            List<uint> indices = new();

            foreach (BlockModel model in models)
            {
                model.ToData(out float[] modelVertices, out int[] modelTextureIndices, out uint[] modelIndices);

                int firstNewIndex = indices.Count;

                vertices.AddRange(modelVertices);
                textureIndices.AddRange(modelTextureIndices);
                indices.AddRange(modelIndices);

                for (int i = firstNewIndex; i < indices.Count; i++) indices[i] += vertexCount;

                vertexCount += (uint) model.VertexCount;
            }

            return (vertices.ToArray(), textureIndices.ToArray(), indices.ToArray());
        }
    }

    /// <summary>
    ///     Get the combined mesh of multiple block models.
    /// </summary>
    /// <param name="models">The models to combine.</param>
    /// <returns>The combined mesh.</returns>
    public static BlockMesh GetCombinedMesh(params BlockModel[] models)
    {
        (float[] vertices, int[] textureIndices, uint[] indices) = CombineData(out uint vertexCount, models);

        return new BlockMesh(vertexCount, vertices, textureIndices, indices);
    }

    #endregion STATIC METHODS
}

/// <summary>
///     A quad.
/// </summary>
public struct Quad : IEquatable<Quad>
{
    /// <summary>
    ///     The texture id used for this quad.
    /// </summary>
    public int TextureId { get; set; }

    /// <summary>
    ///     The first vertex.
    /// </summary>
    public Vertex Vert0 { get; set; }

    /// <summary>
    ///     The second vertex.
    /// </summary>
    public Vertex Vert1 { get; set; }

    /// <summary>
    ///     The third vertex.
    /// </summary>
    public Vertex Vert2 { get; set; }

    /// <summary>
    ///     The fourth vertex.
    /// </summary>
    public Vertex Vert3 { get; set; }

    /// <summary>
    ///     The center of the quad.
    /// </summary>
    public Vector3 Center => (Vert0.Position + Vert1.Position + Vert2.Position + Vert3.Position) / 4;

    /// <summary>
    ///     Apply a matrix only affecting the xyz values.
    /// </summary>
    /// <param name="xyz">The matrix to apply.</param>
    /// <returns>The new quad.</returns>
    public Quad ApplyTranslationMatrix(Matrix4 xyz)
    {
        Vert0 = Vert0.ApplyTranslationMatrix(xyz);
        Vert1 = Vert1.ApplyTranslationMatrix(xyz);
        Vert2 = Vert2.ApplyTranslationMatrix(xyz);
        Vert3 = Vert3.ApplyTranslationMatrix(xyz);

        return this;
    }

    /// <summary>
    ///     Apply a matrix to this quad.
    /// </summary>
    /// <param name="xyz">The matrix to apply to the position.</param>
    /// <param name="nop">The matrix to apply to the normals.</param>
    /// <returns>The quad with the matrices applied.</returns>
    public Quad ApplyMatrix(Matrix4 xyz, Matrix4 nop)
    {
        Vert0 = Vert0.ApplyMatrix(xyz, nop);
        Vert1 = Vert1.ApplyMatrix(xyz, nop);
        Vert2 = Vert2.ApplyMatrix(xyz, nop);
        Vert3 = Vert3.ApplyMatrix(xyz, nop);

        return this;
    }

    /// <summary>
    ///     Apply a rotation matrix to this quad.
    /// </summary>
    public Quad ApplyRotationMatrixY(Matrix4 xyz, Matrix4 nop, int rotations)
    {
        // Rotate positions and normals.
        Vert0 = Vert0.ApplyMatrix(xyz, nop);
        Vert1 = Vert1.ApplyMatrix(xyz, nop);
        Vert2 = Vert2.ApplyMatrix(xyz, nop);
        Vert3 = Vert3.ApplyMatrix(xyz, nop);

        // Rotate UVs for top and bottom sides.
        if (new Vector3(Vert0.N, Vert0.O, Vert0.P).Absolute().Rounded(digits: 2) == Vector3.UnitY)
            for (var r = 0; r < rotations; r++)
            {
                Vert0 = Vert0.RotateUV();
                Vert1 = Vert1.RotateUV();
                Vert2 = Vert2.RotateUV();
                Vert3 = Vert3.RotateUV();
            }

        return this;
    }

    /// <summary>
    ///     Rotate the texture coordinates.
    /// </summary>
    public Quad RotateTextureCoordinates(Vector3 axis, int rotations)
    {
        if (new Vector3(Vert0.N, Vert0.O, Vert0.P).Absolute().Rounded(digits: 2) != axis) return this;

        for (var r = 0; r < rotations; r++)
        {
            Vert0 = Vert0.RotateUV();
            Vert1 = Vert1.RotateUV();
            Vert2 = Vert2.RotateUV();
            Vert3 = Vert3.RotateUV();
        }

        return this;
    }

    /// <inheritdoc />
    public bool Equals(Quad other)
    {
        return (TextureId, Vert0, Vert1, Vert2, Vert3) ==
               (other.TextureId, other.Vert0, other.Vert1, other.Vert2, other.Vert3);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Quad other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(TextureId, Vert0, Vert1, Vert2, Vert3);
    }

    /// <summary>
    ///     Checks if two quads are equal.
    /// </summary>
    public static bool operator ==(Quad left, Quad right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two quads are not equal.
    /// </summary>
    public static bool operator !=(Quad left, Quad right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
///     A vertex.
/// </summary>
public struct Vertex : IEquatable<Vertex>
{
    /// <summary>
    ///     The x position.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    ///     The y position.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    ///     The z position.
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    ///     The u texture coordinate.
    /// </summary>
    public float U { get; set; }

    /// <summary>
    ///     The v texture coordinate.
    /// </summary>
    public float V { get; set; }

    /// <summary>
    ///     The first normal component.
    /// </summary>
    public float N { get; set; }

    /// <summary>
    ///     The second normal component.
    /// </summary>
    public float O { get; set; }

    /// <summary>
    ///     The third normal component.
    /// </summary>
    public float P { get; set; }

    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3 Position => new(X, Y, Z);

    /// <summary>
    ///     Apply a translation matrix to this vertex.
    /// </summary>
    public Vertex ApplyTranslationMatrix(Matrix4 xyz)
    {
        Vector4 position = new Vector4(X, Y, Z, w: 1f) * xyz;

        X = position.X;
        Y = position.Y;
        Z = position.Z;

        return this;
    }

    /// <summary>
    ///     Apply a matrix to this vertex.
    /// </summary>
    public Vertex ApplyMatrix(Matrix4 xyz, Matrix4 nop)
    {
        Vector4 position = new Vector4(X, Y, Z, w: 1f) * xyz;
        Vector4 normal = new Vector4(N, O, P, w: 1f) * nop;

        X = position.X;
        Y = position.Y;
        Z = position.Z;

        N = normal.X;
        O = normal.Y;
        P = normal.Z;

        return this;
    }

    /// <summary>
    ///     Rotate the texture coordinates.
    /// </summary>
    public Vertex RotateUV()
    {
        Vertex old = this;

        U = old.V;
        V = Math.Abs(old.U - 1f);

        return this;
    }

    /// <inheritdoc />
    public bool Equals(Vertex other)
    {
        return (X, Y, Z, U, V, N, O, P) == (other.X, other.Y, other.Z, other.U, other.V, other.N, other.O, other.P);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Vertex other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, U, V, N, O, P);
    }

    /// <summary>
    ///     Checks if two vertices are equal.
    /// </summary>
    public static bool operator ==(Vertex left, Vertex right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two vertices are not equal.
    /// </summary>
    public static bool operator !=(Vertex left, Vertex right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
///     Extension methods for <see cref="BlockModel" />.
/// </summary>
public static class BlockModelExtensions
{
    /// <summary>
    ///     Lock a group of models.
    /// </summary>
    /// <param name="group">The models to lock.</param>
    public static void Lock(
        this (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel
            top) group)
    {
        group.front.Lock();
        group.back.Lock();
        group.left.Lock();
        group.right.Lock();
        group.bottom.Lock();
        group.top.Lock();
    }

    /// <summary>
    ///     Lock a group of models.
    /// </summary>
    /// <param name="group">The group to lock.</param>
    public static void Lock(this (BlockModel north, BlockModel east, BlockModel south, BlockModel west) group)
    {
        group.north.Lock();
        group.east.Lock();
        group.south.Lock();
        group.west.Lock();
    }
}
