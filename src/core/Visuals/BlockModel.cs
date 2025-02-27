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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A block model for complex blocks, can be loaded from disk.
/// </summary>
public sealed partial class BlockModel : IResource, ILocated
{
    private const String BlockModelIsLockedMessage = "This block model is locked and can no longer be modified.";

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
    public String[] TextureNames { get; set; } = [];

    /// <summary>
    ///     The quads that make up this model.
    /// </summary>
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "This class is meant for data storage.")]
    public Quad[] Quads { get; set; } = [];

    /// <inheritdoc />
    public static String[] Path { get; } = ["Models"];

    /// <inheritdoc />
    public static String FileExtension => "json";

    /// <inheritdoc />
    [JsonIgnore] public RID Identifier { get; private set; } = RID.Virtual;

    /// <inheritdoc />
    [JsonIgnore] public ResourceType Type => ResourceTypes.Model;

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSING

    /// <summary>
    ///     Create a block mesh from this model.
    /// </summary>
    /// <param name="textureIndexProvider">A texture index provider.</param>
    /// <returns>The block mesh.</returns>
    public BlockMesh CreateMesh(ITextureIndexProvider textureIndexProvider)
    {
        ToData(out BlockMesh.Quad[] quads, textureIndexProvider);

        return new BlockMesh(quads);
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
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

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
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        var xyz = Matrix4.CreateTranslation((Vector3) movement);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyMatrix(xyz);
    }

    /// <summary>
    ///     Rotates the model on the y axis in steps of ninety degrees.
    /// </summary>
    /// <param name="rotations">Number of rotations.</param>
    /// <param name="rotateTopAndBottomTexture">Whether the top and bottom texture should be rotated.</param>
    public void RotateY(Int32 rotations, Boolean rotateTopAndBottomTexture = true)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        if (rotations == 0) return;

        Single angle = rotations * MathHelper.PiOver2 * -1f;

        Matrix4 xyz = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * Matrix4.CreateRotationY(angle) *
                      Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        rotations = rotateTopAndBottomTexture ? 0 : rotations;

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyRotationMatrixY(xyz, rotations);
    }

    /// <summary>
    ///     Overwrites the texture of the model, replacing them with a single texture.
    ///     This is only valid for models that have a single texture.
    /// </summary>
    /// <param name="newTexture">The replacement texture.</param>
    public void OverwriteTexture(TID newTexture)
    {
        Debug.Assert(TextureNames.Length == 1);

        OverwriteTexture(newTexture, index: 0);
    }

    /// <summary>
    ///     Overwrite the texture with the given local index.
    /// </summary>
    /// <param name="newTexture">The new texture.</param>
    /// <param name="index">The index of the texture to replace.</param>
    public void OverwriteTexture(TID newTexture, Int32 index)
    {
        Debug.Assert(index >= 0 && index < TextureNames.Length);

        TextureNames[index] = newTexture.Key;
    }

    /// <summary>
    ///     Creates six models, one for each block side, from a north oriented model.
    /// </summary>
    /// <returns> The six models.</returns>
    public (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
        CreateAllSides()
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top)
            result;

        result.front = this;

        result.back = CreateSideModel(Side.Back);
        result.left = CreateSideModel(Side.Left);
        result.right = CreateSideModel(Side.Right);
        result.bottom = CreateSideModel(Side.Bottom);
        result.top = CreateSideModel(Side.Top);

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

        result.x = CreateSideModel(Side.Left);
        result.y = CreateSideModel(Side.Bottom);

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

    private BlockModel CreateSideModel(Side side)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        BlockModel copy = new(this);

        Matrix4 rotation;
        Vector3d axis;
        Int32 rotations;

        switch (side)
        {
            case Side.Front:
                return copy;

            case Side.Back:
                rotation = Matrix4.CreateRotationY(MathHelper.Pi);
                axis = Vector3d.UnitY;
                rotations = 2;

                break;

            case Side.Left:
                rotation = Matrix4.CreateRotationY(MathHelper.ThreePiOver2);
                axis = Vector3d.UnitY;
                rotations = 1;

                break;

            case Side.Right:
                rotation = Matrix4.CreateRotationY(MathHelper.PiOver2);
                axis = Vector3d.UnitY;
                rotations = 3;

                break;

            case Side.Bottom:
                rotation = Matrix4.CreateRotationX(MathHelper.PiOver2);
                axis = Vector3d.UnitX;
                rotations = 1;

                break;

            case Side.Top:
                rotation = Matrix4.CreateRotationX(MathHelper.ThreePiOver2);
                axis = Vector3d.UnitX;
                rotations = 1;

                break;

            default:
                throw Exceptions.UnsupportedEnumValue(side);
        }

        Matrix4 matrix = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * rotation *
                         Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        copy.ApplyMatrix(matrix);
        copy.RotateTextureCoordinates(axis, rotations);

        return copy;
    }

    private void ApplyMatrix(Matrix4 xyz)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyMatrix(xyz);
    }

    private void RotateTextureCoordinates(Vector3d axis, Int32 rotations)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].RotateTextureCoordinates(axis, rotations);
    }

    /// <summary>
    ///     Get this model as data that can be used for rendering.
    /// </summary>
    public void ToData(out BlockMesh.Quad[] quads, ITextureIndexProvider textureIndexProvider)
    {
        if (lockedQuads != null)
        {
            quads = lockedQuads;

            return;
        }

        var textureIndexLookup = new Int32[TextureNames.Length];

        for (var i = 0; i < TextureNames.Length; i++)
            textureIndexLookup[i] = textureIndexProvider.GetTextureIndex(TID.FromString(TextureNames[i], isBlock: true));

        quads = new BlockMesh.Quad[Quads.Length];

        for (var index = 0; index < Quads.Length; index++)
        {
            Quad quad = Quads[index];

            quads[index].A = (Vector3) quad.Vert0.Position;
            quads[index].B = (Vector3) quad.Vert1.Position;
            quads[index].C = (Vector3) quad.Vert2.Position;
            quads[index].D = (Vector3) quad.Vert3.Position;

            Meshing.SetTextureIndex(ref quads[index].data, textureIndexLookup[quad.TextureId]);

            Meshing.SetUVs(ref quads[index].data,
                (Vector2) quad.Vert0.UV,
                (Vector2) quad.Vert1.UV,
                (Vector2) quad.Vert2.UV,
                (Vector2) quad.Vert3.UV);
        }
    }

    /// <summary>
    ///     Lock the model. This will prevent modifications to the model, but combining with other models will be faster.
    /// </summary>
    public void Lock(ITextureIndexProvider textureIndexProvider)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        ToData(out lockedQuads, textureIndexProvider);

        Debug.Assert(lockedQuads != null);
    }

    /// <summary>
    ///     Save this model to a file.
    /// </summary>
    /// <param name="directory">The directory to save the file to.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task SaveAsync(DirectoryInfo directory, String name, CancellationToken token = default)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(BlockModelIsLockedMessage);

        Result result = await Serialize.SaveJsonAsync(this, directory.GetFile(FileSystem.GetResourceFileName<BlockModel>(name)), token).InAnyContext();

        result.Switch(
            () => {},
            exception => LogFailedToSaveBlockModel(logger, exception));
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
    ///     Load a block model from a file.
    /// </summary>
    /// <param name="file">The file to load the model from.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result<BlockModel>> LoadAsync(FileInfo file, CancellationToken token = default)
    {
        Result<BlockModel> result = await Serialize.LoadJsonAsync<BlockModel>(file, token).InAnyContext();

        return result.Map(model =>
        {
            model.Identifier = RID.Path(file);

            return model;
        });
    }

    /// <summary>
    ///     Get the combined mesh of multiple block models.
    /// </summary>
    /// <param name="models">The models to combine.</param>
    /// <param name="textureIndexProvider">The texture index provider.</param>
    /// <returns>The combined mesh.</returns>
    public static BlockMesh GetCombinedMesh(ITextureIndexProvider textureIndexProvider, params BlockModel[] models)
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
            model.ToData(out BlockMesh.Quad[] modelQuads, textureIndexProvider);
            vertices.AddRange(modelQuads);
        }

        return new BlockMesh(vertices.ToArray());
    }

    #endregion STATIC METHODS

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<BlockModel>();

    [LoggerMessage(EventId = LogID.BlockModel + 0, Level = LogLevel.Warning, Message = "Failed to save block model")]
    private static partial void LogFailedToSaveBlockModel(ILogger logger, Exception exception);

    [LoggerMessage(EventId = LogID.BlockModel + 1, Level = LogLevel.Warning, Message = "Loading of models is currently disabled, fallback will be used instead")]
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
