// <copyright file="PlayerInput.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Client.Inputs;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Input.Actions;
using VoxelGame.Support.Input.Composite;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Contains all player input.
/// </summary>
internal sealed class Input
{
    private const Single InteractionCooldown = 0.25f;
    
    private readonly Button blockInteractButton;
    private readonly Button crouchButton;
    private readonly Button destroyButton;

    private readonly Button interactOrPlaceButton;
    private readonly Button jumpButton;
    private readonly InputAxis2 movementInput;

    private readonly ToggleButton placementModeToggle;

    private readonly InputAxis selectionAxis;
    private readonly PushButton selectTargetedButton;
    private readonly Button sprintButton;

    private Double timer;

    internal Input()
    {
        KeybindManager keybind = Application.Client.Instance.Keybinds;

        Button forwardsButton = keybind.GetButton(keybind.Forwards);
        Button backwardsButton = keybind.GetButton(keybind.Backwards);
        Button strafeRightButton = keybind.GetButton(keybind.StrafeRight);
        Button strafeLeftButton = keybind.GetButton(keybind.StrafeLeft);

        movementInput = new InputAxis2(
            new InputAxis(forwardsButton, backwardsButton),
            new InputAxis(strafeRightButton, strafeLeftButton));

        sprintButton = keybind.GetButton(keybind.Sprint);
        jumpButton = keybind.GetButton(keybind.Jump);
        crouchButton = keybind.GetButton(keybind.Crouch);

        interactOrPlaceButton = keybind.GetButton(keybind.InteractOrPlace);
        destroyButton = keybind.GetButton(keybind.Destroy);
        blockInteractButton = keybind.GetButton(keybind.BlockInteract);

        placementModeToggle = keybind.GetToggle(keybind.PlacementMode);
        placementModeToggle.Clear();

        selectTargetedButton = keybind.GetPushButton(keybind.SelectTargeted);

        Button nextButton = keybind.GetPushButton(keybind.NextPlacement);
        Button previousButton = keybind.GetPushButton(keybind.PreviousPlacement);
        selectionAxis = new InputAxis(nextButton, previousButton);
    }

    internal Boolean ShouldJump => jumpButton.IsDown;

    internal Boolean ShouldCrouch => crouchButton.IsDown;

    private Boolean IsCooldownOver => timer >= InteractionCooldown;

    internal Boolean ShouldInteract => IsCooldownOver && interactOrPlaceButton.IsDown;

    internal Boolean ShouldDestroy => IsCooldownOver && destroyButton.IsDown;

    internal Boolean ShouldChangePlacementMode => placementModeToggle.Changed;

    internal Boolean ShouldSelectTargeted => selectTargetedButton.IsDown;

    internal Boolean IsInteractionBlocked => blockInteractButton.IsDown;

    /// <summary>
    ///     Get the movement decided by the user input for an orientable object.
    /// </summary>
    /// <param name="orientable">An orientable object.</param>
    /// <param name="normalSpeed">The factor to use for normal speed.</param>
    /// <param name="sprintSpeed">The factor to use for sprint speed.</param>
    /// <param name="allowFlying">Whether flying is allowed.</param>
    /// <returns>The movement vector.</returns>
    internal Vector3d GetMovement(IOrientable orientable, Double normalSpeed, Double sprintSpeed, Boolean allowFlying)
    {
        (Single x, Single z) = movementInput.Value;
        Single y = (ShouldJump.ToInt() - ShouldCrouch.ToInt()) * allowFlying.ToInt();

        Vector3d movement = x * orientable.Forward + z * orientable.Right + y * Vector3d.UnitY;

        if (movement != Vector3d.Zero)
            movement = sprintButton.IsDown
                ? movement.Normalized() * sprintSpeed
                : movement.Normalized() * normalSpeed;

        return movement;
    }

    internal void Update(Double deltaTime)
    {
        timer += deltaTime;
    }

    internal void RegisterInteraction()
    {
        timer = 0;
    }

    internal Int32 GetSelectionChange()
    {
        return Math.Sign(selectionAxis.Value);
    }
}
