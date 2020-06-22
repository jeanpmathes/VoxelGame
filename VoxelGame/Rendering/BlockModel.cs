// <copyright file="BlockModel.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.IO;
using System.Text.Json;

namespace VoxelGame.Rendering
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This class is meant for data storage.")]
    public class BlockModel
    {
        private static readonly string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Models");

        public string[] TextureNames { get; set; } = Array.Empty<string>();
        public Quad[] Quads { get; set; } = Array.Empty<Quad>();

        public void ToData(out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            int[] texIndexLookup = new int[TextureNames.Length];

            for (int i = 0; i < TextureNames.Length; i++)
            {
                try
                {
                    texIndexLookup[i] = Game.BlockTextureArray.GetTextureIndex(TextureNames[i]);
                }
                catch (ArgumentException)
                {
                    texIndexLookup[i] = 0;
                }
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
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

            string json = JsonSerializer.Serialize<BlockModel>(this, options);
            File.WriteAllText(Path.Combine(path, name + ".json"), json);
        }

        public static BlockModel Load(string name)
        {
            string json = File.ReadAllText(Path.Combine(path, name + ".json"));
            return JsonSerializer.Deserialize<BlockModel>(json) ?? new BlockModel();
        }
    }

    public struct Quad
    {
        public int TextureId { get; set; }

        public Vertex Vert0 { get; set; }
        public Vertex Vert1 { get; set; }
        public Vertex Vert2 { get; set; }
        public Vertex Vert3 { get; set; }
    }

    public struct Vertex
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float U { get; set; }
        public float V { get; set; }

        public float N { get; set; }
        public float O { get; set; }
        public float P { get; set; }
    }
}
