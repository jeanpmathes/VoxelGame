// <copyright file="Space.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics.Raytracing;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Core;

/// <summary>
///     Contains all space management for the native client.
/// </summary>
public class Space
{
    private Vector3d adjustment = Vector3d.Zero;

    private Camera? camera;

    private Light? light;

    /// <summary>
    ///     Create a new native space.
    /// </summary>
    /// <param name="client">The client.</param>
    public Space(Client client)
    {
        Client = client;
    }

    /// <summary>
    ///     Get the client of this space.
    /// </summary>
    public Client Client { get; }

    /// <summary>
    ///     Get the camera of this space.
    /// </summary>
    public Camera Camera
    {
        get
        {
            camera ??= Native.GetCamera(Client);

            return camera;
        }
    }

    /// <summary>
    ///     Get the light of this space.
    /// </summary>
    public Light Light
    {
        get { return light ??= Native.GetLight(Client); }
    }

    /// <summary>
    ///     Check whether the adjustment has changed since the last set.
    /// </summary>
    public bool HasAdjustmentChanged { get; private set; }

    /// <summary>
    ///     Create a new mesh.
    /// </summary>
    /// <param name="material">The material. It cannot be changed later.</param>
    /// <param name="position">The initial position.</param>
    /// <param name="rotation">The initial rotation.</param>
    public Mesh CreateMesh(Material material, Vector3d position = default, Quaterniond rotation = default)
    {
        Mesh mesh = Native.CreateMesh(Client, material.Index);

        mesh.Position = position;
        mesh.Rotation = rotation;

        return mesh;
    }

    /// <summary>
    ///     Set the adjustment performed by the camera.
    ///     All space objects are adjusted by this offset.
    /// </summary>
    /// <param name="newAdjustment">The new adjustment.</param>
    public void SetAdjustment(Vector3d newAdjustment)
    {
        if (VMath.NearlyEqual(adjustment, newAdjustment))
        {
            HasAdjustmentChanged = false;
        }
        else
        {
            HasAdjustmentChanged = true;
            adjustment = newAdjustment;
        }
    }

    /// <summary>
    ///     Get the adjusted data of a spatial object.
    /// </summary>
    /// <param name="spatial">The spatial object.</param>
    /// <returns>The adjusted data.</returns>
    public SpatialData GetAdjustedData(Spatial spatial)
    {
        Vector3d adjustedPosition = spatial.Position + adjustment;
        Quaterniond adjustedRotation = spatial.Rotation;

        Vector3 adaptedPosition = new((float) adjustedPosition.X, (float) adjustedPosition.Y, (float) adjustedPosition.Z);
        Vector4 adaptedRotation = new((float) adjustedRotation.X, (float) adjustedRotation.Y, (float) adjustedRotation.Z, (float) adjustedRotation.W);

        return new SpatialData
        {
            Position = adaptedPosition,
            Rotation = adaptedRotation
        };
    }
}
