// <copyright file="WorldInformation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace VoxelGame.Logic
{
    public class WorldInformation
    {
        public string Name { get; set; }
        public DateTime Creation { get; set; }

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
            return JsonSerializer.Deserialize<WorldInformation>(json);
        }
    }
}
