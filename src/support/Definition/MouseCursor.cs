// <copyright file="MouseCursor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Mouse cursor types.
/// </summary>
#pragma warning disable CS1591
[SuppressMessage("ReSharper", "InconsistentNaming")]
#pragma warning disable S4022 // Storage is explicit as it is passed to native code.
public enum MouseCursor : byte
#pragma warning restore S4022 // Storage is explicit as it is passed to native code.
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
