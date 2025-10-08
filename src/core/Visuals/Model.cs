// <copyright file="Model.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
///     Models support many operations to create variants of a base model.
///     These operations can be costly and should be done during loading, not during gameplay.
///     At the end of loading, all models should be converted to meshes using <see cref="CreateMesh" />.
///
///     This class should not be mutated as it may be shared through caching.
/// </summary>
public sealed partial class Model : IResource, ILocated
{
    /// <summary>
    ///     Create an empty model.
    /// </summary>
    public Model() : this([], []) {}

    /// <summary>
    ///     Copy-constructor.
    /// </summary>
    /// <param name="original">The original model to copy.</param>
    private Model(Model original) : this(original.TextureNames, original.Quads) {}
    
    [JsonConstructor]
    private Model(ImmutableArray<String> textureNames, ImmutableArray<Quad> quads)
    {
        TextureNames = textureNames.IsDefault ? [] : textureNames;
        Quads = quads.IsDefault ? [] : quads;
    }

    /// <summary>
    ///     The names of the textures used by this model.
    /// </summary>
    [JsonInclude]
    public ImmutableArray<String> TextureNames { get; private set; }

    /// <summary>
    ///     The quads that make up this model.
    /// </summary>
    [JsonInclude]
    public ImmutableArray<Quad> Quads { get; private set; }

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
        Box3d bounds = GetBounds();

        Vector3i size = bounds.Size.Ceiling();

        if (size.X <= 0 || size.Y <= 0 || size.Z <= 0)
            return new Model[0, 0, 0];

        var partQuads = new List<Quad>[size.X, size.Y, size.Z];

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
            partQuads[x, y, z] = [];

        foreach (Quad quad in Quads)
        {
            Vector3i target = DetermineTargetCell(quad, size);

            partQuads[target.X, target.Y, target.Z].Add(quad);
        }

        var parts = new Model[size.X, size.Y, size.Z];

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
        for (var z = 0; z < size.Z; z++)
        {
            List<Quad> quads = partQuads[x, y, z];

            if (quads.Count == 0)
            {
                parts[x, y, z] = new Model(TextureNames, []);

                continue;
            }

            var translation = Matrix4.CreateTranslation(-x, -y, -z);

            for (var index = 0; index < quads.Count; index++)
            {
                quads[index] = quads[index].ApplyMatrix(translation);
            }
            
            parts[x, y, z] = new Model(TextureNames, [..quads]);
        }

