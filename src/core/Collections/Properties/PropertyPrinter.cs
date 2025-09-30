// <copyright file="PropertyPrinter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    /// <returns>The string representation of the property.</returns>
    public static String Print(Property property)
    {
        Builder builder = new();
        builder.Visit(property);

        return builder.String;
    }

    private sealed class Builder : Visitor
    {
        private readonly StringBuilder builder = new();

        private Int32 indent;

        public String String => builder.ToString();

        private String Indent => new(c: ' ', indent);

        public override void Visit(Group group)
        {
            builder.AppendLine($"{Indent}{group.Name}:");

            indent += 2;
            base.Visit(group);
            indent -= 2;
        }

        public override void Visit(Error error)
        {
            builder.AppendLine($"{Indent}{error.Name}: {error.Message}");
        }

        public override void Visit(Message message)
        {
            builder.AppendLine($"{Indent}{message.Name}: {message.Text}");
        }

        public override void Visit(Integer integer)
        {
            builder.AppendLine($"{Indent}{integer.Name}: {integer.Value}");
        }

        public override void Visit(FileSystemPath path)
        {
            builder.AppendLine($"{Indent}{path.Name}: {path.Path}");
        }

        public override void Visit(Measure measure)
        {
            builder.AppendLine($"{Indent}{measure.Name}: {measure.Value}");
        }

        public override void Visit(Truth truth)
        {
            builder.AppendLine($"{Indent}{truth.Name}: {(truth.Value ? "true" : "false")}");
        }

        public override void Visit(Color color)
        {
            builder.AppendLine($"{Indent}{color.Name}: {color.Value.R:P}, {color.Value.G:P}, {color.Value.B:P}, {color.Value.A:P}");
        }
    }
}
