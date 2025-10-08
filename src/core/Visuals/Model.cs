// <copyright file="Model.cs" company="VoxelGame">
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
///     A model for complex blocks and other modelled things, can be loaded from disk.
/// </summary>
public sealed partial class Model : IResource, ILocated
{
    private const String ModelIsLockedMessage = "This model is locked and can no longer be modified.";

    private Mesh.Quad[]? lockedQuads;

    /// <summary>
    ///     Create an empty model.
    /// </summary>
    public Model() {}

    /// <summary>
    ///     Copy-constructor.
    /// </summary>
    /// <param name="original">The original model to copy.</param>
    private Model(Model original)
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

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Create a mesh from this model.
    /// </summary>
    /// <param name="textureIndexProvider">A texture index provider.</param>
    /// <param name="textureOverrides">
    ///     Optional texture overrides, using by-index substitution. A minus one key will replace
    ///     all textures that are not explicitly named.
    /// </param>
    /// <returns>The mesh.</returns>
    public Mesh CreateMesh(ITextureIndexProvider textureIndexProvider, IReadOnlyDictionary<Int32, TID>? textureOverrides = null)
    {
        ToData(out Mesh.Quad[] quads, textureIndexProvider, textureOverrides);

        return new Mesh(quads);
    }

    /// <summary>
    ///     Get the axis-aligned bounding box of this model.
    /// </summary>
    /// <returns>The bounding box.</returns>
    public Box3d GetBounds()
    {
        if (Quads.Length == 0)
            return new Box3d();

        Vector3d min = Quads[0].Vert0.Position;
        Vector3d max = Quads[0].Vert0.Position;

        foreach (Quad quad in Quads)
        {
            foreach (Vertex vert in (Vertex[]) [quad.Vert0, quad.Vert1, quad.Vert2, quad.Vert3])
            {
                min = Vector3d.ComponentMin(min, vert.Position);
                max = Vector3d.ComponentMax(max, vert.Position);
            }
        }

        return new Box3d(min, max);
    }

    /// <summary>
    ///     Partitions the model into block-sized parts.
    ///     This method does not cut individual quads, it only sorts them into the parts they fully belong to.
    ///     Quads are also moved so that the part they belong to has its origin at (0,0,0).
    /// </summary>
    /// <returns>The parts of the model.</returns>
    public Model[,,] PartitionByBlocks()
    {
        // todo: this is a bit ugly and could maybe sometimes put quads into the wrong part
        // todo: so add to the note for the custom editor that models should already define the separation in the format
        // todo: so the format would be changed and the editor would help with displaying and configuring that

        Box3d bounds = GetBounds();

        var sizeX = (Int32) Math.Ceiling(bounds.Size.X);
        var sizeY = (Int32) Math.Ceiling(bounds.Size.Y);
        var sizeZ = (Int32) Math.Ceiling(bounds.Size.Z);

        var partQuads = new List<Quad>[sizeX, sizeY, sizeZ];

        for (var x = 0; x < sizeX; x++)
        for (var y = 0; y < sizeY; y++)
        for (var z = 0; z < sizeZ; z++)
            partQuads[x, y, z] = [];

        foreach (Quad quad in Quads)
        {
            Vector3i target = quad.Center.Floor();

            Int32 x = Math.Clamp(target.X, min: 0, sizeX - 1);
            Int32 y = Math.Clamp(target.Y, min: 0, sizeY - 1);
            Int32 z = Math.Clamp(target.Z, min: 0, sizeZ - 1);

            partQuads[x, y, z].Add(quad);
        }

        var parts = new Model[sizeX, sizeY, sizeZ];

        for (var x = 0; x < sizeX; x++)
        for (var y = 0; y < sizeY; y++)
        for (var z = 0; z < sizeZ; z++)
        {
            List<Quad> quads = partQuads[x, y, z];

            var translation = Matrix4.CreateTranslation(-x, -y, -z);

            for (var index = 0; index < quads.Count; index++)
            {
                quads[index] = quads[index].ApplyMatrix(translation);
            }

            parts[x, y, z] = new Model
            {
                TextureNames = TextureNames.ToArray(),
                Quads = quads.ToArray()
            };
        }

        return parts;
    }

    /// <summary>
    ///     Rotates the model on the y-axis in steps of ninety degrees.
    /// </summary>
    /// <param name="rotations">Number of rotations.</param>
    /// <param name="rotateTopAndBottomTexture">Whether the top and bottom texture should be rotated.</param>
    public void RotateY(Int32 rotations, Boolean rotateTopAndBottomTexture = true)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

        if (rotations == 0) return;

        Single angle = rotations * MathHelper.PiOver2 * -1f;

        Matrix4 xyz = Matrix4.CreateTranslation(x: -0.5f, y: -0.5f, z: -0.5f) * Matrix4.CreateRotationY(angle) *
                      Matrix4.CreateTranslation(x: 0.5f, y: 0.5f, z: 0.5f);

