// <copyright file="BlockModel.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Visuals
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This class is meant for data storage.")]
    public sealed class BlockModel
    {
        private const string BlockModelIsLockedMessage = "This block model is locked and can no longer be modified.";

        private static readonly ILogger Logger = LoggingHelper.CreateLogger<BlockModel>();

        private static readonly string Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Models");

        private static ITextureIndexProvider _blockTextureIndexProvider = null!;

        public static void SetBlockTextureIndexProvider(ITextureIndexProvider blockTextureIndexProvider)
        {
            _blockTextureIndexProvider = blockTextureIndexProvider;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // Has to be public for serialization.
        public string[] TextureNames { get; set; } = Array.Empty<string>();

        public Quad[] Quads { get; set; } = Array.Empty<Quad>();

        public int VertexCount => Quads.Length * 4;

        private bool isLocked;

        private float[] lockedVertices = null!;
        private int[] lockedTextureIndices = null!;
        private uint[] lockedIndices = null!;

        public BlockModel()
        {
        }

        /// <summary>
        /// Copy-constructor.
        /// </summary>
        /// <param name="original">The original model to copy.</param>
        private BlockModel(BlockModel original)
        {
            this.TextureNames = (string[])original.TextureNames.Clone();
            this.Quads = (Quad[])original.Quads.Clone();
        }

        /// <summary>
        /// Splits the BlockModel into two parts, using a given plane to sort all faces.
        /// </summary>
        /// <param name="position">Position of the plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="a">The first model.</param>
        /// <param name="b">The second model.</param>
        public void PlaneSplit(Vector3 position, Vector3 normal, out BlockModel a, out BlockModel b)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            normal = normal.Normalized();
            List<Quad> quadsA = new List<Quad>();
            List<Quad> quadsB = new List<Quad>();

            foreach (Quad quad in Quads)
            {
                if (Vector3.Dot(quad.Center - position, normal) > 0)
                {
                    quadsA.Add(quad);
                }
                else
                {
                    quadsB.Add(quad);
                }
            }

            a = new BlockModel() { TextureNames = this.TextureNames };
            b = new BlockModel() { TextureNames = this.TextureNames };

            a.Quads = quadsA.ToArray();
            b.Quads = quadsB.ToArray();
        }

        /// <summary>
        /// Moves all vertices of this model.
        /// </summary>
        /// <param name="movement"></param>
        public void Move(Vector3 movement)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            Matrix4 xyz = Matrix4.CreateTranslation(movement);

            for (var i = 0; i < Quads.Length; i++)
            {
                Quads[i] = Quads[i].ApplyTranslationMatrix(xyz);
            }
        }

        /// <summary>
        /// Rotates the model on the y axis in steps of ninety degrees.
        /// </summary>
        /// <param name="rotations">Number of rotations.</param>
        /// <param name="rotateTopAndBottomTexture">Whether the top and bottom texture should be rotated.</param>
        public void RotateY(int rotations, bool rotateTopAndBottomTexture = true)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            if (rotations == 0)
            {
                return;
            }

            float angle = rotations * MathHelper.PiOver2 * -1f;

            Matrix4 xyz = Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f) * Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            Matrix4 nop = Matrix4.CreateRotationY(angle);

            rotations = rotateTopAndBottomTexture ? 0 : rotations;

            for (var i = 0; i < Quads.Length; i++)
            {
                Quads[i] = Quads[i].ApplyRotationMatrixY(xyz, nop, rotations);
            }
        }

        /// <summary>
        /// Creates six models, one for each block side, from a north oriented model.
        /// </summary>
        /// <returns> The six models.</returns>
        public (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) CreateAllSides()
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) result;

            result.front = this;

            result.back = CreateSideModel(BlockSide.Back);
            result.left = CreateSideModel(BlockSide.Left);
            result.right = CreateSideModel(BlockSide.Right);
            result.bottom = CreateSideModel(BlockSide.Bottom);
            result.top = CreateSideModel(BlockSide.Top);

            return result;
        }

        public (BlockModel x, BlockModel y, BlockModel z) CreateAllAxis()
        {
            (BlockModel x, BlockModel y, BlockModel z) result;

            result.z = this;

            result.x = CreateSideModel(BlockSide.Left);
            result.y = CreateSideModel(BlockSide.Bottom);

            return result;
        }

        public (BlockModel north, BlockModel east, BlockModel south, BlockModel west) CreateAllDirections(bool rotateTopAndBottomTexture)
        {
            BlockModel north = this;

            BlockModel east = new BlockModel(north);
            east.RotateY(1, rotateTopAndBottomTexture);

            BlockModel south = new BlockModel(east);
            south.RotateY(1, rotateTopAndBottomTexture);

            BlockModel west = new BlockModel(south);
            west.RotateY(1, rotateTopAndBottomTexture);

            return (north, east, south, west);
        }

        private BlockModel CreateSideModel(BlockSide side)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            BlockModel copy = new BlockModel(this);

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

            Matrix4 matrix = Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f) * rotation * Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            copy.ApplyMatrix(matrix, rotation);
            copy.RotateTextureCoordinates(axis, rotations);

            return copy;
        }

        private void ApplyMatrix(Matrix4 xyz, Matrix4 nop)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            for (var i = 0; i < Quads.Length; i++)
            {
                Quads[i] = Quads[i].ApplyMatrix(xyz, nop);
            }
        }

        private void RotateTextureCoordinates(Vector3 axis, int rotations)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            for (var i = 0; i < Quads.Length; i++)
            {
                Quads[i] = Quads[i].RotateTextureCoordinates(axis, rotations);
            }
        }

        public void ToData(out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            if (isLocked)
            {
                vertices = lockedVertices;
                textureIndices = lockedTextureIndices;
                indices = lockedIndices;

                return;
            }

            int[] texIndexLookup = new int[TextureNames.Length];

            for (var i = 0; i < TextureNames.Length; i++)
            {
                texIndexLookup[i] = _blockTextureIndexProvider.GetTextureIndex(TextureNames[i]);
            }

            vertices = new float[Quads.Length * 32];
            textureIndices = new int[Quads.Length * 4];

            for (var q = 0; q < Quads.Length; q++)
            {
                Quad quad = Quads[q];

                // Vertex 0.
                vertices[(q * 32) + 0] = quad.Vert0.X;
                vertices[(q * 32) + 1] = quad.Vert0.Y;
                vertices[(q * 32) + 2] = quad.Vert0.Z;
                vertices[(q * 32) + 3] = MathHelper.Clamp(quad.Vert0.U, 0f, 1f);
                vertices[(q * 32) + 4] = MathHelper.Clamp(quad.Vert0.V, 0f, 1f);
                vertices[(q * 32) + 5] = MathHelper.Clamp(quad.Vert0.N, -1f, 1f);
                vertices[(q * 32) + 6] = MathHelper.Clamp(quad.Vert0.O, -1f, 1f);
                vertices[(q * 32) + 7] = MathHelper.Clamp(quad.Vert0.P, -1f, 1f);

                textureIndices[(q * 4) + 0] = texIndexLookup[quad.TextureId];

                // Vertex 1.
                vertices[(q * 32) + 8] = quad.Vert1.X;
                vertices[(q * 32) + 9] = quad.Vert1.Y;
                vertices[(q * 32) + 10] = quad.Vert1.Z;
                vertices[(q * 32) + 11] = MathHelper.Clamp(quad.Vert1.U, 0f, 1f);
                vertices[(q * 32) + 12] = MathHelper.Clamp(quad.Vert1.V, 0f, 1f);
                vertices[(q * 32) + 13] = MathHelper.Clamp(quad.Vert1.N, -1f, 1f);
                vertices[(q * 32) + 14] = MathHelper.Clamp(quad.Vert1.O, -1f, 1f);
                vertices[(q * 32) + 15] = MathHelper.Clamp(quad.Vert1.P, -1f, 1f);

                textureIndices[(q * 4) + 1] = texIndexLookup[quad.TextureId];

                // Vertex 2.
                vertices[(q * 32) + 16] = quad.Vert2.X;
                vertices[(q * 32) + 17] = quad.Vert2.Y;
                vertices[(q * 32) + 18] = quad.Vert2.Z;
                vertices[(q * 32) + 19] = MathHelper.Clamp(quad.Vert2.U, 0f, 1f);
                vertices[(q * 32) + 20] = MathHelper.Clamp(quad.Vert2.V, 0f, 1f);
                vertices[(q * 32) + 21] = MathHelper.Clamp(quad.Vert2.N, -1f, 1f);
                vertices[(q * 32) + 22] = MathHelper.Clamp(quad.Vert2.O, -1f, 1f);
                vertices[(q * 32) + 23] = MathHelper.Clamp(quad.Vert2.P, -1f, 1f);

                textureIndices[(q * 4) + 2] = texIndexLookup[quad.TextureId];

                // Vertex 3.
                vertices[(q * 32) + 24] = quad.Vert3.X;
                vertices[(q * 32) + 25] = quad.Vert3.Y;
                vertices[(q * 32) + 26] = quad.Vert3.Z;
                vertices[(q * 32) + 27] = MathHelper.Clamp(quad.Vert3.U, 0f, 1f);
                vertices[(q * 32) + 28] = MathHelper.Clamp(quad.Vert3.V, 0f, 1f);
                vertices[(q * 32) + 29] = MathHelper.Clamp(quad.Vert3.N, -1f, 1f);
                vertices[(q * 32) + 30] = MathHelper.Clamp(quad.Vert3.O, -1f, 1f);
                vertices[(q * 32) + 31] = MathHelper.Clamp(quad.Vert3.P, -1f, 1f);

                textureIndices[(q * 4) + 3] = texIndexLookup[quad.TextureId];
            }

            indices = new uint[Quads.Length * 6];

            for (int i = 0; i < Quads.Length; i++)
            {
                uint offset = (uint)(i * 4);

                indices[(i * 6) + 0] = 0 + offset;
                indices[(i * 6) + 1] = 2 + offset;
                indices[(i * 6) + 2] = 1 + offset;
                indices[(i * 6) + 3] = 0 + offset;
                indices[(i * 6) + 4] = 3 + offset;
                indices[(i * 6) + 5] = 2 + offset;
            }
        }

        public void Lock()
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            ToData(out lockedVertices, out lockedTextureIndices, out lockedIndices);

            isLocked = true;
        }

        public void Save(string name)
        {
            if (isLocked) throw new InvalidOperationException(BlockModelIsLockedMessage);

            JsonSerializerOptions options = new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true };

            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(System.IO.Path.Combine(Path, name + ".json"), json);
        }

        #region STATIC METHODS

        public static BlockModel Load(string name)
        {
            try
            {
                string json = File.ReadAllText(System.IO.Path.Combine(Path, name + ".json"));
                BlockModel model = JsonSerializer.Deserialize<BlockModel>(json) ?? new BlockModel();

                Logger.LogDebug("Loaded BlockModel: {name}", name);

                return model;
            }
            catch (Exception e) when (e is IOException || e is FileNotFoundException || e is JsonException)
            {
                Logger.LogWarning(Events.MissingResource, e, "Could not load the model '{name}' because an exception occurred, a fallback will be used instead.", name);

                return CreateFallback();
            }
        }

        private static BlockModel CreateFallback()
        {
            return new BlockModel
            {
                TextureNames = new string[] { "missing_texture" },
                Quads = new Quad[]
                {
                    new Quad { Vert0 = new Vertex { X = 0.375f, Y = 0.375f, Z = 0.375f, U = 0.375f, V = 0.000f, N = -1f, O = 0f, P = 0f},
                               Vert1 = new Vertex { X = 0.375f, Y = 0.625f, Z = 0.375f, U = 0.625f, V = 0.000f, N = -1f, O = 0f, P = 0f},
                               Vert2 = new Vertex { X = 0.375f, Y = 0.625f, Z = 0.625f, U = 0.625f, V = 0.250f, N = -1f, O = 0f, P = 0f},
                               Vert3 = new Vertex { X = 0.375f, Y = 0.375f, Z = 0.625f, U = 0.375f, V = 0.250f, N = -1f, O = 0f, P = 0f}
                    },
                    new Quad { Vert0 = new Vertex { X = 0.375f, Y = 0.375f, Z = 0.625f, U = 0.375f, V = 0.250f, N = 0f, O = 0f, P = 1f},
                               Vert1 = new Vertex { X = 0.375f, Y = 0.625f, Z = 0.625f, U = 0.625f, V = 0.250f, N = 0f, O = 0f, P = 1f},
                               Vert2 = new Vertex { X = 0.625f, Y = 0.625f, Z = 0.625f, U = 0.625f, V = 0.500f, N = 0f, O = 0f, P = 1f},
                               Vert3 = new Vertex { X = 0.625f, Y = 0.375f, Z = 0.625f, U = 0.375f, V = 0.500f, N = 0f, O = 0f, P = 1f}
                    },
                    new Quad { Vert0 = new Vertex { X = 0.625f, Y = 0.375f, Z = 0.625f, U = 0.375f, V = 0.500f, N = 1f, O = 0f, P = 0f},
                               Vert1 = new Vertex { X = 0.625f, Y = 0.625f, Z = 0.625f, U = 0.625f, V = 0.500f, N = 1f, O = 0f, P = 0f},
                               Vert2 = new Vertex { X = 0.625f, Y = 0.625f, Z = 0.375f, U = 0.625f, V = 0.750f, N = 1f, O = 0f, P = 0f},
                               Vert3 = new Vertex { X = 0.625f, Y = 0.375f, Z = 0.375f, U = 0.375f, V = 0.750f, N = 1f, O = 0f, P = 0f}
                    },
                    new Quad { Vert0 = new Vertex { X = 0.625f, Y = 0.375f, Z = 0.375f, U = 0.375f, V = 0.750f, N = 0f, O = 0f, P = -1f},
                               Vert1 = new Vertex { X = 0.625f, Y = 0.625f, Z = 0.375f, U = 0.625f, V = 0.750f, N = 0f, O = 0f, P = -1f},
                               Vert2 = new Vertex { X = 0.375f, Y = 0.625f, Z = 0.375f, U = 0.625f, V = 1.000f, N = 0f, O = 0f, P = -1f},
                               Vert3 = new Vertex { X = 0.375f, Y = 0.375f, Z = 0.375f, U = 0.375f, V = 1.000f, N = 0f, O = 0f, P = -1f}
                    },
                    new Quad { Vert0 = new Vertex { X = 0.375f, Y = 0.375f, Z = 0.625f, U = 0.125f, V = 0.500f, N = 0f, O = -1f, P = 0f},
                               Vert1 = new Vertex { X = 0.625f, Y = 0.375f, Z = 0.625f, U = 0.375f, V = 0.500f, N = 0f, O = -1f, P = 0f},
                               Vert2 = new Vertex { X = 0.625f, Y = 0.375f, Z = 0.375f, U = 0.375f, V = 0.750f, N = 0f, O = -1f, P = 0f},
                               Vert3 = new Vertex { X = 0.375f, Y = 0.375f, Z = 0.375f, U = 0.125f, V = 0.750f, N = 0f, O = -1f, P = 0f}
                    },
                    new Quad { Vert0 = new Vertex { X = 0.625f, Y = 0.625f, Z = 0.625f, U = 0.625f, V = 0.500f, N = 0f, O = 1f, P = 0f},
                               Vert1 = new Vertex { X = 0.375f, Y = 0.625f, Z = 0.625f, U = 0.875f, V = 0.500f, N = 0f, O = 1f, P = 0f},
                               Vert2 = new Vertex { X = 0.375f, Y = 0.625f, Z = 0.375f, U = 0.875f, V = 0.750f, N = 0f, O = 1f, P = 0f},
                               Vert3 = new Vertex { X = 0.625f, Y = 0.625f, Z = 0.375f, U = 0.625f, V = 0.750f, N = 0f, O = 1f, P = 0f}
                    }
                }
            };
        }

        public static (float[] vertices, int[] textureIndices, uint[] indices) CombineData(out uint vertexCount, params BlockModel[] models)
        {
            vertexCount = 0;

            bool locked = models.Aggregate(true, (current, model) => current && model.isLocked);

            if (locked)
            {
                int vertexArrayLength = models.Sum(model => model.lockedVertices.Length);
                int textureIndicesArrayLength = models.Sum(model => model.lockedTextureIndices.Length);
                int indicesArrayLength = models.Sum(model => model.lockedIndices.Length);

                float[] vertices = new float[vertexArrayLength];
                int[] textureIndices = new int[textureIndicesArrayLength];
                uint[] indices = new uint[indicesArrayLength];

                int copiedVertices = 0;
                int copiedTextureIndices = 0;
                int copiedIndices = 0;

                foreach (var model in models)
                {
                    Array.Copy(model.lockedVertices, 0, vertices, copiedVertices, model.lockedVertices.Length);
                    Array.Copy(model.lockedTextureIndices, 0, textureIndices, copiedTextureIndices, model.lockedTextureIndices.Length);
                    Array.Copy(model.lockedIndices, 0, indices, copiedIndices, model.lockedIndices.Length);

                    for (int i = copiedIndices; i < copiedIndices + model.lockedIndices.Length; i++)
                    {
                        indices[i] += vertexCount;
                    }

                    copiedVertices += model.lockedVertices.Length;
                    copiedTextureIndices += model.lockedTextureIndices.Length;
                    copiedIndices += model.lockedIndices.Length;

                    vertexCount += (uint)model.VertexCount;
                }

                return (vertices, textureIndices, indices);
            }
            else
            {
                List<float> vertices = new List<float>();
                List<int> textureIndices = new List<int>();
                List<uint> indices = new List<uint>();

                foreach (BlockModel model in models)
                {
                    model.ToData(out float[] modelVertices, out int[] modelTextureIndices, out uint[] modelIndices);

                    int firstNewIndex = indices.Count;

                    vertices.AddRange(modelVertices);
                    textureIndices.AddRange(modelTextureIndices);
                    indices.AddRange(modelIndices);

                    for (int i = firstNewIndex; i < indices.Count; i++)
                    {
                        indices[i] += vertexCount;
                    }

                    vertexCount += (uint)model.VertexCount;
                }

                return (vertices.ToArray(), textureIndices.ToArray(), indices.ToArray());
            }
        }

        #endregion STATIC METHODS
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types

    public struct Quad
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public int TextureId { get; set; }

        public Vertex Vert0 { get; set; }
        public Vertex Vert1 { get; set; }
        public Vertex Vert2 { get; set; }
        public Vertex Vert3 { get; set; }

        public Vector3 Center => (Vert0.Position + Vert1.Position + Vert2.Position + Vert3.Position) / 4;

        /// <summary>
        /// Apply a matrix only affecting the xyz values.
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

        public Quad ApplyMatrix(Matrix4 xyz, Matrix4 nop)
        {
            Vert0 = Vert0.ApplyMatrix(xyz, nop);
            Vert1 = Vert1.ApplyMatrix(xyz, nop);
            Vert2 = Vert2.ApplyMatrix(xyz, nop);
            Vert3 = Vert3.ApplyMatrix(xyz, nop);

            return this;
        }

        public Quad ApplyRotationMatrixY(Matrix4 xyz, Matrix4 nop, int rotations)
        {
            // Rotate positions and normals.
            Vert0 = Vert0.ApplyMatrix(xyz, nop);
            Vert1 = Vert1.ApplyMatrix(xyz, nop);
            Vert2 = Vert2.ApplyMatrix(xyz, nop);
            Vert3 = Vert3.ApplyMatrix(xyz, nop);

            // Rotate UVs for top and bottom sides.
            if (new Vector3(Vert0.N, Vert0.O, Vert0.P).Absolute().Rounded(2) == Vector3.UnitY)
            {
                for (int r = 0; r < rotations; r++)
                {
                    Vert0 = Vert0.RotateUV();
                    Vert1 = Vert1.RotateUV();
                    Vert2 = Vert2.RotateUV();
                    Vert3 = Vert3.RotateUV();
                }
            }

            return this;
        }

        public Quad RotateTextureCoordinates(Vector3 axis, int rotations)
        {
            if (new Vector3(Vert0.N, Vert0.O, Vert0.P).Absolute().Rounded(2) != axis) return this;

            for (var r = 0; r < rotations; r++)
            {
                Vert0 = Vert0.RotateUV();
                Vert1 = Vert1.RotateUV();
                Vert2 = Vert2.RotateUV();
                Vert3 = Vert3.RotateUV();
            }

            return this;
        }
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types

    public struct Vertex
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float U { get; set; }
        public float V { get; set; }

        public float N { get; set; }
        public float O { get; set; }
        public float P { get; set; }

        public Vector3 Position => new Vector3(X, Y, Z);

        public Vertex ApplyTranslationMatrix(Matrix4 xyz)
        {
            Vector4 position = new Vector4(X, Y, Z, 1f) * xyz;

            X = position.X;
            Y = position.Y;
            Z = position.Z;

            return this;
        }

        public Vertex ApplyMatrix(Matrix4 xyz, Matrix4 nop)
        {
            Vector4 position = new Vector4(X, Y, Z, 1f) * xyz;
            Vector4 normal = new Vector4(N, O, P, 1f) * nop;

            X = position.X;
            Y = position.Y;
            Z = position.Z;

            N = normal.X;
            O = normal.Y;
            P = normal.Z;

            return this;
        }

        public Vertex RotateUV()
        {
            Vertex old = this;

            U = old.V;
            V = Math.Abs(old.U - 1f);

            return this;
        }
    }

    public static class BlockModelExtensions
    {
        public static void Lock(this (BlockModel front, BlockModel back, BlockModel left, BlockModel right, BlockModel bottom, BlockModel top) group)
        {
            group.front.Lock();
            group.back.Lock();
            group.left.Lock();
            group.right.Lock();
            group.bottom.Lock();
            group.top.Lock();
        }

        public static void Lock(this (BlockModel north, BlockModel east, BlockModel south, BlockModel west) group)
        {
            group.north.Lock();
            group.east.Lock();
            group.south.Lock();
            group.west.Lock();
        }
    }
}