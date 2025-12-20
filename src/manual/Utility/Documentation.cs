// <copyright file="ClassDocumentation.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
            doc.DocumentElement["members"]?.ChildNodes.OfType<XmlElement>() ?? [];

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
        return documentation.GetValueOrDefault($"F:{field.DeclaringType?.FullName ?? ""}.{field.Name}", "");
    }

    /// <summary>
    ///     Get the documentation for a property
    /// </summary>
    /// <param name="property">The property to get the summary for.</param>
    /// <returns>The summary.</returns>
    public String GetPropertySummary(MemberInfo property)
    {
        return documentation.GetValueOrDefault($"P:{property.DeclaringType?.FullName ?? ""}.{property.Name}", "");
    }

    /// <summary>
    ///     Get the documentation for a type.
    /// </summary>
    /// <param name="type">The type to get the summary for.</param>
    /// <returns>The summary.</returns>
    public String GetTypeSummary(Type type)
    {
        return documentation.GetValueOrDefault($"T:{type.FullName}", "");
    }
}
