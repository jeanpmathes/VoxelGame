// <copyright file="VoxelGamePlatform.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using Gwen.Net;
using Gwen.Net.Platform;
using TextCopy;
using VoxelGame.Graphics.Definition;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.UI.Platform;

/// <summary>
///     Implementation of <see cref="IPlatform" /> for VoxelGame.
/// </summary>
public class VoxelGamePlatform : IPlatform
{
    // Clipboard related code from:
    // http://forums.getpaint.net/index.php?/topic/13712-trouble-accessing-the-clipboard/page__view__findpost__p__226140

    private const String LibrariesCategory = "Libraries";
    private readonly Action<MouseCursor> setCursor;
    private readonly Stopwatch watch;

    /// <summary>
    ///     Creates a new instance of <see cref="VoxelGamePlatform" />.
    /// </summary>
    /// <param name="setCursor">Action to set the mouse cursor.</param>
    public VoxelGamePlatform(Action<MouseCursor> setCursor)
    {
        this.setCursor = setCursor;
        watch = new Stopwatch();
    }

    /// <summary>
    ///     Gets text from clipboard.
    /// </summary>
    /// <returns>Clipboard text.</returns>
    public String GetClipboardText()
    {
        var ret = String.Empty;

        Thread staThread = new(
            () =>
            {
                try
                {
                    String? text = ClipboardService.GetText();

                    if (String.IsNullOrEmpty(text)) return;

                    ret = text;
                }
                catch (Exception)
                {
                    // Method should be safe to call.
                }
            });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        // At this point either you have clipboard data or an exception
        return ret;
    }

    /// <summary>
    ///     Sets the clipboard text.
    /// </summary>
    /// <param name="text">Text to set.</param>
    /// <returns>True if succeeded.</returns>
    public Boolean SetClipboardText(String text)
    {
        var ret = false;

        Thread staThread = new(
            () =>
            {
                try
                {
                    ClipboardService.SetText(text);
                    ret = true;
                }
                catch (Exception)
                {
                    // Method should be safe to call.
                }
            });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();

        // At this point either you have clipboard data or an exception.
        return ret;
    }

    /// <summary>
    ///     Gets elapsed time since this class was initialized.
    /// </summary>
    /// <returns>Time interval in seconds.</returns>
    public Double GetTimeInSeconds()
    {
        return watch.Elapsed.TotalSeconds;
    }

    /// <summary>
    ///     Changes the mouse cursor.
    /// </summary>
    /// <param name="cursor">Cursor type.</param>
    public void SetCursor(Cursor cursor)
    {
        MouseCursor translatedCursor = cursor switch
        {
            Cursor.Normal => MouseCursor.Arrow,
            Cursor.Beam => MouseCursor.IBeam,
            Cursor.SizeNS => MouseCursor.SizeNS,
            Cursor.SizeWE => MouseCursor.SizeWE,
            Cursor.SizeNWSE => MouseCursor.SizeNWSE,
            Cursor.SizeNESW => MouseCursor.SizeNESW,
            Cursor.SizeAll => MouseCursor.SizeAll,
            Cursor.No => MouseCursor.No,
            Cursor.Wait => MouseCursor.Wait,
            Cursor.Finger => MouseCursor.Hand,
            _ => throw Exceptions.UnsupportedEnumValue(cursor)
        };

        setCursor.Invoke(translatedCursor);
    }

    /// <summary>
    ///     Get special folders of the system.
    /// </summary>
    /// <returns>List of folders.</returns>
    public IEnumerable<ISpecialFolder> GetSpecialFolders()
    {
        List<SpecialFolder> folders = new();
        DriveInfo[]? drives = null;

        try
        {
            folders.Add(
                new SpecialFolder(
                    "Documents",
                    LibrariesCategory,
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));

            folders.Add(
                new SpecialFolder(
                    "Music",
                    LibrariesCategory,
                    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));

            folders.Add(
                new SpecialFolder(
                    "Pictures",
                    LibrariesCategory,
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));

            folders.Add(
                new SpecialFolder(
                    "Videos",
                    LibrariesCategory,
                    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)));

            drives = DriveInfo.GetDrives();
        }
        catch (Exception)
        {
            // Method should be safe to call.
        }

        if (drives == null) return folders;

        foreach (DriveInfo driveInfo in drives)
        {
            try
            {
                if (!driveInfo.IsReady) continue;

                folders.Add(String.IsNullOrWhiteSpace(driveInfo.VolumeLabel)
                    ? new SpecialFolder(driveInfo.Name, "Computer", driveInfo.Name)
                    : new SpecialFolder(
                        $"{driveInfo.VolumeLabel} ({driveInfo.Name})",
                        "Computer",
                        driveInfo.Name));
            }
            catch (Exception)
            {
                // Method should be safe to call.
            }
        }

