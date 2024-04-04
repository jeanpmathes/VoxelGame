// <copyright file="PlayerInput.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Client.Inputs;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Input.Actions;
using VoxelGame.Support.Input.Composite;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Contains all player input.
/// </summary>
internal sealed class Input
{
    private readonly Button blockInteractButton;
    private readonly Button crouchButton;
    private readonly Button destroyButton;

    private readonly Single interactionCooldown = 0.25f;

    private readonly Button interactOrPlaceButton;
    private readonly Button jumpButton;
    private readonly InputAxis2 movementInput;

    private readonly ToggleButton placementModeToggle;

    private readonly Player player;
    private readonly InputAxis selectionAxis;
    private readonly PushButton selectTargetedButton;
    private readonly Button sprintButton;

    private Double timer;

    internal Input(Player player)
    {
        this.player = player;

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

    private Boolean IsCooldownOver => timer >= interactionCooldown;

    internal Boolean ShouldInteract => IsCooldownOver && interactOrPlaceButton.IsDown;

    internal Boolean ShouldDestroy => IsCooldownOver && destroyButton.IsDown;

    internal Boolean ShouldChangePlacementMode => placementModeToggle.Changed;

    internal Boolean ShouldSelectTargeted => selectTargetedButton.IsDown;

    internal Boolean IsInteractionBlocked => blockInteractButton.IsDown;

    internal Vector3d GetMovement(Single normalSpeed, Single sprintSpeed)
    {
        (Single x, Single z) = movementInput.Value;
        Vector3d movement = x * player.Forward + z * player.Right;

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

    internal Vector3d GetFlyingMovement(Double flyingSpeed, Double flyingSprintSpeed)
    {
        (Single x, Single z) = movementInput.Value;
        Single y = ShouldJump.ToInt() - ShouldCrouch.ToInt();

        Vector3d movement = x * player.LookingDirection + y * Vector3d.UnitY + z * player.CameraRight;

        if (movement != Vector3d.Zero)
            movement = sprintButton.IsDown
                ? movement.Normalized() * flyingSprintSpeed
                : movement.Normalized() * flyingSpeed;

        return movement;
    }
}
