// <copyright file="ClassDocumentation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace VoxelGame.Manual.Utility
{
    /// <summary>
    ///     The documentation for a class.
    /// </summary>
    public class Documentation
    {
        private readonly Dictionary<string, string> documentation;

        /// <summary>
        ///     Create a documentation file for an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to document.</param>
        public Documentation(Assembly assembly)
        {
            documentation = ReadDocumentation(assembly);
        }

        private static string GetDocumentationFile(Assembly assembly)
        {
            string location = assembly.Location;
            string directory = Path.GetDirectoryName(location)!;

            string name = assembly.GetName().Name!;
            string path = Path.Combine(directory, name + ".xml");

            return path;
        }

        private static Dictionary<string, string> ReadDocumentation(Assembly assembly)
        {
            Dictionary<string, string> loadedDocumentation = new();

            XmlDocument doc = new();
            doc.Load(GetDocumentationFile(assembly));

            if (doc.DocumentElement == null) return loadedDocumentation;

            IEnumerable<XmlElement> members =
                doc.DocumentElement["members"]?.ChildNodes.OfType<XmlElement>() ?? Enumerable.Empty<XmlElement>();

            foreach (XmlElement member in members)
            {
                if (member.Name != "member") continue;

                string name = member.GetAttribute("name");
                string? value = member["summary"]?.InnerText;

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
        public string GetFieldSummary(MemberInfo field)
        {
            return documentation.TryGetValue(
                $"F:{field.DeclaringType?.FullName ?? ""}.{field.Name}",
                out string? summary)
                ? summary
                : "";
        }
    }
}
