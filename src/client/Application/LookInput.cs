// <copyright file="LookInput.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Input.Devices;

namespace VoxelGame.Client.Application;

/// <summary>
///     Wraps input sources to provide action data for look movement.
/// </summary>
public class LookInput
{
    private readonly Mouse mouse;

    private float sensitivity;

    /// <summary>
    ///     Create a new look input wrapper.
    /// </summary>
    /// <param name="mouse">The mouse providing the movement.</param>
    /// <param name="sensitivity">The sensitivity to apply to the mouse movement.</param>
    public LookInput(Mouse mouse, float sensitivity)
    {
        this.mouse = mouse;
        this.sensitivity = sensitivity;
    }

    /// <summary>
    ///     Get the input value.
    /// </summary>
    public Vector2d Value => mouse.Delta * sensitivity;

    /// <summary>
    ///     Set the sensitivity of the look input.
    /// </summary>
    /// <param name="newSensitivity">The new sensitivity.</param>
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
    }
}