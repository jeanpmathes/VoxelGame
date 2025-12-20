// <copyright file="Result.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Result with no value.
/// </summary>
public class Result
{
    private static readonly Result singleton = new(exception: null);

    private readonly Exception? exception;

    /// <summary>
    ///     Create a result.
    /// </summary>
    /// <param name="exception">The exception, if any.</param>
    protected Result(Exception? exception)
    {
        this.exception = exception;
    }

    /// <summary>
    ///     Throw an exception if the result is an error.
    /// </summary>
    public void ThrowIfError()
    {
        if (exception != null) throw exception;
    }

    /// <summary>
    ///     Switch on the result.
    /// </summary>
    public void Switch(Action ok, Action<Exception> error)
    {
        if (exception == null) ok();
        else error(exception);
    }

    /// <summary>
    ///     Switch on the result, returning a value.
    /// </summary>
    public TResult Switch<TResult>(Func<TResult> ok, Func<Exception, TResult> error)
    {
        return exception == null ? ok() : error(exception);
    }

    /// <summary>
    ///     Create a result that is not an error.
    /// </summary>
    public static Result Ok()
    {
        return singleton;
    }

    /// <inheritdoc cref="Result{T}.Ok(T)" />
    public static Result<T> Ok<T>(T value)
    {
        return Result<T>.Ok(value);
    }

    /// <summary>
    ///     Create a result that is an error.
    /// </summary>
    public static Result Error(Exception exception)
    {
        return new Result(exception);
    }

    /// <inheritdoc cref="Result{T}.Error" />
    public static Result<T> Error<T>(Exception exception)
    {
        return Result<T>.Error(exception);
    }
}

/// <summary>
///     A result of an operation, is either a value or an exception.
/// </summary>
public class Result<T> : Result
{
    private readonly T? value;

    private Result(T value) : base(exception: null)
    {
        this.value = value;
    }

    private Result(Exception exception) : base(exception) {}

    /// <summary>
    ///     Create a result that is a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static Result<T> Ok(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    ///     Create a result that is an error.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The result.</returns>
    public new static Result<T> Error(Exception exception)
    {
        return new Result<T>(exception);
    }

    /// <summary>
    ///     Switch on the result.
    /// </summary>
    public void Switch(Action<T> ok, Action<Exception> error)
    {
        Switch(() => { ok(value!); }, error);
    }

    /// <summary>
    ///     Switch on the result, returning a value.
    /// </summary>
    public TResult Switch<TResult>(Func<T, TResult> ok, Func<Exception, TResult> error)
    {
        return Switch(() => ok(value!), error);
    }

    /// <summary>
    ///     Map the result to a new result.
    /// </summary>
    /// <param name="mapper">The mapping function.</param>
    /// <typeparam name="TResult">The type of the new result.</typeparam>
    /// <returns>The new result.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return Switch(
            v => Ok(mapper(v)),
            Error<TResult>
        );
    }

    /// <summary>
    ///     Unwrap the result, throwing an exception if it is an error.
    /// </summary>
    public T UnwrapOrThrow()
    {
        ThrowIfError();

        return value!;
    }

    /// <summary>
    ///     Unwrap the result, returning a fallback value if it is an error.
    ///     This will not throw an exception.
    /// </summary>
    /// <param name="fallback">A function providing the fallback value.</param>
    /// <param name="error">The exception that occurred, if any.</param>
    /// <returns>The value.</returns>
    public T UnwrapWithFallback(Func<T> fallback, out Exception? error)
    {
        (T result, error) = Switch(
            () => (value!, (Exception?) null),
            e => (fallback(), e));

        return result;
    }
}

/// <summary>
///     Extension methods for results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    ///     Unwrap a result, using the null value if it is an error.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The value or null.</returns>
    #pragma warning disable S4226 // Has to be extension because of type inference.
    public static T? UnwrapOrNull<T>(this Result<T> result) where T : class
    #pragma warning restore S4226
    {
        return result.Switch<T?>(
            v => v,
            _ => null
        );
    }
}
