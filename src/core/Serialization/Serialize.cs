// <copyright file="Serialize.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Utility for easy serialization and deserialization of objects.
/// </summary>
public static class Serialize
{
    private static readonly JsonSerializerOptions options = new()
    {
        IgnoreReadOnlyProperties = true,
        WriteIndented = true
    };

    /// <summary>
    ///     Save an object to a JSON file.
    /// </summary>
    /// <param name="obj">The object to save.</param>
    /// <param name="file">The file to save to.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? SaveJSON<T>(T obj, FileInfo file)
    {
        try
        {
            String json = JsonSerializer.Serialize(obj, options);
            file.WriteAllText(json);

            return null;
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            return e;
        }
    }

    /// <summary>
    ///     Load an object from a JSON file.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="obj">Will be set to the loaded object or a fallback object if loading failed.</param>
    /// <param name="fallback">Function to create a fallback object if loading failed.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? LoadJSON<T>(FileInfo file, out T obj, Func<T> fallback)
    {
        try
        {
            String json = file.ReadAllText();

            obj = JsonSerializer.Deserialize<T>(json) ?? fallback();

            return null;
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            obj = fallback();

            return e;
        }
    }

    /// <summary>
    ///     Load an object from a JSON file.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="obj">Will be set to the loaded object or a new object if loading failed.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? LoadJSON<T>(FileInfo file, out T obj) where T : new()
    {
        return LoadJSON(file, out obj, () => new T());
    }

    /// <summary>
    ///     Save an object to a binary file.
    /// </summary>
    /// <param name="entity">The object to save.</param>
    /// <param name="file">The file to save to.</param>
    /// <param name="signature">The signature of the file format defined by the entity.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? SaveBinary<T>(T entity, FileInfo file, String signature = "") where T : IEntity
    {
        try
        {
            using Stream fileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            using DeflateStream compressionStream = new(fileStream, CompressionMode.Compress);
            using BufferedStream bufferedStream = new(compressionStream);
            using BinarySerializer serializer = new(bufferedStream, signature, file);

            serializer.SerializeEntity(entity);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return e;
        }

        return null;
    }

    /// <summary>
    ///     Load an object from a binary file.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="entity">The object to load into.</param>
    /// <param name="signature">The signature of the file format defined by the entity.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? LoadBinary<T>(FileInfo file, T entity, String signature = "") where T : IEntity
    {
        try
        {
            using Stream fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            using DeflateStream decompressionStream = new(fileStream, CompressionMode.Decompress);
            using BufferedStream bufferedStream = new(decompressionStream);
            using BinaryDeserializer deserializer = new(bufferedStream, signature, file);

            deserializer.SerializeEntity(entity);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return e;
        }

        return null;
    }

    /// <summary>
    ///     Load an object from a binary file.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="entity">The loaded object or a new object if loading failed.</param>
    /// <param name="signature">The signature of the file format defined by the entity.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? LoadBinary<T>(FileInfo file, out T entity, String signature = "") where T : IEntity, new()
    {
        entity = new T();

        return LoadBinary(file, entity, signature);
    }
}
