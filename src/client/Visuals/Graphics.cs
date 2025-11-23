// <copyright file="Graphics.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Utility class for graphics related commands.
///     This mainly affects the rendering of 3D content.
/// </summary>
public partial class Graphics
{
    private static readonly Engine.RaytracingData defaultData = new()
    {
        wireframe = false,
        windDirection = new Vector3(x: 0.7f, y: 0.0f, z: 0.7f).Normalized(),
        antiAliasing =
        {
            isEnabled = false,
            minGridSize = 1,
            maxGridSize = 1,
            varianceThreshold = 0.0f
        }
    };

    private static readonly Engine.PostProcessingData defaultPostProcessingData = new()
    {
        fxaa = new Engine.FxaaSettings
        {
            isEnabled = false,
            contrastThreshold = 0.0f,
            relativeThreshold = 0.0f,
            subpixelBlending = 0.0f,
            edgeStepCount = 0,
            edgeStep = 0,
            edgeGuess = 0.0f
        }
    };

    private Engine? engine;

    private Graphics() {}

    /// <summary>
    ///     Get the active instance controlling the graphics.
    /// </summary>
    public static Graphics Instance { get; } = new();

    /// <summary>
    ///     Initializes the graphics.
    /// </summary>
    /// <param name="e">The engine to use for rendering.</param>
    public void Initialize(Engine e)
    {
        Debug.Assert(engine == null);
        engine = e;

        LogGraphicsInitialized(logger);

        Instance.Reset();
    }

    /// <summary>
    ///     Resets the graphics to the default state.
    /// </summary>
    public void Reset()
    {
        if (engine == null) return;

        engine.RaytracingDataBuffer.Data = defaultData;
        engine.PostProcessingBuffer.Data = defaultPostProcessingData;

        LogGraphicsReset(logger);
    }

    /// <summary>
    ///     Sets the wireframe mode.
    /// </summary>
    /// <param name="enable">Whether to enable wireframe rendering.</param>
    public void SetWireframe(Boolean enable)
    {
        engine?.RaytracingDataBuffer.Modify((ref Engine.RaytracingData data) => data.wireframe = enable);
    }

    /// <summary>
    ///     Configure the fog values to describe the overlap between the fog and the view plane.
    ///     This is necessary when the view plane is inside a fog volume.
    /// </summary>
    /// <param name="size">
    ///     The size of the overlap between the fog and the view plane.
    ///     Given in relative height, positive values start from the bottom, negative values from the top.
    ///     If the view plane is not inside the fog, this value should be 0.
    /// </param>
    /// <param name="color">The color of the fog.</param>
    public void SetFogOverlapConfiguration(Double size, ColorS color)
    {
        engine?.RaytracingDataBuffer.Modify((ref Engine.RaytracingData data) =>
        {
            data.fogOverlapSize = (Single) size;
            data.fogOverlapColor = color.ToVector4().Xyz;
        });
    }

    /// <summary>
    ///     Set whether to display the sampling rate of the raytracing antialiasing algorithm.
    /// </summary>
    /// <param name="enable">Whether to enable the sampling rate display.</param>
    public void SetSamplingDisplay(Boolean enable)
    {
        engine?.RaytracingDataBuffer.Modify((ref Engine.RaytracingData data) => data.antiAliasing.showSamplingRate = enable);
    }

    private void SetRaytracingAntiAliasingConfiguration(Boolean enabled, Int32 min, Int32 max, Single variance, Single depth)
    {
        engine?.RaytracingDataBuffer.Modify((ref Engine.RaytracingData data) =>
        {
            data.antiAliasing.isEnabled = enabled;
            data.antiAliasing.minGridSize = min;
            data.antiAliasing.maxGridSize = max;
            data.antiAliasing.varianceThreshold = variance;
            data.antiAliasing.depthThreshold = depth;
        });
    }

    private static (Boolean enabled, Int32 min, Int32 max, Single variance, Single depth) GetRaytracingAntiAliasingConfiguration(Quality quality)
    {
        return quality switch
        {
            Quality.Low => (false, 1, 1, 0.0f, 0.0f),
            Quality.Medium => (true, 2, 3, 0.016f, 0.0025f),
            Quality.High => (true, 2, 4, 0.008f, 0.0015f),
            Quality.Ultra => (true, 3, 5, 0.004f, 0.0010f),
            _ => throw Exceptions.UnsupportedEnumValue(quality)
        };
    }

    private void SetPostProcessingAntiAliasingConfiguration(
        Boolean enabled, 
        Single contrastThreshold, Single relativeThreshold, 
        Single subpixelBlending, 
        Int32 edgeStepCount, Int32 edgeStep, Single edgeGuess)
    {
        engine?.PostProcessingBuffer.Modify((ref Engine.PostProcessingData data) =>
        {
            data.fxaa.isEnabled = enabled;
            data.fxaa.contrastThreshold = contrastThreshold;
            data.fxaa.relativeThreshold = relativeThreshold;
            data.fxaa.subpixelBlending = subpixelBlending;
            data.fxaa.edgeStepCount = edgeStepCount;
            data.fxaa.edgeStep = edgeStep;
            data.fxaa.edgeGuess = edgeGuess;
        });
    }

    private static (Boolean, Single, Single, Single, Int32, Int32, Single) GetPostProcessingAntiAliasingConfiguration(Quality quality)
    {
        return quality switch
        {
            Quality.Low => (false, 0.0f, 0.0f, 0.0f, 0, 0, 0.0f),
            Quality.Medium => (true, 0.0833f, 0.333f, 0.50f, 4, 2, 12.0f),
            Quality.High => (true, 0.0625f, 0.166f, 0.75f, 8, 2, 8.0f),
            Quality.Ultra => (true, 0.0312f, 0.063f, 1.00f, 12, 1, 8.0f),
            _ => throw Exceptions.UnsupportedEnumValue(quality)
        };
    }

    /// <summary>
    ///     Apply a quality preset to the ray generation antialiasing algorithm.
    /// </summary>
    /// <param name="quality">The selected quality preset.</param>
    public void ApplyRenderingAntiAliasingQuality(Quality quality)
    {
        (Boolean enabled, Int32 initial, Int32 max, Single variance, Single depth) configuration = GetRaytracingAntiAliasingConfiguration(quality);
        SetRaytracingAntiAliasingConfiguration(configuration.enabled, configuration.initial, configuration.max, configuration.variance, configuration.depth);
    }

    /// <summary>
    ///     Apply a quality preset to the post-processing antialiasing pass.
    /// </summary>
    /// <param name="quality">The selected quality preset.</param>
    public void ApplyPostProcessingAntiAliasingQuality(Quality quality)
    {
        (Boolean enabled, Single contrastThreshold, Single relativeThreshold, Single subpixelBlending, Int32 edgeStepCount, Int32 edgeStep, Single edgeGuess) 
            configuration = GetPostProcessingAntiAliasingConfiguration(quality);

        SetPostProcessingAntiAliasingConfiguration(
            configuration.enabled, 
            configuration.contrastThreshold, configuration.relativeThreshold, 
            configuration.subpixelBlending, 
            configuration.edgeStepCount, configuration.edgeStep, configuration.edgeGuess);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Graphics>();

    [LoggerMessage(EventId = LogID.Graphics + 0, Level = LogLevel.Debug, Message = "Graphics initialized with engine")]
    private static partial void LogGraphicsInitialized(ILogger logger);

    [LoggerMessage(EventId = LogID.Graphics + 1, Level = LogLevel.Debug, Message = "Graphics reset to default state")]
    private static partial void LogGraphicsReset(ILogger logger);

    #endregion LOGGING
}
