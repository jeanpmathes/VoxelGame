// <copyright file="PropertyPrinter.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    public static string Print(Property property)
    {
        Builder builder = new();
        builder.Visit(property);

        return builder.String;
    }

    private sealed class Builder : Visitor
    {
        private readonly StringBuilder builder = new();

        private int indent;

        public string String => builder.ToString();

        private string Indent => new(c: ' ', indent);

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
    }
}
