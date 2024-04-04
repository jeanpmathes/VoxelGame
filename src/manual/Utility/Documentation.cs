// <copyright file="ClassDocumentation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Manual.Utility;

/// <summary>
///     The documentation for a class.
/// </summary>
public class Documentation
{
    private readonly Dictionary<String, String> documentation;

    /// <summary>
    ///     Create a documentation file for an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to document.</param>
    public Documentation(Assembly assembly)
    {
        documentation = ReadDocumentation(assembly);
    }

    private static FileInfo GetDocumentationFile(Assembly assembly)
    {
        FileInfo location = new(assembly.Location);
        DirectoryInfo directory = location.Directory!;

        String name = assembly.GetName().Name!;
        FileInfo path = directory.GetFile(name + ".xml");

        return path;
    }

    private static Dictionary<String, String> ReadDocumentation(Assembly assembly)
    {
        Dictionary<String, String> loadedDocumentation = new();

        XmlDocument doc = new();
        doc.Load(GetDocumentationFile(assembly).OpenText());

        if (doc.DocumentElement == null) return loadedDocumentation;

        IEnumerable<XmlElement> members =
            doc.DocumentElement["members"]?.ChildNodes.OfType<XmlElement>() ?? Enumerable.Empty<XmlElement>();

        foreach (XmlElement member in members)
        {
            if (member.Name != "member") continue;

            String name = member.GetAttribute("name");
            String? value = member["summary"]?.InnerText;

            if (value == null) continue;

            loadedDocumentation.Add(name, value.Trim());
        }

        return loadedDocumentation;
    }

    /// <summary>
    ///     Get the documentation for a field
    /// </summary>
    /// <param name="field">The field to get the summary for.</param>
    /// <returns>The summary.</returns>
    public String GetFieldSummary(MemberInfo field)
    {
        return documentation.TryGetValue(
            $"F:{field.DeclaringType?.FullName ?? ""}.{field.Name}",
            out String? summary)
            ? summary
            : "";
    }

    /// <summary>
    ///     Get the documentation for a property
    /// </summary>
    /// <param name="property">The property to get the summary for.</param>
    /// <returns>The summary.</returns>
    public String GetPropertySummary(MemberInfo property)
    {
        return documentation.TryGetValue(
            $"P:{property.DeclaringType?.FullName ?? ""}.{property.Name}",
            out String? summary)
            ? summary
            : "";
    }
}