        return parts;
    }

    private static Vector3i DetermineTargetCell(Quad quad, Vector3i size)
    {
        const Double tolerance = 1e-6;

        ReadOnlySpan<Vertex> vertices =
        [
            quad.Vert0,
            quad.Vert1,
            quad.Vert2,
            quad.Vert3
        ];

        Vector3d min = vertices[0].Position;
        Vector3d max = min;

        for (var index = 1; index < vertices.Length; index++)
        {
            Vector3d position = vertices[index].Position;

            min = Vector3d.ComponentMin(min, position);
            max = Vector3d.ComponentMax(max, position);
        }

        Vector3i start = min.Floor();
        Vector3i end = max.Floor();

        for (Int32 x = start.X; x <= end.X; x++)
        for (Int32 y = start.Y; y <= end.Y; y++)
        for (Int32 z = start.Z; z <= end.Z; z++)
            if (IsQuadWithinCell(vertices, (x, y, z), tolerance))
                return new Vector3i(x, y, z).ClampComponents(Vector3i.Zero, size - Vector3i.One);
        
        return quad.Center.Floor().ClampComponents(Vector3i.Zero, size - Vector3i.One);
    }

    private static Boolean IsQuadWithinCell(ReadOnlySpan<Vertex> vertices, Vector3i cell, Double tolerance)
    {
        Vector3d min = cell - new Vector3d(tolerance);
        Vector3d max = cell + new Vector3d(1 + tolerance);

        foreach (Vertex vertex in vertices)
        {
            Vector3d position = vertex.Position;

            if (position.X < min.X || position.X > max.X) return false;
            if (position.Y < min.Y || position.Y > max.Y) return false;
            if (position.Z < min.Z || position.Z > max.Z) return false;
        }

        return true;
    }

    /// <summary>
    /// How model transformations, e.g. <see cref="CreateModelForSide(Side, TransformationMode)"/>, should be performed.
    /// </summary>
    public enum TransformationMode
    {
        /// <summary>
        /// The object is rotated, meaning the texture seems to visually rotate with the object.
        /// To achieve this, the texture coordinates are kept unchanged while the vertex positions are rotated.
        /// This solution is preferable for most cases, especially when the model represents a complete object.
        /// </summary>
        Rotate,
        
        /// <summary>
        /// The object is reshaped, meaning the texture seems to stay in place while the object is rotated.
        /// To achieve this, the texture coordinates are rotated accordingly.
        /// This solution is used in some special cases, e.g. when the model represents a part of a larger object and is combined with other models.
        /// </summary>
        Reshape
    }
    
    /// <summary>
    /// Create a model for the given orientation, under the assumption that the original model is aligned with the north orientation.
    /// </summary>
    /// <param name="orientation">The orientation to create the model for.</param>
    /// <param name="mode">The transformation mode to use.</param>
    /// <returns>The model for the given orientation.</returns>
    public Model CreateModelForOrientation(Orientation orientation, TransformationMode mode = TransformationMode.Rotate)
    {
        return CreateModelForSide(orientation.ToSide().Opposite(), mode);
    }

    /// <summary>
    /// Create a model for the given axis, under the assumption that the original model is aligned with the Z axis.
    /// </summary>
    /// <param name="axis">The axis to create the model for.</param>
    /// <param name="mode">The transformation mode to use.</param>
    /// <returns>The model for the given axis.</returns>
    public Model CreateModelForAxis(Axis axis, TransformationMode mode = TransformationMode.Rotate)
    {
        return axis switch
        {
            Axis.Z => this,
            
            Axis.X => CreateModelForSide(Side.Left, mode),
            Axis.Y => CreateModelForSide(Side.Bottom, mode),
            
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }
    
    /// <summary>
    /// Create a model for the given side, under the assumption that the original model is for the front side.
    /// </summary>
    /// <param name="side">The side to create the model for.</param>
    /// <param name="mode">The transformation mode to use.</param>
    /// <returns>The model for the given side.</returns>
    public Model CreateModelForSide(Side side, TransformationMode mode = TransformationMode.Rotate)
    {
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
        
        if (mode == TransformationMode.Rotate && axis == Vector3d.UnitY)
            rotations = 0;

        copy.ApplyMatrix(matrix);
        copy.RotateTextureCoordinates(axis, rotations);

        return copy;
    }

    private void ApplyMatrix(Matrix4 xyz)
    {
        ImmutableArray<Quad>.Builder builder = Quads.ToBuilder();

        for (var i = 0; i < builder.Count; i++) builder[i] = builder[i].ApplyMatrix(xyz);

        Quads = builder.ToImmutable();
    }

    private void RotateTextureCoordinates(Vector3d axis, Int32 rotations)
    {
        ImmutableArray<Quad>.Builder builder = Quads.ToBuilder();

        for (var i = 0; i < builder.Count; i++) builder[i] = builder[i].RotateTextureCoordinates(axis, rotations);

        Quads = builder.ToImmutable();
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
    private void ToData(out Mesh.Quad[] quads, ITextureIndexProvider textureIndexProvider, IReadOnlyDictionary<Int32, TID>? textureOverrides = null)
    {
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
    ///     Save this model to a file.
    /// </summary>
    /// <param name="directory">The directory to save the file to.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task SaveAsync(DirectoryInfo directory, String name, CancellationToken token = default)
    {
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
    
    /// <summary>
    ///     Create a fallback model.
    ///     It does not rely on any textures and can be safely used when resources are not available.
    /// </summary>
    /// <returns>The fallback model.</returns>
    public static Model CreateFallback()
    {
        const Single begin = 0.275f;
        const Single size = 0.5f;

        Int32[][] uvs = Meshes.GetBlockUVs(isRotated: false);

        return new Model([TID.MissingTextureKey], [
                BuildQuad(Side.Front),
                BuildQuad(Side.Back),
                BuildQuad(Side.Left),
                BuildQuad(Side.Right),
                BuildQuad(Side.Bottom),
                BuildQuad(Side.Top)
            ]);

        Quad BuildQuad(Side side)
        {
            side.Corners(out Int32[] a, out Int32[] b, out Int32[] c, out Int32[] d);

            return new Quad
            {
                TextureId = 0,
                Vert0 = BuildVertex(a, uvs[0]),
                Vert1 = BuildVertex(b, uvs[1]),
                Vert2 = BuildVertex(c, uvs[2]),
                Vert3 = BuildVertex(d, uvs[3])
            };

            Vertex BuildVertex(IReadOnlyList<Int32> corner, IReadOnlyList<Int32> uv)
            {
                return new Vertex
                {
                    X = begin + corner[index: 0] * size,
                    Y = begin + corner[index: 1] * size,
                    Z = begin + corner[index: 2] * size,
                    U = begin + uv[index: 0] * size,
                    V = begin + uv[index: 1] * size
                };
            }
        }
    }
    
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
    /// Combine a set of models into a single model.
    /// If only a single model is provided, it is simply returned without copying.
    /// If no models are provided, an empty model is returned.
    /// </summary>
    /// <param name="models">All models to combine.</param>
    /// <returns>The combined model.</returns>
    public static Model Combine(params IEnumerable<Model> models)
    {
        Model[] array = models.ToArray();
        
        switch (array)
        {
            case []:
                return new Model();
            
            case [var model]:
                return model;
        }

        Dictionary<String, Int32> textureNameToIndex = new();
        List<String> textureNames = [];
        
        List<Quad> quads = [];
        
        foreach (Model model in array)
        {
            foreach (String textureName in model.TextureNames)
            {
                if (textureNameToIndex.TryAdd(textureName, textureNames.Count))
                {
                    textureNames.Add(textureName);
                }
            }
            
            foreach (Quad quad in model.Quads)
            {
                Quad newQuad = quad;

                newQuad.TextureId = textureNameToIndex[model.TextureNames[quad.TextureId]];

                quads.Add(newQuad);
            }
        }
        
        return new Model([..textureNames], [..quads]);
    }

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
