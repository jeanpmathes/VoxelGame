// <copyright file="PlayerInput.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Inputs;
using VoxelGame.Core.Actors;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Graphics.Input.Composite;
using VoxelGame.Toolkit;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Defines all player input actions and provides methods to easily retrieve information about the player's input.
/// </summary>
public sealed partial class PlayerInput : ActorComponent
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

    [Constructible]
    private PlayerInput(Player player) : base(player)
    {
        KeybindManager keybinds = player.Input.Keybinds;

        Button forwardsButton = keybinds.GetButton(keybinds.Forwards);
        Button backwardsButton = keybinds.GetButton(keybinds.Backwards);
        Button strafeRightButton = keybinds.GetButton(keybinds.StrafeRight);
        Button strafeLeftButton = keybinds.GetButton(keybinds.StrafeLeft);

        movementInput = new InputAxis2(
            new InputAxis(forwardsButton, backwardsButton),
            new InputAxis(strafeRightButton, strafeLeftButton));

        sprintButton = keybinds.GetButton(keybinds.Sprint);
        jumpButton = keybinds.GetButton(keybinds.Jump);
        crouchButton = keybinds.GetButton(keybinds.Crouch);

        interactOrPlaceButton = keybinds.GetButton(keybinds.InteractOrPlace);
        destroyButton = keybinds.GetButton(keybinds.Destroy);
        blockInteractButton = keybinds.GetButton(keybinds.BlockInteract);

        placementModeToggle = keybinds.GetToggle(keybinds.PlacementMode);
        placementModeToggle.Clear();

        selectTargetedButton = keybinds.GetPushButton(keybinds.SelectTargeted);

        Button nextButton = keybinds.GetPushButton(keybinds.NextPlacement);
        Button previousButton = keybinds.GetPushButton(keybinds.PreviousPlacement);
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

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
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
