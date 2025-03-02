// <copyright file="Serialize.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Memory;

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
    /// Save an object to a JSON file.
    /// </summary>
    /// <param name="obj">The object to save.</param>
    /// <param name="file">The file to save to.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result> SaveJsonAsync<T>(T obj, FileInfo file, CancellationToken token = default)
    {
        try
        {
            await using Stream stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);

            await JsonSerializer.SerializeAsync(stream, obj, options, token).InAnyContext();

            return Result.Ok();
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            return Result.Error(e);
        }
    }

    /// <summary>
    ///     Load an object from a JSON file.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result<T>> LoadJsonAsync<T>(FileInfo file, CancellationToken token = default)
    {
        try
        {
            await using Stream stream = file.OpenRead();

            T obj = await JsonSerializer.DeserializeAsync<T>(stream, options, token).InAnyContext() ?? throw new JsonException("Deserialized object is null.");

            return Result.Ok(obj);
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            return Result.Error<T>(e);
        }
    }

    /// <summary>
    ///     Save an object to a binary file asynchronously.
    /// </summary>
    /// <param name="entity">The object to save.</param>
    /// <param name="file">The file to save to.</param>
    /// <param name="signature">The signature of the file format defined by the entity.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result> SaveBinaryAsync<T>(T entity, FileInfo file, String signature, CancellationToken token = default) where T : IEntity
    {
        try
        {
            using MemoryStream memoryStream = Streams.Shared.GetPooledMemoryStream();

            await using (DeflateStream compressionStream = new(memoryStream, CompressionMode.Compress, leaveOpen: true))
            await using (BufferedStream bufferedStream = new(compressionStream))
            using (BinarySerializer serializer = new(bufferedStream, signature, file))
            {
                serializer.SerializeEntity(entity);
            }


            memoryStream.Position = 0;

            await using Stream fileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            await memoryStream.CopyToAsync(fileStream, token).InAnyContext();

            return Result.Ok();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Result.Error(e);
        }
    }

    /// <summary>
    ///     Load an object from a binary file asynchronously.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="entity">The object to load into, will be modified by the loading operation.</param>
    /// <param name="signature">The signature of the file format defined by the entity.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static async Task<Result> LoadBinaryAsync<T>(FileInfo file, T entity, String signature, CancellationToken token = default) where T : IEntity
    {
        try
        {
            using MemoryStream memoryStream = Streams.Shared.GetPooledMemoryStream();

            await using (Stream fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await fileStream.CopyToAsync(memoryStream, token).InAnyContext();
            }


            memoryStream.Position = 0;

            await using DeflateStream decompressionStream = new(memoryStream, CompressionMode.Decompress);
            await using BufferedStream bufferedStream = new(decompressionStream);
            using BinaryDeserializer deserializer = new(bufferedStream, signature, file);

            deserializer.SerializeEntity(entity);

            return Result.Ok();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return Result.Error(e);
        }
    }

    /// <summary>
    ///     Load an object from a binary file asynchronously.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="signature">The signature of the file format defined by the entity.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The result of the operation.</returns>
    public static async Task<Result<T>> LoadBinaryAsync<T>(FileInfo file, String signature, CancellationToken token = default) where T : IEntity, new()
    {
        T entity = new();

        return (await LoadBinaryAsync(file, entity, signature, token).InAnyContext()).Switch(
            () => Result.Ok(entity),
            Result.Error<T>
        );
    }
}
