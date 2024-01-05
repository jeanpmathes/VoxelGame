// <copyright file="PlayerInput.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Client.Application;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Input.Actions;
using VoxelGame.Support.Input.Composite;

namespace VoxelGame.Client.Entities.Players;

/// <summary>
///     Contains all player input.
/// </summary>
internal sealed class Input
{
    private readonly Button blockInteractButton;
    private readonly Button crouchButton;
    private readonly Button destroyButton;

    private readonly float interactionCooldown = 0.25f;

    private readonly Button interactOrPlaceButton;
    private readonly Button jumpButton;
    private readonly InputAxis2 movementInput;

    private readonly ToggleButton placementModeToggle;

    private readonly Player player;
    private readonly InputAxis selectionAxis;
    private readonly PushButton selectTargetedButton;
    private readonly Button sprintButton;

    private double timer;

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

    internal bool ShouldJump => jumpButton.IsDown;

    internal bool ShouldCrouch => crouchButton.IsDown;

    private bool IsCooldownOver => timer >= interactionCooldown;

    internal bool ShouldInteract => IsCooldownOver && interactOrPlaceButton.IsDown;

    internal bool ShouldDestroy => IsCooldownOver && destroyButton.IsDown;

    internal bool ShouldChangePlacementMode => placementModeToggle.Changed;

    internal bool ShouldSelectTargeted => selectTargetedButton.IsDown;

    internal bool IsInteractionBlocked => blockInteractButton.IsDown;

    internal Vector3d GetMovement(float normalSpeed, float sprintSpeed)
    {
        (float x, float z) = movementInput.Value;
        Vector3d movement = x * player.Forward + z * player.Right;

        if (movement != Vector3d.Zero)
            movement = sprintButton.IsDown
                ? movement.Normalized() * sprintSpeed
                : movement.Normalized() * normalSpeed;

        return movement;
    }

    internal void Update(double deltaTime)
    {
        timer += deltaTime;
    }

    internal void RegisterInteraction()
    {
        timer = 0;
    }

    internal int GetSelectionChange()
    {
        return Math.Sign(selectionAxis.Value);
    }

    internal Vector3d GetFlyingMovement(double flyingSpeed, double flyingSprintSpeed)
    {
        (float x, float z) = movementInput.Value;
        float y = ShouldJump.ToInt() - ShouldCrouch.ToInt();

        Vector3d movement = x * player.LookingDirection + y * Vector3d.UnitY + z * player.CameraRight;

        if (movement != Vector3d.Zero)
            movement = sprintButton.IsDown
                ? movement.Normalized() * flyingSprintSpeed
                : movement.Normalized() * flyingSpeed;

        return movement;
    }
}
