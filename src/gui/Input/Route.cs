// <copyright file="Route.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Represents a route of visuals from the root to a target visual.
/// </summary>
public sealed class Route : IDisposable
{
    /// <summary>
    ///     The list of visuals in the route, ordered from the target visual to the root visual.
    /// </summary>
    private readonly List<Visual> visuals;

    private Route(List<Visual> visuals)
    {
        this.visuals = visuals;
    }

    /// <summary>
    ///     Create a new empty <seealso cref="Route" />.
    /// </summary>
    public static Route Empty => new(GetList());

    /// <summary>
    ///     Get the root visual of the route.
    /// </summary>
    public Visual Root => visuals[^1];

    /// <summary>
    ///     Get the target visual of the route.
    /// </summary>
    public Visual Target => visuals[0];

    /// <summary>
    ///     The number of visuals in the route.
    /// </summary>
    public Int32 Count => visuals.Count;

    /// <inheritdoc />
    public void Dispose()
    {
        ReturnList(visuals);
    }

    /// <summary>
    ///     Creates a new <seealso cref="Route" /> for the given target visual. The route will include the target visual and
    ///     all of its ancestors.
    /// </summary>
    /// <param name="target">The target visual to create the route for, or <c>null</c> to create an empty route.</param>
    /// <returns>A new <seealso cref="Route" /> instance representing the route.</returns>
    public static Route Create(Visual? target)
    {
        if (target == null)
            return Empty;

        List<Visual> route = GetList();

        Visual? current = target;

        while (current != null)
        {
            route.Add(current);
            current = current.Parent;
        }

        return new Route(route);
    }

    /// <summary>
    ///     Get a visual from the route by its index from the top (root).
    /// </summary>
    /// <param name="index">The index of the visual to get, where 0 is the root visual.</param>
    /// <returns>The visual at the specified index from the top of the route.</returns>
    public Visual GetFromTop(Int32 index)
    {
        return visuals[^(index + 1)];
    }

    /// <summary>
    ///     Get a visual from the route by its index from the bottom (target).
    /// </summary>
    /// <param name="index">The index of the visual to get, where 0 is the target visual.</param>
    /// <returns>The visual at the specified index from the bottom of the route.</returns>
    public Visual GetFromBottom(Int32 index)
    {
        return visuals[index];
    }

    /// <summary>
    ///     Find the first index from the top (root) where the two routes differ.
    ///     If the routes are identical up to the length of the shorter route, the index returned will be equal to the length
    ///     of the shorter route.
    /// </summary>
    /// <param name="route1">The first route to compare.</param>
    /// <param name="route2">The second route to compare.</param>
    /// <returns>
    ///     The index from the top where the two routes first differ, or the length of the shorter route if they are
    ///     identical up to that point.
    /// </returns>
    public static Int32 FindFirstDifferenceFromTop(Route route1, Route route2)
    {
        Int32 lowerCount = Math.Min(route1.Count, route2.Count);

        for (Int32 index = 0; index < lowerCount; index++)
        {
            if (route1.GetFromTop(index) != route2.GetFromTop(index))
                return index;
        }

        return lowerCount;
    }

    #region POOLING

    private static readonly ConcurrentBag<List<Visual>> listPool = [];

    private static List<Visual> GetList()
    {
        if (!listPool.TryTake(out List<Visual>? list)) return [];

        list.Clear();

        return list;
    }

    private static void ReturnList(List<Visual> list)
    {
        listPool.Add(list);
    }

    #endregion POOLING
}