        return folders;
    }

    /// <summary>
    ///     Get file name from path.
    /// </summary>
    public String GetFileName(String path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    ///     Get directory name from path.
    /// </summary>
    public String GetDirectoryName(String path)
    {
        return Path.GetDirectoryName(path)!;
    }

    /// <summary>
    ///     Check if file exists.
    /// </summary>
    public Boolean FileExists(String path)
    {
        return File.Exists(path);
    }

    /// <summary>
    ///     Check if directory exists.
    /// </summary>
    public Boolean DirectoryExists(String path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    ///     Create directory.
    /// </summary>
    public void CreateDirectory(String path)
    {
        Directory.CreateDirectory(path);
    }

    /// <summary>
    ///     Combine paths.
    /// </summary>
    public String Combine(String path1, String path2)
    {
        return Path.Combine(path1, path2);
    }

    /// <summary>
    ///     Combine paths.
    /// </summary>
    public String Combine(String path1, String path2, String path3)
    {
        return Path.Combine(path1, path2, path3);
    }

    /// <summary>
    ///     Combine paths.
    /// </summary>
    public String Combine(String path1, String path2, String path3, String path4)
    {
        return Path.Combine(path1, path2, path3, path4);
    }

    /// <summary>
    ///     Get current directory.
    /// </summary>
    public String CurrentDirectory => Environment.CurrentDirectory;

    /// <summary>
    ///     Get all directories at a given path.
    /// </summary>
    public IEnumerable<IFileSystemDirectoryInfo> GetDirectories(String path)
    {
        DirectoryInfo di = new(path);

        try
        {
            return di.GetDirectories().Select(
                d => new FileSystemDirectoryInfo(d.FullName, d.LastWriteTime) as IFileSystemDirectoryInfo);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or SecurityException or UnauthorizedAccessException)
        {
            return Enumerable.Empty<IFileSystemDirectoryInfo>();
        }
    }

    /// <summary>
    ///     Get all files at a given path.
    /// </summary>
    public IEnumerable<IFileSystemFileInfo> GetFiles(String path, String filter)
    {
        DirectoryInfo di = new(path);

        try
        {
            return di.GetFiles(filter).Select(
                f => new FileSystemFileInfo(f.FullName, f.LastWriteTime, f.Length) as IFileSystemFileInfo);

        }
        catch (Exception e) when (e is DirectoryNotFoundException or SecurityException or UnauthorizedAccessException)
        {
            return Enumerable.Empty<IFileSystemFileInfo>();
        }
    }

    /// <summary>
    ///     Get a file stream to a given path.
    /// </summary>
    public Stream GetFileStream(String path, Boolean isWritable)
    {
        return new FileStream(
            path,
            isWritable ? FileMode.Create : FileMode.Open,
            isWritable ? FileAccess.Write : FileAccess.Read);
    }

    private sealed class SpecialFolder : ISpecialFolder
    {
        public SpecialFolder(String name, String category, String path)
        {
            Name = name;
            Category = category;
            Path = path;
        }

        public String Name { get; }
        public String Category { get; }
        public String Path { get; }
    }

    /// <summary>
    ///     Implementation of <see cref="IFileSystemItemInfo" />.
    /// </summary>
    private class FileSystemItemInfo : IFileSystemItemInfo
    {
        /// <summary>
        ///     Create a new instance of <see cref="FileSystemItemInfo" />.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <param name="lastWriteTime">The last write time of the file or directory.</param>
        protected FileSystemItemInfo(String path, DateTime lastWriteTime)
        {
            Name = Path.GetFileName(path);
            FullName = path;

            FormattedLastWriteTime = $"{lastWriteTime.ToShortDateString()} {lastWriteTime.ToLongTimeString()}";
        }

        /// <summary>
        ///     Get the name of the file or directory.
        /// </summary>
        public String Name { get; }

        /// <summary>
        ///     Get the full path of the file or directory.
        /// </summary>
        public String FullName { get; }

        /// <summary>
        ///     Get the formatted last write time of the file or directory.
        /// </summary>
        public String FormattedLastWriteTime { get; }
    }

    private sealed class FileSystemDirectoryInfo : FileSystemItemInfo, IFileSystemDirectoryInfo
    {
        public FileSystemDirectoryInfo(String path, DateTime lastWriteTime)
            : base(path, lastWriteTime) {}
    }

    private sealed class FileSystemFileInfo : FileSystemItemInfo, IFileSystemFileInfo
    {
        public FileSystemFileInfo(String path, DateTime lastWriteTime, Int64 length)
            : base(path, lastWriteTime)
        {
            FormattedFileLength = FormatFileLength(length);
        }

        public String FormattedFileLength { get; }

        private static String FormatFileLength(Int64 length)
        {
            if (length > 1024 * 1024 * 1024) return $"{(Double) length / (1024 * 1024 * 1024):0.0} GB";

            if (length > 1024 * 1024) return $"{(Double) length / (1024 * 1024):0.0} MB";

            if (length > 1024) return $"{(Double) length / 1024:0.0} kB";

            return $"{length} B";
        }
    }
}
