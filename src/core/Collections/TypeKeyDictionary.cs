// <copyright file="TypeKeyDictionary.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Stores objects and allows access by type (and super-types) and with an optional secondary key.
///     This is a reflection-based container.
/// </summary>
/// <typeparam name="TKey">The type of the secondary key.</typeparam>
public class TypeKeyDictionary<TKey> where TKey : struct
{
    private readonly Dictionary<Type, Entry> storage = [];

    /// <summary>
    ///     Add a new object to the dictionary. This will make the object retrievable by its direct type and all super-types.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    /// <param name="key">
    ///     The key to use for the object.
    ///     Must be unique in the context of the object type(s).
    ///     Passing <c>null</c> will add the object without a key.
    /// </param>
    public void Add(Object obj, TKey? key)
    {
        foreach (Type type in GetTypeHierarchy(obj.GetType()))
        {
            if (!storage.TryGetValue(type, out Entry? entry))
            {
                entry = new Entry();
                storage[type] = entry;
            }

            if (key == null) entry.Unmapped.Add(obj);
            else if (!entry.Mapped.TryAdd(key.Value, obj))
                throw new ArgumentException($"An object with key {key} already exists for type {type.FullName}.");
        }
    }

    /// <summary>
    ///     Get an object that fulfills the specified type and has the specified key.
    /// </summary>
    /// <param name="key">The key of the object to get.</param>
    /// <typeparam name="TObject">The type of the object to get.</typeparam>
    /// <returns>The object if found, otherwise <c>null</c>.</returns>
    public TObject? Get<TObject>(TKey key) where TObject : class
    {
        if (!storage.TryGetValue(typeof(TObject), out Entry? entry))
            return null;

        if (!entry.Mapped.TryGetValue(key, out Object? obj))
            return null;

        return (TObject) obj;
    }

    /// <summary>
    ///     Get an object that fulfills the specified type.
    ///     This method will not return the object if there are multiple objects fulfilling the type.
    /// </summary>
    /// <typeparam name="TObject">The type of the objects to get.</typeparam>
    /// <returns>The object if found and unique, otherwise <c>null</c>.</returns>
    public TObject? Get<TObject>() where TObject : class
    {
        IEnumerable<TObject> objects = GetAll<TObject>().Take(count: 2).ToList();

        return objects.Count() == 1 ? objects.First() : null;
    }

    /// <summary>
    ///     Get all objects that fulfill the specified type.
    /// </summary>
    /// <typeparam name="TObject">The type of the objects to get.</typeparam>
    /// <returns>The objects if found, otherwise an empty collection.</returns>
    public IEnumerable<TObject> GetAll<TObject>() where TObject : class
    {
        if (!storage.TryGetValue(typeof(TObject), out Entry? entry))
            return [];

        IEnumerable<Object> mapped = entry.Mapped.Values;
        IEnumerable<Object> unmapped = entry.Unmapped;

        return mapped.Concat(unmapped).Cast<TObject>();
    }

    private static IEnumerable<Type> GetTypeHierarchy(Type type)
    {
        yield return type;

        foreach (Type interfaceType in type.GetInterfaces()) yield return interfaceType;

        Type? baseType = type.BaseType;

        while (baseType != null)
        {
            yield return baseType;

            baseType = baseType.BaseType;
        }
    }

    /// <summary>
    ///     Remove all objects that have the specified key.
    /// </summary>
    /// <param name="key">The key of the objects to remove, or <c>null</c> to remove all objects without a key.</param>
    public void Remove(TKey? key)
    {
        foreach (Entry entry in storage.Values)
            if (key == null) entry.Unmapped.Clear();
            else entry.Mapped.Remove(key.Value);
    }

    /// <summary>
    ///     Add all objects from another <see cref="TypeKeyDictionary{TKey}" /> to this one.
    /// </summary>
    /// <param name="source">The source dictionary to add objects from.</param>
    public void AddAll(TypeKeyDictionary<TKey> source)
    {
        Entry root = source.storage[typeof(Object)];

        foreach (Object obj in root.Unmapped) Add(obj, key: null);

        foreach ((TKey key, Object obj) in root.Mapped) Add(obj, key);
    }

    private sealed record Entry(List<Object> Unmapped, Dictionary<TKey, Object> Mapped)
    {
        public Entry() : this([], []) {}
    }
}
