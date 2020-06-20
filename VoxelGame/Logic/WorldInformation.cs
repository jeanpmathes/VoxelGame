// <copyright file="WorldInformation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.IO;
using System.Text.Json;

namespace VoxelGame.Logic
{
    public class WorldInformation
    {
        public string Name { get; set; } = "No Name";
        public int Seed { get; set; } = 2133;
        public DateTime Creation { get; set; } = DateTime.MinValue;
        public string Version { get; set; } = "missing";

        public void Save(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize<WorldInformation>(this, options);
            File.WriteAllText(path, json);
        }

        public static WorldInformation Load(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<WorldInformation>(json) ?? new WorldInformation();
        }
    }
}