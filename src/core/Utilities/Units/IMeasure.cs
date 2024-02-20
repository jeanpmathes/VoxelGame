// <copyright file="IUnit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     Base interface for all measures.
/// </summary>
public interface IMeasure
{
    /// <summary>
    ///     The unit of the measures.
    /// </summary>
    Unit Unit { get; }

    /// <summary>
    ///     The value of the measures.
    /// </summary>
    double Value { get; }

    /// <summary>
    ///     Get the string representation of the measures.
    /// </summary>
    public string Text
    {
        get
        {
            Prefix prefix = Prefix.FindBest(Value);

            return $"{Value / prefix.Factor:F2} {prefix.Symbol}{Unit.Symbol}";
        }
    }

    /// <summary>
    ///     Get the string representation of a measure.
    /// </summary>
    public static string ToString(IMeasure measure)
    {
        return measure.Text;
    }
}
