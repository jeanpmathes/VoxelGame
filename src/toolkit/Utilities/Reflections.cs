// <copyright file="Reflections.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    ///     Get all properties of an object that have a certain type.
    /// </summary>
    /// <typeparam name="T">The type of the properties.</typeparam>
    /// <param name="target">The object to get the properties from.</param>
    /// <returns>The found properties.</returns>
    public static IEnumerable<PropertyInfo> GetPropertiesOfType<T>(Object target) where T : class
    {
        Type filterType = typeof(T);

        return target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(info => info.PropertyType == filterType);
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
            .Where(m => m.Name.Equals(name, StringComparison.InvariantCulture) && !m.IsStatic);
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
