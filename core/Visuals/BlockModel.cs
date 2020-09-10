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
using System.Text.Json;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This class is meant for data storage.")]
    public class BlockModel
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<BlockModel>();

        private static readonly string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Models");

        public string[] TextureNames { get; set; } = Array.Empty<string>();
        public Quad[] Quads { get; set; } = Array.Empty<Quad>();

        public int VertexCount { get => Quads.Length * 4; }

        /// <summary>
        /// Splits the BlockModel into two parts, using a given plane to sort all faces.
        /// </summary>
        /// <param name="position">Position of the plane.</param>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="a">The first model.</param>
        /// <param name="b">The second model.</param>
        public void PlaneSplit(Vector3 position, Vector3 normal, out BlockModel a, out BlockModel b)
        {
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
            Matrix4 xyz = Matrix4.CreateTranslation(movement);

            for (int i = 0; i < Quads.Length; i++)
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
            if (rotations == 0)
            {
                return;
            }

            float angle = rotations * MathHelper.PiOver2 * -1f;

            Matrix4 xyz = Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f) * Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
            Matrix4 nop = Matrix4.CreateRotationY(angle);

            rotations = rotateTopAndBottomTexture ? 0 : rotations;

            for (int i = 0; i < Quads.Length; i++)
            {
                Quads[i] = Quads[i].ApplyRotationMatrixY(xyz, nop, rotations);
            }
        }

        public void ToData(out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            int[] texIndexLookup = new int[TextureNames.Length];

            for (int i = 0; i < TextureNames.Length; i++)
            {
                texIndexLookup[i] = Game.BlockTextures.GetTextureIndex(TextureNames[i]);
            }

            vertices = new float[Quads.Length * 32];
            textureIndices = new int[Quads.Length * 4];

            for (int q = 0; q < Quads.Length; q++)
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

        public void Save(string name)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true };

            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(Path.Combine(path, name + ".json"), json);
        }

        public static BlockModel Load(string name)
        {
            try
            {
                string json = File.ReadAllText(Path.Combine(path, name + ".json"));
                BlockModel model = JsonSerializer.Deserialize<BlockModel>(json) ?? new BlockModel();

                logger.LogDebug("Loaded BlockModel: {name}", name);

                return model;
            }
            catch (Exception e) when (e is IOException || e is FileNotFoundException || e is JsonException)
            {
                logger.LogWarning(LoggingEvents.MissingRessource, e, "Could not load the model '{name}' because an exception occurred, a fallback will be used instead.", name);

                return CreateFallback();
            }
        }

        public static float[][] CubeVertices()
        {
            return new float[][]
            {
                new float[] // Front face
                {
                    0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f,
                    0f, 1f, 1f, 0f, 1f, 0f, 0f, 1f,
                    1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f,
                    1f, 0f, 1f, 1f, 0f, 0f, 0f, 1f
                },
                new float[] // Back face
                {
                    1f, 0f, 0f, 0f, 0f, 0f, 0f, -1f,
                    1f, 1f, 0f, 0f, 1f, 0f, 0f, -1f,
                    0f, 1f, 0f, 1f, 1f, 0f, 0f, -1f,
                    0f, 0f, 0f, 1f, 0f, 0f, 0f, -1f
                },
                new float[] // Left face
                {
                    0f, 0f, 0f, 0f, 0f, -1f, 0f, 0f,
                    0f, 1f, 0f, 0f, 1f, -1f, 0f, 0f,
                    0f, 1f, 1f, 1f, 1f, -1f, 0f, 0f,
                    0f, 0f, 1f, 1f, 0f, -1f, 0f, 0f
                },
                new float[] // Right face
                {
                    1f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,
                    1f, 1f, 1f, 0f, 1f, 1f, 0f, 0f,
                    1f, 1f, 0f, 1f, 1f, 1f, 0f, 0f,
                    1f, 0f, 0f, 1f, 0f, 1f, 0f, 0f
                },
                new float[] // Bottom face
                {
                    0f, 0f, 0f, 0f, 0f, 0f, -1f, 0f,
                    0f, 0f, 1f, 0f, 1f, 0f, -1f, 0f,
                    1f, 0f, 1f, 1f, 1f, 0f, -1f, 0f,
                    1f, 0f, 0f, 1f, 0f, 0f, -1f, 0f
                },
                new float[] // Top face
                {
                    0f, 1f, 1f, 0f, 0f, 0f, 1f, 0f,
                    0f, 1f, 0f, 0f, 1f, 0f, 1f, 0f,
                    1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f,
                    1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f
                }
            };
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

        public Quad ApplyTranslationMatrix(Matrix4 xyz)
        {
            Vert0 = Vert0.ApplyTranslationMatrix(xyz);
            Vert1 = Vert1.ApplyTranslationMatrix(xyz);
            Vert2 = Vert2.ApplyTranslationMatrix(xyz);
            Vert3 = Vert3.ApplyTranslationMatrix(xyz);

            return this;
        }

        public Quad ApplyRotationMatrixY(Matrix4 xyz, Matrix4 nop, int rotations)
        {
            // Rotate positions and normals.
            Vert0 = Vert0.ApplyRotationMatrix(xyz, nop);
            Vert1 = Vert1.ApplyRotationMatrix(xyz, nop);
            Vert2 = Vert2.ApplyRotationMatrix(xyz, nop);
            Vert3 = Vert3.ApplyRotationMatrix(xyz, nop);

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

        public Vertex ApplyRotationMatrix(Matrix4 xyz, Matrix4 nop)
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
}