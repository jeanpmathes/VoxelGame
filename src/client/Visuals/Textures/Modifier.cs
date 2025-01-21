// <copyright file="Modifier.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Defines the base class of an image modifier.
///     Modifiers are always applied to single images and can return a sheet of images.
///     One can inherit from this class to create custom modifiers.
///     Custom modifiers are detected by reflection.
/// </summary>
public abstract class Modifier
{
    private readonly Parameter[] @params;

    /// <summary>
    ///     Create a new modifier.
    /// </summary>
    /// <param name="type">The type of this modifier. Used as a key to find the correct modifier.</param>
    /// <param name="params">The parameters of the modifier.</param>
    protected Modifier(String type, params Parameter[] @params)
    {
        Type = type;

        this.@params = @params;
    }

    /// <summary>
    ///     The type of this modifier. Used as a key to find the correct modifier.
    /// </summary>
    public String Type { get; }

    /// <summary>
    ///     Modify the given image.
    /// </summary>
    /// <param name="image">The image - can be modified in place.</param>
    /// <param name="parameters">The parameters of the modifier.</param>
    /// <param name="context">The context in which the modifier is executed.</param>
    /// <returns>The resulting sheet of images, or <c>null</c> if the modifier is not applicable.</returns>
    public Sheet? Modify(Image image, IReadOnlyDictionary<String, String> parameters, IContext context)
    {
        Parameters? parsed = ParseParameters(parameters, context);

        return parsed != null ? Modify(image, parsed, context) : null;

    }

    /// <summary>
    ///     Modify the given image.
    /// </summary>
    /// <param name="image">The image - can be modified in place.</param>
    /// <param name="parameters">The parsed parameters of the modifier.</param>
    /// <param name="context">The context in which the modifier is executed.</param>
    /// <returns>The resulting sheet of images.</returns>
    protected abstract Sheet Modify(Image image, Parameters parameters, IContext context);

    /// <summary>
    ///     Wrap an image in a sheet, without copying it.
    /// </summary>
    protected static Sheet Wrap(Image image)
    {
        return new Sheet(width: 1, height: 1)
        {
            [x: 0, y: 0] = image
        };
    }

    private Parameters? ParseParameters(IReadOnlyDictionary<String, String> parameters, IContext context)
    {
        var failed = false;
        var parsed = new Dictionary<Parameter, Object>();
        var unknown = new HashSet<String>(parameters.Keys);

        foreach (Parameter parameter in @params)
        {
            String? value = parameters.GetValueOrDefault(parameter.Name);
            Object? parsedValue = parameter.DetermineValue(value);

            if (parsedValue == null)
            {
                context.ReportWarning($"Failed to provide parameter '{parameter.Name}'");
                failed = true;
            }
            else parsed[parameter] = parsedValue;

            unknown.Remove(parameter.Name);
        }

        foreach (String name in unknown)
        {
            context.ReportWarning($"Unknown parameter '{name}'");
        }

        return failed ? null : new Parameters(parsed);
    }

    /// <summary>
    /// Create a new color parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="fallback">The optional fallback value.</param>
    /// <returns>The created color parameter.</returns>
    protected static Parameter<ColorS> CreateColorParameter(String name, ColorS? fallback = null)
    {
        return new ColorParameter(name, fallback);
    }

    /// <summary>
    /// Create a new double parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="fallback">The optional fallback value.</param>
    /// <returns>The created double parameter.</returns>
    protected static Parameter<Double> CreateDoubleParameter(String name, Double? fallback = null)
    {
        return new DoubleParameter(name, fallback);
    }

    /// <summary>
    ///     Create a new boolean parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="fallback">The optional fallback value.</param>
    /// <returns>The created boolean parameter.</returns>
    protected static Parameter<Boolean> CreateBooleanParameter(String name, Boolean? fallback = null)
    {
        return new BooleanParameter(name, fallback);
    }

    /// <summary>
    /// The context in which the modifier is executed.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        ///     Get the position of the image in the sheet currently being processed.
        /// </summary>
        public Vector2i Position { get; }

        /// <summary>
        ///     Get the size of the sheet currently being processed.
        /// </summary>
        public Vector2i Size { get; }

        /// <summary>
        /// Report a warning.
        /// </summary>
        /// <param name="message">The message of the warning.</param>
        public void ReportWarning(String message);
    }

    /// <summary>
    /// Contains all parsed parameters of a modifier.
    /// </summary>
    /// <param name="parameters">The parsed parameters of the modifier.</param>
    protected class Parameters(Dictionary<Parameter, Object> parameters)
    {
        /// <summary>
        ///     Get the value of the given parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <returns>The value of the parameter.</returns>
        public T Get<T>(Parameter<T> parameter) where T : notnull
        {
            return (T) parameters[parameter];
        }
    }

    /// <summary>
    /// Base class for a parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    protected abstract class Parameter(String name)
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public String Name { get; } = name;

        /// <summary>
        /// Determine the value of the parameter.
        /// </summary>
        /// <param name="input">The string input of the parameter, can be <c>null</c>.</param>
        /// <returns>The value of the parameter, or <c>null</c> if the parameter failed.</returns>
        public abstract Object? DetermineValue(String? input);
    }

    /// <summary>
    /// Specific and typed base class for a parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="fallback">An optional fallback value, if not set the parameter is required.</param>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    protected abstract class Parameter<T>(String name, Object? fallback) : Parameter(name) where T : notnull
    {
        /// <summary>
        /// Get the value of the parameter.
        /// </summary>
        /// <param name="parameters">The current parameters of the modifier.</param>
        /// <returns>The value of the parameter.</returns>
        public T Get(Parameters parameters)
        {
            return parameters.Get(this);
        }

        /// <inheritdoc />
        public override Object? DetermineValue(String? input)
        {
            return input != null ? Parse(input) : fallback;
        }

        /// <summary>
        /// Parse the input text to the parameter type.
        /// </summary>
        /// <param name="text">A string representation of the parameter.</param>
        /// <returns>An object of the parameter type, or <c>null</c> if the parsing failed.</returns>
        protected abstract Object? Parse(String text);
    }

    private sealed class ColorParameter(String name, ColorS? fallback) : Parameter<ColorS>(name, fallback)
    {
        protected override Object? Parse(String text)
        {
            return ColorS.FromString(text);
        }
    }

    private sealed class DoubleParameter(String name, Double? fallback) : Parameter<Double>(name, fallback)
    {
        protected override Object? Parse(String text)
        {
            return Double.TryParse(text, CultureInfo.InvariantCulture, out Double result) ? result : null;
        }
    }

    private sealed class BooleanParameter(String name, Boolean? fallback) : Parameter<Boolean>(name, fallback)
    {
        protected override Object? Parse(String text)
        {
            return Boolean.TryParse(text, out Boolean result) ? result : null;
        }
    }
}
