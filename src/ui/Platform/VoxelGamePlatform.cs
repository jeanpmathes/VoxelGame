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
using VoxelGame.Support.Definition;

namespace VoxelGame.UI.Platform
{
    /// <summary>
    ///     Implementation of <see cref="IPlatform" /> for VoxelGame.
    /// </summary>
    public class VoxelGamePlatform : IPlatform
    {
        // Clipboard related code from:
        // http://forums.getpaint.net/index.php?/topic/13712-trouble-accessing-the-clipboard/page__view__findpost__p__226140

        private const string LibrariesCategory = "Libraries";
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
        public string GetClipboardText()
        {
            var ret = string.Empty;

            Thread staThread = new(
                () =>
                {
                    try
                    {
                        string? text = ClipboardService.GetText();

                        if (string.IsNullOrEmpty(text))
                        {
                            return;
                        }

                        ret = text;
                    }
                    #pragma warning disable S2221 // Not clear what could be thrown here.
                    catch (Exception)
                    #pragma warning restore S2221
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
        public bool SetClipboardText(string text)
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
                    #pragma warning disable S2221 // Not clear what could be thrown here.
                    catch (Exception)
                    #pragma warning restore S2221
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
        public double GetTimeInSeconds()
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
                _ => throw new ArgumentOutOfRangeException(nameof(cursor), cursor, message: null)
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
            #pragma warning disable S2221 // Not clear what could be thrown here.
            catch (Exception)
            #pragma warning restore S2221
            {
                // Method should be safe to call.
            }

            if (drives == null) return folders;

            foreach (DriveInfo driveInfo in drives)
            {
                try
                {
                    if (!driveInfo.IsReady) continue;

                    folders.Add(string.IsNullOrWhiteSpace(driveInfo.VolumeLabel)
                        ? new SpecialFolder(driveInfo.Name, "Computer", driveInfo.Name)
                        : new SpecialFolder(
                            $"{driveInfo.VolumeLabel} ({driveInfo.Name})",
                            "Computer",
                            driveInfo.Name));
                }
                #pragma warning disable S2221 // Not clear what could be thrown here.
                catch (Exception)
                #pragma warning restore S2221
                {
                    // Method should be safe to call.
                }
            }

            return folders;
        }

        /// <summary>
        ///     Get file name from path.
        /// </summary>
        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        ///     Get directory name from path.
        /// </summary>
        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path)!;
        }

        /// <summary>
        ///     Check if file exists.
        /// </summary>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        ///     Check if directory exists.
        /// </summary>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        ///     Create directory.
        /// </summary>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        ///     Combine paths.
        /// </summary>
        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        ///     Combine paths.
        /// </summary>
        public string Combine(string path1, string path2, string path3)
        {
            return Path.Combine(path1, path2, path3);
        }

        /// <summary>
        ///     Combine paths.
        /// </summary>
        public string Combine(string path1, string path2, string path3, string path4)
        {
            return Path.Combine(path1, path2, path3, path4);
        }

        /// <summary>
        ///     Get current directory.
        /// </summary>
        public string CurrentDirectory => Environment.CurrentDirectory;

        /// <summary>
        ///     Get all directories at a given path.
        /// </summary>
        public IEnumerable<IFileSystemDirectoryInfo> GetDirectories(string path)
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
        public IEnumerable<IFileSystemFileInfo> GetFiles(string path, string filter)
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
        public Stream GetFileStream(string path, bool isWritable)
        {
            return new FileStream(
                path,
                isWritable ? FileMode.Create : FileMode.Open,
                isWritable ? FileAccess.Write : FileAccess.Read);
        }

        private sealed class SpecialFolder : ISpecialFolder
        {
            public SpecialFolder(string name, string category, string path)
            {
                Name = name;
                Category = category;
                Path = path;
            }

            public string Name { get; }
            public string Category { get; }
            public string Path { get; }
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
            protected FileSystemItemInfo(string path, DateTime lastWriteTime)
            {
                Name = Path.GetFileName(path);
                FullName = path;

                FormattedLastWriteTime = $"{lastWriteTime.ToShortDateString()} {lastWriteTime.ToLongTimeString()}";
            }

            /// <summary>
            ///     Get the name of the file or directory.
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            ///     Get the full path of the file or directory.
            /// </summary>
            public string FullName { get; internal set; }

            /// <summary>
            ///     Get the formatted last write time of the file or directory.
            /// </summary>
            public string FormattedLastWriteTime { get; internal set; }
        }

        private sealed class FileSystemDirectoryInfo : FileSystemItemInfo, IFileSystemDirectoryInfo
        {
            public FileSystemDirectoryInfo(string path, DateTime lastWriteTime)
                : base(path, lastWriteTime) {}
        }

        private sealed class FileSystemFileInfo : FileSystemItemInfo, IFileSystemFileInfo
        {
            public FileSystemFileInfo(string path, DateTime lastWriteTime, long length)
                : base(path, lastWriteTime)
            {
                FormattedFileLength = FormatFileLength(length);
            }

            public string FormattedFileLength { get; internal set; }

            private static string FormatFileLength(long length)
            {
                if (length > 1024 * 1024 * 1024)
                {
                    return $"{(double) length / (1024 * 1024 * 1024):0.0} GB";
                }

                if (length > 1024 * 1024)
                {
                    return $"{(double) length / (1024 * 1024):0.0} MB";
                }

                if (length > 1024)
                {
                    return $"{(double) length / 1024:0.0} kB";
                }

                return $"{length} B";
            }
        }
    }
}
