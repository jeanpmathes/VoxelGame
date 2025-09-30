// <copyright file="ContributorExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
///     Extensions for contributing to aspects.
/// </summary>
public static class ContributorExtensions
{
    /// <summary>
    ///     Contribute a constant value to a given aspect.
    /// </summary>
    /// <param name="aspect">The aspect to which the value should be contributed.</param>
    /// <param name="value">The constant value to contribute.</param>
    /// <param name="exclusive">Whether the contribution should be exclusive.</param>
    /// <typeparam name="TValue">The type of the value to contribute.</typeparam>
    /// <typeparam name="TContext">The type of context in which the aspect is evaluated.</typeparam>
    public static void ContributeConstant<TValue, TContext>(
        this Aspect<TValue, TContext> aspect,
        TValue value,
        Boolean exclusive = false)
    {
        aspect.Add(new ConstantContributor<TValue, TContext>(value), exclusive);
    }

    /// <summary>
    ///     Contribute a value derived from a function to a given aspect.
    /// </summary>
    /// <param name="aspect">The aspect to which the function should be contributed.</param>
    /// <param name="function">The function that takes the original value and context, and returns a new value.</param>
    /// <param name="exclusive">Whether the contribution should be exclusive.</param>
    /// <typeparam name="TValue">The type of the value to contribute.</typeparam>
    /// <typeparam name="TContext">The type of context in which the aspect is evaluated.</typeparam>
    public static void ContributeFunction<TValue, TContext>(
        this Aspect<TValue, TContext> aspect,
        Func<TValue, TContext, TValue> function,
        Boolean exclusive = false)
    {
        aspect.Add(new FunctionContributor<TValue, TContext>(function), exclusive);
    }

    private class ConstantContributor<TValue, TContext>(TValue value) : IContributor<TValue, TContext>
    {
        public TValue Contribute(TValue original, TContext context)
        {
            return value;
        }
    }

    private class FunctionContributor<TValue, TContext>(Func<TValue, TContext, TValue> function) : IContributor<TValue, TContext>
    {
        public TValue Contribute(TValue original, TContext context)
        {
            return function(original, context);
        }
    }
}
