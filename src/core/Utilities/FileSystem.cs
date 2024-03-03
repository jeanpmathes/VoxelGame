﻿// <copyright file="FileSystem.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Some utilities for filesystem operations.
/// </summary>
#pragma warning disable S3242 // The distinction between file and directory information has semantic relevance.
public static class FileSystem
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger(nameof(FileSystem));

    private static readonly ISet<string> reservedNames = new HashSet<string>
    {
        "CON",
        "PRN",
        "AUX",
        "NUL",
        "COM",
        "COM0",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT0",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9"
    };

    /// <summary>
    ///     Creates all subdirectories along a path, starting from a special folder.
    /// </summary>
    /// <param name="parent">The parent special folder.</param>
    /// <param name="subdirectories">A list of subdirectories.</param>
    /// <returns>The subdirectory.</returns>
    /// <exception cref="IOException">If the directory could not be created.</exception>
    public static DirectoryInfo CreateSubdirectory(Environment.SpecialFolder parent, params string[] subdirectories)
    {
        return Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(parent), Path.Combine(subdirectories)));
    }

    /// <summary>
    ///     Get the path of a file in a directory. This does not create the file.
    /// </summary>
    /// <param name="parent">The parent directory.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The file path.</returns>
    public static FileInfo GetFile(this DirectoryInfo parent, string fileName)
    {
        return new FileInfo(Path.Combine(parent.FullName, fileName));
    }

    /// <summary>
    ///     Get the path of a subdirectory in a directory. This does not create the subdirectory.
    /// </summary>
    /// <param name="parent">The parent directory.</param>
    /// <param name="directoryName">The subdirectory name.</param>
    /// <returns>The subdirectory path.</returns>
    public static DirectoryInfo GetDirectory(this DirectoryInfo parent, string directoryName)
    {
        return new DirectoryInfo(Path.Combine(parent.FullName, directoryName));
    }

    /// <summary>
    ///     Opens a file in a directory.
    /// </summary>
    /// <param name="parent">The directory.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="mode">The file mode.</param>
    /// <param name="access">The requested access.</param>
    /// <param name="share">The sharing mode.</param>
    /// <returns>The file stream.</returns>
    public static FileStream OpenFile(this DirectoryInfo parent, string fileName, FileMode mode, FileAccess access, FileShare share = FileShare.None)
    {
        return parent.GetFile(fileName).Open(mode, access, share);
    }

    /// <summary>
    ///     Read all text from a file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The file text.</returns>
    public static string ReadAllText(this FileInfo file)
    {
        return File.ReadAllText(file.FullName);
    }

    /// <summary>
    ///     Write all text to a file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="text">The text to write.</param>
    public static void WriteAllText(this FileInfo file, string text)
    {
        File.WriteAllText(file.FullName, text);
    }

    /// <summary>
    ///     Get the path of a resource folder in the application resources directory.
    /// </summary>
    /// <param name="path">The folder structure.</param>
    /// <returns>The directory path.</returns>
    public static DirectoryInfo GetResourceDirectory(params string[] path)
    {
        return new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Resources", Path.Combine(path)));
    }

    /// <summary>
    ///     Get the path to this resource file or directory relative to the resource directory.
    /// </summary>
    public static string GetResourceRelativePath(this FileSystemInfo resource)
    {
        return Path.GetRelativePath(GetResourceDirectory().FullName, resource.FullName);
    }

    private static bool IsNameReserved(string name)
    {
        return reservedNames.Contains(name);
    }

    /// <summary>
    ///     Get a unique directory name.
    /// </summary>
    /// <param name="parent">The parent directory.</param>
    /// <param name="name">The first directory name, that will be modified if necessary.</param>
    /// <returns>The unique directory.</returns>
    public static DirectoryInfo GetUniqueDirectory(DirectoryInfo parent, string name)
    {
        StringBuilder path = new(Path.Combine(parent.FullName, name));

        if (IsNameReserved(name)) path.Append(value: '_');

        if (!Directory.Exists(path.ToString())) return Directory.CreateDirectory(path.ToString());

        Regex pattern = new(Regex.Escape(name) + @"\s\((\d+)\)");

        int number = parent.EnumerateDirectories()
            .Select(directory => pattern.Match(directory.Name))
            .Where(match => match.Success)
            .Select(match =>
            {
                int.TryParse(match.Groups[groupnum: 1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result);

                return result;
            })
            .DefaultIfEmpty(defaultValue: 0)
            .Max();

        path.Append(value: ' ');
        path.Append(value: '(');
        path.Append(number + 1);
        path.Append(value: ')');

        return Directory.CreateDirectory(path.ToString());
    }

    /// <summary>
    ///     Get the file name without extension.
    /// </summary>
    public static string GetFileNameWithoutExtension(this FileInfo file)
    {
        return Path.GetFileNameWithoutExtension(file.FullName);
    }

    /// <summary>
    ///     Get the full path to a directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The full path.</returns>
    public static DirectoryInfo GetFullPath(string path)
    {
        return new DirectoryInfo(Path.GetFullPath(path));
    }

    /// <summary>
    ///     Creates a temporary directory.
    /// </summary>
    /// <returns>The temporary directory.</returns>
    public static DirectoryInfo CreateTemporaryDirectory()
    {
        string? directory;

        do
        {
            directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (IOException)
            {
                directory = null;
            }
        } while (directory == null);

        return new DirectoryInfo(directory);
    }

    /// <summary>
    ///     Save an object to a JSON file.
    /// </summary>
    /// <param name="obj">The object to save.</param>
    /// <param name="file">The file to save to.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>An exception if the operation failed, null otherwise.</returns>
    public static Exception? SaveJSON<T>(T obj, FileInfo file)
    {
        JsonSerializerOptions options = new()
        {
            IgnoreReadOnlyProperties = true,
            WriteIndented = true
        };

        try
        {
            string json = JsonSerializer.Serialize(obj, options);
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
            string json = file.ReadAllText();

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
    ///     Copy a directory to another directory.
    ///     Will copy all content, recursively.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="destination">The destination directory.</param>
    public static void CopyTo(this DirectoryInfo source, DirectoryInfo destination)
    {
        destination.Create();

        foreach (FileInfo file in source.EnumerateFiles()) file.CopyTo(destination.GetFile(file.Name).FullName, overwrite: true);

        foreach (DirectoryInfo directory in source.EnumerateDirectories()) directory.CopyTo(destination.GetDirectory(directory.Name));
    }

    /// <summary>
    ///     Get the size of a file or directory.
    /// </summary>
    /// <param name="info">The file or directory.</param>
    /// <returns>The size of the file or directory, or null if the size could not be determined.</returns>
    public static Memory? GetSize(this FileSystemInfo info)
    {
        try
        {
            return new Memory
            {
                Bytes = info switch
                {
                    FileInfo fileInfo => fileInfo.Length,
                    DirectoryInfo directoryInfo => directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length),
                    _ => 0
                }
            };
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not get the size of: {Path}", info.FullName);

            return null;
        }
    }
}
