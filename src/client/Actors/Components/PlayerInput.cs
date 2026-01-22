// <copyright file="PlayerInput.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Inputs;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Utilities;
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
    ///     Get the movement decided by the user input for a given transform.
    /// </summary>
    /// <param name="transform">The transform to use for orientation.</param>
    /// <param name="normalSpeed">The factor to use for normal speed.</param>
    /// <param name="sprintSpeed">The factor to use for sprint speed.</param>
    /// <param name="allowFlying">Whether flying is allowed.</param>
    /// <returns>The movement vector.</returns>
    internal Vector3d GetMovement(Transform transform, Double normalSpeed, Double sprintSpeed, Boolean allowFlying)
    {
        (Single x, Single z) = movementInput.Value;
        Single y = (ShouldJump.ToInt() - ShouldCrouch.ToInt()) * allowFlying.ToInt();

        Vector3d movement = x * transform.Forward + z * transform.Right + y * Vector3d.UnitY;

        if (movement != Vector3d.Zero)
            movement = sprintButton.IsDown
                ? movement.Normalized() * sprintSpeed
                : movement.Normalized() * normalSpeed;

        return movement;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Delta delta)
    {
        timer += delta.Time;
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
