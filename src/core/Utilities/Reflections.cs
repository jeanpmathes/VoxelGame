// <copyright file="Reflections.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using System.Text;

namespace VoxelGame.Core.Utilities;

/// <summary>
/// Provides reflection utilities.
/// </summary>
public static class Reflections
{
    /// <summary>
    /// Get the long name of a type.
    /// Different from <see cref="Type.FullName"/> for generic types.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The full name of the type.</returns>
    public static String GetLongName<T>() where T : notnull
    {
        return GetLongName(typeof(T));
    }

    /// <summary>
    /// Get the long name of a type.
    /// Different from <see cref="Type.FullName"/> for generic types.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The full name of the type.</returns>
    public static String GetLongName(Type type)
    {
        StringBuilder builder = new();

        if (type.Namespace is {} ns)
            builder.Append(ns).Append(value: '.');

        if (type.IsGenericType)
        {
            String name = type.Name;
            Int32 index = name.IndexOf(value: '`');

            name = index != -1 ? name[..index] : name;

            builder
                .Append(name)
                .Append(value: '<')
                .AppendJoin(separator: ',', type.GetGenericArguments().Select(GetLongName))
                .Append(value: '>');
        }
        else
        {
            builder.Append(type.Name);

            if (type.IsArray)
                builder.Append("[]");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Get a decorated name for a type.
    /// </summary>
    /// <param name="prefix">A prefix for the name.</param>
    /// <param name="instance">The name of an instance, if applicable.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The decorated name.</returns>
    public static String GetDecoratedName<T>(String prefix, String? instance) where T : notnull
    {
        return GetDecoratedName(prefix, typeof(T), instance);
    }

    /// <summary>
    /// Get a decorated name for a type.
    /// </summary>
    /// <param name="prefix">A prefix for the name.</param>
    /// <param name="type">The type.</param>
    /// <param name="instance">The name of an instance, if applicable.</param>
    /// <returns>The decorated name.</returns>
    public static String GetDecoratedName(String prefix, Type type, String? instance)
    {
        return instance == null
            ? $"{prefix}::{GetLongName(type)}"
            : $"{prefix}::{GetLongName(type)}::{instance}";
    }
}
