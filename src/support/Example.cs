//  <copyright file="Example.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     An example client implementation.
/// </summary>
public class Example : Client
{
    private readonly List<(uint width, uint height)> resolutionOptions = new()
    {
        (800, 600),
        (1200, 900),
        (1280, 720),
        (1920, 1080),
        (1920, 1200),
        (2560, 1440),
        (3440, 1440),
        (3840, 2160)
    };

    private bool firstFrame = true;

    private int resolutionIndex;

    /// <inheritdoc />
    protected override void OnUpdate(double delta)
    {
        if (firstFrame)
        {
            firstFrame = false;

            IndexedMeshObject plane = Space.CreateIndexedMeshObject();

            SpatialVertex[] planeVertices =
            {
                new() {Position = (-1.5f, -.8f, -1.5f), Color = (1.0f, 1.0f, 1.0f, 1.0f)},
                new() {Position = (-1.5f, -.8f, +1.5f), Color = (1.0f, 1.0f, 1.0f, 1.0f)},
                new() {Position = (+1.5f, -.8f, +1.5f), Color = (1.0f, 1.0f, 1.0f, 1.0f)},
                new() {Position = (+1.5f, -.8f, -1.5f), Color = (1.0f, 1.0f, 1.0f, 1.0f)}
            };

            uint[] planeIndices = {0, 1, 2, 0, 2, 3};
            plane.SetMesh(planeVertices, planeIndices);

            SequencedMeshObject triangle = Space.CreateSequencedMeshObject();

            SpatialVertex[] triangleVertices =
            {
                new() {Position = (0.0f, 0.5f, 0.0f), Color = (1.0f, 0.0f, 0.0f, 1.0f)},
                new() {Position = (0.5f, -0.5f, 0.0f), Color = (0.0f, 1.0f, 0.0f, 1.0f)},
                new() {Position = (-0.5f, -0.5f, 0.0f), Color = (0.0f, 0.0f, 1.0f, 1.0f)}
            };

            triangle.SetMesh(triangleVertices);

            IndexedMeshObject cube = Space.CreateIndexedMeshObject();

            SpatialVertex[] cubeVertices =
            {
                // Bottom face
                new() {Position = (-0.5f, -0.5f, -0.5f), Color = (1.0f, 0f, 0f, 1.0f)},
                new() {Position = (0.5f, -0.5f, -0.5f), Color = (0f, 0.5f, 0f, 1.0f)},
                new() {Position = (0.5f, -0.5f, 0.5f), Color = (0f, 0f, 1.0f, 1.0f)},
                new() {Position = (-0.5f, -0.5f, 0.5f), Color = (1.0f, 1.0f, 0f, 1.0f)},
                // Top face
                new() {Position = (-0.5f, 0.5f, -0.5f), Color = (0f, 1.0f, 1.0f, 1.0f)},
                new() {Position = (0.5f, 0.5f, -0.5f), Color = (1.0f, 0f, 1.0f, 1.0f)},
                new() {Position = (0.5f, 0.5f, 0.5f), Color = (1.0f, 1.0f, 1.0f, 1.0f)},
                new() {Position = (-0.5f, 0.5f, 0.5f), Color = (0f, 0f, 0f, 1.0f)}
            };

            uint[] cubeIndices =
            {
                // Bottom face
                0, 1, 2,
                0, 2, 3,
                // Top face
                4, 6, 5,
                4, 7, 6,
                // Front face
                3, 2, 6,
                3, 6, 7,
                // Back face
                0, 5, 1,
                0, 4, 5,
                // Left face
                0, 3, 7,
                0, 7, 4,
                // Right face
                1, 5, 6,
                1, 6, 2
            };

            cube.SetMesh(cubeVertices, cubeIndices);
            cube.Position = new Vector3d(x: 3.0f, y: 0.0f, z: 3.0f);
        }

        Space.Light.Position = new Vector3d(x: 2.0f, y: 2.0f, Math.Sin(Time) * 2.0f);

        if (KeyState.IsKeyPressed(VirtualKeys.Space)) ToggleFullscreen();

        if (KeyState.IsKeyPressed(VirtualKeys.Left))
        {
            resolutionIndex = (resolutionIndex - 1 + resolutionOptions.Count) % resolutionOptions.Count;
            SetResolution(resolutionOptions[resolutionIndex].width, resolutionOptions[resolutionIndex].height);
        }

        if (KeyState.IsKeyPressed(VirtualKeys.Right))
        {
            resolutionIndex = (resolutionIndex + 1) % resolutionOptions.Count;
            SetResolution(resolutionOptions[resolutionIndex].width, resolutionOptions[resolutionIndex].height);
        }

        if (KeyState.IsKeyDown(VirtualKeys.W)) Space.Camera.Position += Vector3d.UnitX * delta;

        if (KeyState.IsKeyDown(VirtualKeys.S)) Space.Camera.Position -= Vector3d.UnitX * delta;

        if (KeyState.IsKeyDown(VirtualKeys.A)) Space.Camera.Position -= Vector3d.UnitZ * delta;

        if (KeyState.IsKeyDown(VirtualKeys.D)) Space.Camera.Position += Vector3d.UnitZ * delta;
    }
}

