// <copyright file="ObjectPool.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A polyline is a function that is created from multiple line segments.
///     The line segments are defined by a list of points.
/// </summary>
public class Polyline
{
    /// <summary>
    ///     Create a new polyline.
    ///     The default behaviour of the left and right functions is to return the value of the first and last point.
    /// </summary>
    public Polyline()
    {
        Left = _ => Points[index: 0].Y;
        Right = _ => Points[^1].Y;
    }

    /// <summary>
    ///     The left function, which is used outside of the interval defined by the points.
    /// </summary>
    public Func<Double, Double> Left { get; init; }

    /// <summary>
    ///     The list of points that define the line segments. There must be at least two points.
    /// </summary>
    public IList<Vector2d> Points { get; } = new List<Vector2d>();

    /// <summary>
    ///     The right function, which is used outside of the interval defined by the points.
    /// </summary>
    public Func<Double, Double> Right { get; init; }

    /// <summary>
    ///     Evaluates the function at the given position.
    /// </summary>
    /// <param name="x">The position to evaluate at.</param>
    /// <returns>The value of the function at the given position.</returns>
    public Double Evaluate(Double x)
    {
        Debug.Assert(Points.Count >= 2);

        if (x < Points[index: 0].X) return Left(x);

        for (var i = 0; i < Points.Count - 1; i++)
        {
            Vector2d a = Points[i];
            Vector2d b = Points[i + 1];

            if (x < b.X) return MathHelper.Lerp(a.Y, b.Y, MathTool.InverseLerp(a.X, b.X, x));
        }

        return Right(x);
    }
}
