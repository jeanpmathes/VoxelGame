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
        windDirection = new Vector3(x: 0.7f, y: 0.0f, z: 0.7f).Normalized()
    };

    private readonly Engine? engine;

    private Graphics(Engine? engine)
    {
        this.engine = engine;
    }

    /// <summary>
    ///     Get the active instance controlling the graphics.
    /// </summary>
    public static Graphics Instance { get; private set; } = null!;

    /// <summary>
    ///     Initializes the graphics.
    /// </summary>
    /// <param name="engine">The engine to use for rendering.</param>
    public static void Initialize(Engine engine)
    {
        Debug.Assert(Instance == null);
        Instance = new Graphics(engine);

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

        LogGraphicsReset(logger);
    }

    /// <summary>
    ///     Sets the wireframe mode.
    /// </summary>
    /// <param name="enable">Whether to enable wireframe rendering.</param>
    public void SetWireframe(Boolean enable)
    {
        if (engine == null) return;

        engine.RaytracingDataBuffer.Modify((ref Engine.RaytracingData data) => data.wireframe = enable);

        LogSetWireframe(logger, enable);
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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Graphics>();

    [LoggerMessage(EventId = LogID.Graphics + 0, Level = LogLevel.Debug, Message = "Graphics initialized with engine")]
    private static partial void LogGraphicsInitialized(ILogger logger);

    [LoggerMessage(EventId = LogID.Graphics + 1, Level = LogLevel.Debug, Message = "Graphics reset to default state")]
    private static partial void LogGraphicsReset(ILogger logger);

    [LoggerMessage(EventId = LogID.Graphics + 2, Level = LogLevel.Debug, Message = "Wireframe mode set to {Mode}")]
    private static partial void LogSetWireframe(ILogger logger, Boolean mode);

    #endregion LOGGING
}
