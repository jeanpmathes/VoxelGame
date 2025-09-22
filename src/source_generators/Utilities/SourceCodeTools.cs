// <copyright file="SourceCodeTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Text;

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
/// Tools to help with generating source code.
/// </summary>
public static class SourceCodeTools
{
    /// <summary>
    /// Generate a nested class structure based on the provided containing type information.
    /// The content will be placed inside the innermost class.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="containingType">The containing type information defining the nesting structure.</param>
    /// <param name="content">The content to place inside the innermost class.</param>
    /// <returns>The modified string builder.</returns>
    public static StringBuilder AppendNestedClass(this StringBuilder sb, ContainingType? containingType, StringBuilder content)
    // todo: test and improve this, maybe use lambda for content that passes indentation level
    {
        var nestedLevel = 0;
        
        while (containingType is not null)
        {
            sb.AppendLine($$"""
                            partial {{containingType.Keyword}} {{containingType.Name}} {{containingType.Constraints}} 
                            {
                            """);
            
            nestedLevel++;
            containingType = containingType.Child;
        }

        sb.Append(content);
        
        for (var i = 0; i < nestedLevel; i++)
        {
            sb.AppendLine("    }");
        }
        
        return sb;
    }
}
