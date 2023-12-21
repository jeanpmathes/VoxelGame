﻿// <copyright file="Graphics.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Utility class for graphics related commands.
///     This mainly affects the rendering of 3D content.
/// </summary>
public class Graphics
{
    private readonly Pipelines pipelines;

    private Graphics(Pipelines pipelines)
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
    public static void Initialize(Pipelines pipelines)
    {
        Debug.Assert(Instance == null);
        Instance = new Graphics(pipelines);
    }

    /// <summary>
    ///     Resets the graphics to the default state.
    /// </summary>
    public void Reset()
    {
        SetWireframe(enable: false);
    }

    /// <summary>
    ///     Sets the wireframe mode.
    /// </summary>
    /// <param name="enable">Whether to enable wireframe rendering.</param>
    public void SetWireframe(bool enable)
    {
        pipelines.RaytracingDataBuffer.Modify((ref Pipelines.RaytracingData data) => data.wireframe = enable);
    }
}
