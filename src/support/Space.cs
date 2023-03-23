﻿// <copyright file="Space.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     Contains all space management for the native client.
/// </summary>
public class Space
{
    private readonly Action<NativeObject> objectHandler;

    private Vector3d adjustment = Vector3d.Zero;

    private Camera? camera;

    private Light? light;

    /// <summary>
    ///     Create a new native space.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="objectHandler">A function that handles created native objects.</param>
    public Space(Client client, Action<NativeObject> objectHandler)
    {
        Client = client;
        this.objectHandler = objectHandler;
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
            if (camera == null)
            {
                camera = Native.GetCamera(Client);
                objectHandler(camera);
            }

            return camera;
        }
    }

    /// <summary>
    ///     Get the light of this space.
    /// </summary>
    public Light Light
    {
        get
        {
            if (light == null)
            {
                light = Native.GetLight(Client);
                objectHandler(light);
            }

            return light;
        }
    }

    /// <summary>
    ///     Create a new sequenced mesh object.
    /// </summary>
    public SequencedMeshObject CreateSequencedMeshObject()
    {
        SequencedMeshObject sequencedMeshObject = Native.CreateSequencedMeshObject(Client);
        objectHandler(sequencedMeshObject);

        return sequencedMeshObject;
    }

    /// <summary>
    ///     Create a new indexed mesh object.
    /// </summary>
    public IndexedMeshObject CreateIndexedMeshObject()
    {
        IndexedMeshObject indexedMeshObject = Native.CreateIndexedMeshObject(Client);
        objectHandler(indexedMeshObject);

        return indexedMeshObject;
    }

    /// <summary>
    ///     Set the adjustment performed by the camera.
    ///     All space objects are adjusted by this offset.
    /// </summary>
    /// <param name="newAdjustment">The new adjustment.</param>
    public void SetAdjustment(Vector3d newAdjustment)
    {
        adjustment = newAdjustment;
    }

    /// <summary>
    ///     Get the adjusted data of a spatial object.
    /// </summary>
    /// <param name="spatialObject">The spatial object.</param>
    /// <returns>The adjusted data.</returns>
    public SpatialObjectData GetAdjustedData(SpatialObject spatialObject)
    {
        Vector3d adjustedPosition = spatialObject.Position + adjustment;
        Quaterniond adjustedRotation = spatialObject.Rotation;

        Vector3 adaptedPosition = new((float) adjustedPosition.X, (float) adjustedPosition.Y, (float) adjustedPosition.Z);
        Vector4 adaptedRotation = new((float) adjustedRotation.X, (float) adjustedRotation.Y, (float) adjustedRotation.Z, (float) adjustedRotation.W);

        return new SpatialObjectData
        {
            Position = adaptedPosition,
            Rotation = adaptedRotation
        };
    }
}
