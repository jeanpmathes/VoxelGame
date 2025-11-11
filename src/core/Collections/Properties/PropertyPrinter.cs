// <copyright file="PropertyPrinter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Globalization;
using System.Text;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     Prints the properties to a string.
/// </summary>
public static class PropertyPrinter
{
    /// <summary>
    ///     Print a given property to a string.
    /// </summary>
    /// <param name="property">The property to print.</param>
    /// <param name="cultureInfo">The culture info to use for formatting.</param>
    /// <returns>The string representation of the property.</returns>
    public static String Print(Property property, CultureInfo cultureInfo)
    {
        Builder builder = new(cultureInfo);
        builder.Visit(property);

        return builder.String;
    }

    private sealed class Builder(CultureInfo cultureInfo) : Visitor
    {
        private readonly StringBuilder builder = new();

        private Int32 indent;

        public String String => builder.ToString();

        private String Indent => new(c: ' ', indent);

        public override void Visit(Group group)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{group.Name}:");

            indent += 2;
            base.Visit(group);
            indent -= 2;
        }

        public override void Visit(Error error)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{error.Name}: {error.Message}");
        }

        public override void Visit(Message message)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{message.Name}: {message.Text}");
        }

        public override void Visit(Integer integer)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{integer.Name}: {integer.Value}");
        }

        public override void Visit(FileSystemPath path)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{path.Name}: {path.Path}");
        }

        public override void Visit(Measure measure)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{measure.Name}: {measure.Value}");
        }

        public override void Visit(Truth truth)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{truth.Name}: {(truth.Value ? "true" : "false")}");
        }

        public override void Visit(Color color)
        {
            builder.AppendLine(cultureInfo, $"{Indent}{color.Name}: {color.Value.R:P}, {color.Value.G:P}, {color.Value.B:P}, {color.Value.A:P}");
        }
    }
}
