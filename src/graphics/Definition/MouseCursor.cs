// <copyright file="MouseCursor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Graphics.Definition;

/// <summary>
///     Mouse cursor types.
/// </summary>
#pragma warning disable CS1591
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum MouseCursor : Byte
{
    Arrow,
    IBeam,
    SizeNS,
    SizeWE,
    SizeNWSE,
    SizeNESW,
    SizeAll,
    No,
    Wait,
    Hand
}
