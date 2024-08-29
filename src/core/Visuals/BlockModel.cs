// <copyright file="BlockModel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Logging;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A block model for complex blocks, can be loaded from disk.
/// </summary>
public sealed partial class BlockModel
{
    private const String BlockModelIsLockedMessage = "This block model is locked and can no longer be modified.";

    private static readonly DirectoryInfo path = FileSystem.GetResourceDirectory("Models");

    private static ITextureIndexProvider blockTextureIndexProvider = null!;

    private BlockMesh.Quad[]? lockedQuads;

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
        TextureNames = (String[]) original.TextureNames.Clone();
        Quads = (Quad[]) original.Quads.Clone();
    }

    /// <summary>
    ///     The names of the textures used by this model.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "This class is meant for data storage.")]
    public String[] TextureNames { get; set; } = Array.Empty<String>();

    /// <summary>
    ///     The quads that make up this model.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "This class is meant for data storage.")]
    public Quad[] Quads { get; set; } = Array.Empty<Quad>();

    /// <summary>
    ///     Get the model as a block mesh.
    /// </summary>
    public BlockMesh Mesh
    {
        get
        {
            ToData(out BlockMesh.Quad[] quads);

            return new BlockMesh(quads);
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
    public void PlaneSplit(Vector3d position, Vector3d normal, out BlockModel a, out BlockModel b)
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        normal = normal.Normalized();
        List<Quad> quadsA = [];
        List<Quad> quadsB = [];

        foreach (Quad quad in Quads)
            if (Vector3d.Dot(quad.Center - position, normal) > 0) quadsA.Add(quad);
            else quadsB.Add(quad);

        a = new BlockModel {TextureNames = TextureNames};
        b = new BlockModel {TextureNames = TextureNames};

        a.Quads = quadsA.ToArray();
        b.Quads = quadsB.ToArray();
    }

    /// <summary>
    ///     Moves all vertices of this model.
    /// </summary>
    /// <param name="movement"></param>
    public void Move(Vector3d movement)
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        var xyz = Matrix4.CreateTranslation(movement.ToVector3());

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyMatrix(xyz);
    }

    /// <summary>
    ///     Rotates the model on the y axis in steps of ninety degrees.
    /// </summary>
    /// <param name="rotations">Number of rotations.</param>
    /// <param name="rotateTopAndBottomTexture">Whether the top and bottom texture should be rotated.</param>
    public void RotateY(Int32 rotations, Boolean rotateTopAndBottomTexture = true)
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        if (rotations == 0) return;

        Single angle = rotations * MathHelper.PiOver2 * -1f;

        Matrix4 xyz = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * Matrix4.CreateRotationY(angle) *
                      Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        rotations = rotateTopAndBottomTexture ? 0 : rotations;

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyRotationMatrixY(xyz, rotations);
    }

    /// <summary>
    ///     Overwrites the textures of the model, replacing them with a single texture.
    /// </summary>
    /// <param name="newTexture">The replacement texture.</param>
    public void OverwriteTexture(String newTexture)
    {
        TextureNames = [newTexture];

        for (var i = 0; i < Quads.Length; i++)
        {
            Quad old = Quads[i];

            Quads[i] = old with
            {
                TextureId = 0
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
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

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
        Boolean rotateTopAndBottomTexture)
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
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        BlockModel copy = new(this);

        Matrix4 rotation;
        Vector3d axis;
        Int32 rotations;

        switch (side)
        {
            case BlockSide.Front:
                return copy;

            case BlockSide.Back:
                rotation = Matrix4.CreateRotationY(MathHelper.Pi);
                axis = Vector3d.UnitY;
                rotations = 2;

                break;

            case BlockSide.Left:
                rotation = Matrix4.CreateRotationY(MathHelper.ThreePiOver2);
                axis = Vector3d.UnitY;
                rotations = 1;

                break;

            case BlockSide.Right:
                rotation = Matrix4.CreateRotationY(MathHelper.PiOver2);
                axis = Vector3d.UnitY;
                rotations = 3;

                break;

            case BlockSide.Bottom:
                rotation = Matrix4.CreateRotationX(MathHelper.PiOver2);
                axis = Vector3d.UnitX;
                rotations = 1;

                break;

            case BlockSide.Top:
                rotation = Matrix4.CreateRotationX(MathHelper.ThreePiOver2);
                axis = Vector3d.UnitX;
                rotations = 1;

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(side));
        }

        Matrix4 matrix = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * rotation *
                         Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        copy.ApplyMatrix(matrix);
        copy.RotateTextureCoordinates(axis, rotations);

        return copy;
    }

    private void ApplyMatrix(Matrix4 xyz)
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyMatrix(xyz);
    }

    private void RotateTextureCoordinates(Vector3d axis, Int32 rotations)
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].RotateTextureCoordinates(axis, rotations);
    }

    /// <summary>
    ///     Get this model as data that can be used for rendering.
    /// </summary>
    public void ToData(out BlockMesh.Quad[] quads)
    {
        if (lockedQuads != null)
        {
            quads = lockedQuads;

            return;
        }

        var textureIndexLookup = new Int32[TextureNames.Length];

        for (var i = 0; i < TextureNames.Length; i++)
            textureIndexLookup[i] = blockTextureIndexProvider.GetTextureIndex(TextureNames[i]);

        quads = new BlockMesh.Quad[Quads.Length];

        for (var index = 0; index < Quads.Length; index++)
        {
            Quad quad = Quads[index];

            quads[index].A = quad.Vert0.Position.ToVector3();
            quads[index].B = quad.Vert1.Position.ToVector3();
            quads[index].C = quad.Vert2.Position.ToVector3();
            quads[index].D = quad.Vert3.Position.ToVector3();

            Meshing.SetTextureIndex(ref quads[index].data, textureIndexLookup[quad.TextureId]);

            Meshing.SetUVs(ref quads[index].data,
                quad.Vert0.UV.ToVector2(),
                quad.Vert1.UV.ToVector2(),
                quad.Vert2.UV.ToVector2(),
                quad.Vert3.UV.ToVector2());
        }
    }

    /// <summary>
    ///     Lock the model. This will prevent modifications to the model, but combining with other models will be faster.
    /// </summary>
    public void Lock()
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        ToData(out lockedQuads);

        Debug.Assert(lockedQuads != null);
    }

    /// <summary>
    ///     Save this model to a file.
    /// </summary>
    /// <param name="directory">The directory to save the file to.</param>
    /// <param name="name">The name of the file.</param>
    public void Save(DirectoryInfo directory, String name)
    {
        if (lockedQuads != null) throw new InvalidOperationException(BlockModelIsLockedMessage);

        Exception? exception = Serialize.SaveJSON(this, directory.GetFile(GetFileName(name)));

        if (exception != null) LogFailedToSaveBlockModel(logger, exception);
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

    private static String GetFileName(String name)
    {
        return name + ".json";
    }

    private static ILoadingContext? loader;

    /// <summary>
    ///     Enable loading of models.
    /// </summary>
    /// <param name="context">The context to use for loading.</param>
    public static void EnableLoading(ILoadingContext context)
    {
        Debug.Assert(loader == null);
        loader = context;
    }

    /// <summary>
    ///     Disable loading of models. Only fallback models will be available.
    /// </summary>
    public static void DisableLoading()
    {
        Debug.Assert(loader != null);
        loader = null;
    }

    /// <summary>
    ///     Load a block model from file. All models are loaded from a specific directory.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>The loaded model.</returns>
    public static BlockModel Load(String name)
    {
        if (loader == null)
        {
            LogLoadingModelsDisabled(logger);

            return BlockModels.CreateFallback();
        }

        Exception? exception = Serialize.LoadJSON(path.GetFile(GetFileName(name)), out BlockModel model, BlockModels.CreateFallback);

        if (exception == null) loader.ReportSuccess(nameof(BlockModel), name);
        else loader.ReportWarning(nameof(BlockModel), name, exception);

        return model;
    }

    /// <summary>
    ///     Get the combined mesh of multiple block models.
    /// </summary>
    /// <param name="models">The models to combine.</param>
    /// <returns>The combined mesh.</returns>
    public static BlockMesh GetCombinedMesh(params BlockModel[] models)
    {
        Int32 totalQuadCount = models.Sum(model => model.Quads.Length);
        Boolean locked = models.Aggregate(seed: true, (current, model) => current && model.lockedQuads != null);

        if (locked)
        {
            var quads = new BlockMesh.Quad[totalQuadCount];

            var copiedQuads = 0;

            foreach (BlockMesh.Quad[]? modelQuads in models.Select(model => model.lockedQuads))
            {
                Debug.Assert(modelQuads != null);

                Array.Copy(
                    modelQuads,
                    sourceIndex: 0,
                    quads,
                    copiedQuads,
                    modelQuads.Length);

                copiedQuads += modelQuads.Length;
            }

            return new BlockMesh(quads);
        }


        List<BlockMesh.Quad> vertices = new(totalQuadCount);

        foreach (BlockModel model in models)
        {
            model.ToData(out BlockMesh.Quad[] modelQuads);
            vertices.AddRange(modelQuads);
        }

        return new BlockMesh(vertices.ToArray());
    }

    #endregion STATIC METHODS

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<BlockModel>();

    [LoggerMessage(EventId = Events.FileIO, Level = LogLevel.Warning, Message = "Failed to save block model")]
    private static partial void LogFailedToSaveBlockModel(ILogger logger, Exception exception);

    [LoggerMessage(EventId = Events.ResourceLoad, Level = LogLevel.Warning, Message = "Loading of models is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingModelsDisabled(ILogger logger);

    #endregion LOGGING
}

/// <summary>
///     A quad.
/// </summary>
public struct Quad : IEquatable<Quad>
{
    /// <summary>
    ///     The texture id used for this quad.
    /// </summary>
    public Int32 TextureId { get; set; }

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
    public Vector3d Center => (Vert0.Position + Vert1.Position + Vert2.Position + Vert3.Position) / 4;

    /// <summary>
    ///     The normal of the quad.
    /// </summary>
    private Vector3d Normal => Vector3d.Cross(Vert1.Position - Vert0.Position, Vert2.Position - Vert0.Position).Normalized();

    /// <summary>
    ///     Apply a matrix to this quad.
    /// </summary>
    /// <param name="xyz">The matrix to apply to the position.</param>
    /// <returns>The quad with the matrices applied.</returns>
    public Quad ApplyMatrix(Matrix4 xyz)
    {
        Vert0 = Vert0.ApplyMatrix(xyz);
        Vert1 = Vert1.ApplyMatrix(xyz);
        Vert2 = Vert2.ApplyMatrix(xyz);
        Vert3 = Vert3.ApplyMatrix(xyz);

        return this;
    }

    /// <summary>
    ///     Apply a rotation matrix to this quad.
    /// </summary>
    public Quad ApplyRotationMatrixY(Matrix4 xyz, Int32 rotations)
    {
        // Rotate positions.
        Vert0 = Vert0.ApplyMatrix(xyz);
        Vert1 = Vert1.ApplyMatrix(xyz);
        Vert2 = Vert2.ApplyMatrix(xyz);
        Vert3 = Vert3.ApplyMatrix(xyz);

        // Rotate UVs for top and bottom sides.
        if (Normal.Absolute().Rounded(digits: 2) == Vector3d.UnitY)
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
    public Quad RotateTextureCoordinates(Vector3d axis, Int32 rotations)
    {
        if (Normal.Absolute().Rounded(digits: 2) != axis) return this;

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
    public Boolean Equals(Quad other)
    {
        return (TextureId, Vert0, Vert1, Vert2, Vert3) ==
               (other.TextureId, other.Vert0, other.Vert1, other.Vert2, other.Vert3);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Quad other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(TextureId, Vert0, Vert1, Vert2, Vert3);
    }

    /// <summary>
    ///     Checks if two quads are equal.
    /// </summary>
    public static Boolean operator ==(Quad left, Quad right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two quads are not equal.
    /// </summary>
    public static Boolean operator !=(Quad left, Quad right)
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
    public Single X { get; set; }

    /// <summary>
    ///     The y position.
    /// </summary>
    public Single Y { get; set; }

    /// <summary>
    ///     The z position.
    /// </summary>
    public Single Z { get; set; }

    /// <summary>
    ///     The u texture coordinate.
    /// </summary>
    public Single U { get; set; }

    /// <summary>
    ///     The v texture coordinate.
    /// </summary>
    public Single V { get; set; }

    /// <summary>
    ///     The position of the vertex.
    /// </summary>
    public Vector3d Position => new(X, Y, Z);

    /// <summary>
    ///     The texture coordinates of the vertex.
    /// </summary>
    public Vector2d UV => new(U, V);

    /// <summary>
    ///     Apply a matrix to this vertex.
    /// </summary>
    public Vertex ApplyMatrix(Matrix4 xyz)
    {
        Vector4 position = new Vector4(X, Y, Z, w: 1f) * xyz;

        X = position.X;
        Y = position.Y;
        Z = position.Z;

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
    public Boolean Equals(Vertex other)
    {
        return (X, Y, Z, U, V) == (other.X, other.Y, other.Z, other.U, other.V);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Vertex other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, U, V);
    }

    /// <summary>
    ///     Checks if two vertices are equal.
    /// </summary>
    public static Boolean operator ==(Vertex left, Vertex right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two vertices are not equal.
    /// </summary>
    public static Boolean operator !=(Vertex left, Vertex right)
    {
        return !left.Equals(right);
    }
}
