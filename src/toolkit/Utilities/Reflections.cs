// <copyright file="Reflections.cs" company="VoxelGame">
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
using System.Linq;
using System.Reflection;
using System.Text;

namespace VoxelGame.Toolkit.Utilities;

/// <summary>
///     Provides reflection utilities.
/// </summary>
public static class Reflections
{
    /// <summary>
    ///     Get the long name of a type.
    ///     Different from <see cref="Type.FullName" /> for generic types.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The full name of the type.</returns>
    public static String GetLongName<T>() where T : notnull
    {
        return GetLongName(typeof(T));
    }

    /// <summary>
    ///     Get the long name of a type.
    ///     Different from <see cref="Type.FullName" /> for generic types.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The full name of the type.</returns>
    public static String GetLongName(Type type)
    {
        StringBuilder builder = new();

        if (type.Namespace is {} ns)
            builder.Append(ns).Append(value: '.');

        builder.Append(GetName(type));

        return builder.ToString();
    }

    /// <summary>
    ///     Get the long name of a type with an instance name.
    ///     Different from <see cref="Type.FullName" /> for generic types.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="instance">The instance name.</param>
    /// <returns>The long name with instance.</returns>
    public static String GetLongName(Type type, String instance)
    {
        return $"{GetLongName(type)}::{instance}";
    }

    /// <summary>
    ///     Get the name of a type, handling generics and arrays properly.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The name of the type.</returns>
    public static String GetName<T>() where T : notnull
    {
        return GetName(typeof(T));
    }

    /// <summary>
    ///     Get the name of a type, handling generics and arrays properly.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The name of the type.</returns>
    public static String GetName(Type type)
    {
        StringBuilder builder = new();

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
    ///     Get a decorated name for a type.
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
    ///     Get a decorated name for a type.
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

    /// <summary>
    ///     Get all properties of an object that have a certain type or can be assigned to it.
    /// </summary>
    /// <typeparam name="T">The type of the properties.</typeparam>
    /// <param name="target">The object to get the properties from.</param>
    /// <returns>The found properties.</returns>
    public static IEnumerable<PropertyInfo> GetPropertiesOfType<T>(Object target) where T : class
    {
        Type filterType = typeof(T);

        return target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(info => filterType.IsAssignableFrom(info.PropertyType));
    }

    /// <summary>
    ///     Get all overloads of a method with a certain name.
    /// </summary>
    /// <param name="type">The type to get the methods from.</param>
    /// <param name="name">The name of the method.</param>
    /// <returns>All overloads of the method.</returns>
    public static IEnumerable<MethodInfo> GetMethodOverloads(Type type, String name)
    {
        return type.GetMethods()
            .Where(m => m.Name.Equals(name, StringComparison.Ordinal) && !m.IsStatic);
    }


    /// <summary>
    ///     Get instances of all subclasses of a type.
    ///     Only concrete classes with a public parameterless constructor are considered.
    /// </summary>
    /// <typeparam name="T">The type to get the subclasses of.</typeparam>
    /// <returns>All instances of the subclasses.</returns>
    public static IEnumerable<T> GetSubclassInstances<T>()
    {
        List<T> instances = [];

        foreach (Type type in GetSubclasses<T>())
        {
            try
            {
                if (Activator.CreateInstance(type) is T instance)
                    instances.Add(instance);
            }
            catch (Exception e) when (e is MethodAccessException or MemberAccessException or MissingMemberException)
            {
                // Ignore if no public parameterless constructor is available.
            }
        }

        return instances;
    }

    private static IEnumerable<Type> GetSubclasses<T>()
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
            .Where(t => t is {IsClass: true, IsAbstract: false} && t.IsSubclassOf(typeof(T)));
    }
}