        rotations = rotateTopAndBottomTexture ? 0 : rotations;

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyRotationMatrixY(xyz, rotations);
    }

    /// <summary>
    ///     Creates six models, one for each block side, from a north oriented model.
    /// </summary>
    /// <returns> The six models.</returns>
    public (Model front, Model back, Model left, Model right, Model bottom, Model top)
        CreateAllSides()
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

        (Model front, Model back, Model left, Model right, Model bottom, Model top)
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
    public (Model x, Model y, Model z) CreateAllAxis()
    {
        (Model x, Model y, Model z) result;

        result.z = this;

        result.x = CreateSideModel(Side.Left);
        result.y = CreateSideModel(Side.Bottom);

        return result;
    }

    // todo: unify the orientation based and the side based rotations, sided might be better overall but needs param whether to rotate textures too

    /// <summary>
    ///     Create models for each orientation.
    /// </summary>
    /// <param name="rotateTopAndBottomTexture">Whether the top and bottom textures should be rotated.</param>
    /// <returns>All model versions.</returns>
    public (Model north, Model east, Model south, Model west) CreateAllOrientations(
            Boolean rotateTopAndBottomTexture)
        // todo: find out when and why this parameter is used, maybe an abstraction is possible
        // todo: probably for all blocks that use Modelled it can be true and for all that combine meshes on their own it can be false
    {
        Model north = this;

        Model east = new(north);
        east.RotateY(rotations: 1, rotateTopAndBottomTexture);

        Model south = new(east);
        south.RotateY(rotations: 1, rotateTopAndBottomTexture);

        Model west = new(south);
        west.RotateY(rotations: 1, rotateTopAndBottomTexture);

        return (north, east, south, west);
    }

    private Model CreateSideModel(Side side)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

        Model copy = new(this);

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
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].ApplyMatrix(xyz);
    }

    private void RotateTextureCoordinates(Vector3d axis, Int32 rotations)
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

        for (var i = 0; i < Quads.Length; i++) Quads[i] = Quads[i].RotateTextureCoordinates(axis, rotations);
    }

    /// <summary>
    ///     Get this model as data that can be used for rendering.
    /// </summary>
    /// <param name="quads">The quads of the model.</param>
    /// <param name="textureIndexProvider">A texture index provider.</param>
    /// <param name="textureOverrides">
    ///     Optional texture overrides, using by-index substitution. A minus one key will replace
    ///     all textures that are not explicitly named.
    /// </param>
    public void ToData(out Mesh.Quad[] quads, ITextureIndexProvider textureIndexProvider, IReadOnlyDictionary<Int32, TID>? textureOverrides = null)
    {
        if (lockedQuads != null)
        {
            quads = lockedQuads;

            return;
        }

        var textureIndexLookup = new Int32[TextureNames.Length];

        for (var texture = 0; texture < TextureNames.Length; texture++)
        {
            TID id = TID.FromString(TextureNames[texture], isBlock: true);

            if (textureOverrides != null)
            {
                if (textureOverrides.TryGetValue(texture, out TID overrideId) ||
                    textureOverrides.TryGetValue(key: -1, out overrideId))
                {
                    id = overrideId;
                }
            }

            textureIndexLookup[texture] = textureIndexProvider.GetTextureIndex(id);
        }

        quads = new Mesh.Quad[Quads.Length];

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
    public void Lock(ITextureIndexProvider textureIndexProvider) // todo: remove this whole thing
    {
        if (lockedQuads != null)
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

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
            throw Exceptions.InvalidOperation(ModelIsLockedMessage);

        Result result = await Serialize.SaveJsonAsync(this, directory.GetFile(FileSystem.GetResourceFileName<Model>(name)), token).InAnyContext();

        result.Switch(
            () => {},
            exception => LogFailedToSaveModel(logger, exception));
    }

    /// <summary>
    ///     Get a copy of this model.
    /// </summary>
    /// <returns>The copy.</returns>
    public Model Copy()
    {
        return new Model(this);
    }

    #region STATIC METHODS

    /// <summary>
    ///     Load a model from a file.
    /// </summary>
    /// <param name="file">The file to load the model from.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result<Model>> LoadAsync(FileInfo file, CancellationToken token = default)
    {
        Result<Model> result = await Serialize.LoadJsonAsync<Model>(file, token).InAnyContext();

        return result.Map(model =>
        {
            model.Identifier = RID.Path(file);

            return model;
        });
    }

    /// <summary>
    ///     Get the combined mesh of multiple models.
    /// </summary>
    /// <param name="models">The models to combine.</param>
    /// <param name="textureIndexProvider">The texture index provider.</param>
    /// <returns>The combined mesh.</returns>
    public static Mesh GetCombinedMesh(ITextureIndexProvider textureIndexProvider, params Model[] models) // todo: should return model and not mesh, use override then
    {
        Int32 totalQuadCount = models.Sum(model => model.Quads.Length);
        Boolean locked = models.Aggregate(seed: true, (current, model) => current && model.lockedQuads != null);

        if (locked)
        {
            var quads = new Mesh.Quad[totalQuadCount];

            var copiedQuads = 0;

            foreach (Mesh.Quad[]? modelQuads in models.Select(model => model.lockedQuads))
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

            return new Mesh(quads);
        }


        List<Mesh.Quad> vertices = new(totalQuadCount);

        foreach (Model model in models)
        {
            model.ToData(out Mesh.Quad[] modelQuads, textureIndexProvider);
            vertices.AddRange(modelQuads);
        }

        return new Mesh(vertices.ToArray());
    }

    #endregion STATIC METHODS

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Model>();

    [LoggerMessage(EventId = LogID.Model + 0, Level = LogLevel.Warning, Message = "Failed to save model")]
    private static partial void LogFailedToSaveModel(ILogger logger, Exception exception);

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
