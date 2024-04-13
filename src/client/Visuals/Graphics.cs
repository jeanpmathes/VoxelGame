// <copyright file="Graphics.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Utility class for graphics related commands.
///     This mainly affects the rendering of 3D content.
/// </summary>
public class Graphics
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Graphics>();

    private static readonly Pipelines.RaytracingData defaultData = new()
    {
        wireframe = false,
        windDirection = new Vector3(x: 0.7f, y: 0.0f, z: 0.7f).Normalized()
    };

    private readonly Pipelines? pipelines;

    private Graphics(Pipelines? pipelines)
    {
        this.pipelines = pipelines;
    }

    /// <summary>
    ///     Get the active instance controlling the graphics.
    /// </summary>
    public static Graphics Instance { get; private set; } = null!;

    /// <summary>
    ///     Initializes the graphics.
    /// </summary>
    /// <param name="pipelines">All pipelines used for rendering.</param>
    public static void Initialize(Pipelines? pipelines)
    {
        Debug.Assert(Instance == null);
        Instance = new Graphics(pipelines);

        Instance.Reset();
    }

    /// <summary>
    ///     Resets the graphics to the default state.
    /// </summary>
    public void Reset()
    {
        if (pipelines == null) return;

        pipelines.RaytracingDataBuffer.Data = defaultData;
    }

    /// <summary>
    ///     Sets the wireframe mode.
    /// </summary>
    /// <param name="enable">Whether to enable wireframe rendering.</param>
    public void SetWireframe(Boolean enable)
    {
        if (pipelines == null) return;

        pipelines.RaytracingDataBuffer.Modify((ref Pipelines.RaytracingData data) => data.wireframe = enable);

        logger.LogDebug("Wireframe mode set to {Mode}", enable);
    }

    /// <summary>
    ///     Configure the fog values to describe the overlap between the fog and the view plane.
    ///     This is necessary when the view plane is inside of a fog volume.
    /// </summary>
    /// <param name="size">
    ///     The size of the overlap between the fog and the view plane.
    ///     Given in relative height, positive values start from the bottom, negative values from the top.
    ///     If the view plane is not inside of the fog, this value should be 0.
    /// </param>
    /// <param name="color">The color of the fog.</param>
    public void SetFogOverlapConfiguration(Double size, Color4 color)
    {
        pipelines?.RaytracingDataBuffer.Modify((ref Pipelines.RaytracingData data) =>
        {
            data.fogOverlapSize = (Single) size;
            data.fogOverlapColor = color.ToVector3();
        });
    }
}
