// <copyright file="Unit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A unit of measure.
/// </summary>
public record Unit(string Symbol)
{
                                                                                                                                                                                                                                                                                                        #pragma warning disable CS1591 // Understandable without documentation.
    public static Unit None { get; } = new("");
    public static Unit Meter { get; } = new("m");
    public static Unit Byte { get; } = new("B");
#pragma warning disable SA1600 // Understandable without documentation.
}
