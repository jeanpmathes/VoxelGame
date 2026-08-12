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
using VoxelGame.Client.Inputs.Actions;
using VoxelGame.Client.Inputs.Composite;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Utilities;
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

    private readonly PushButton nextSelectionButton;
    private readonly PushButton previousSelectionButton;
    private readonly PushButton selectTargetedButton;
    private readonly Button sprintButton;

    private Double timer;

    [Constructible]
    private PlayerInput(Player player) : base(player)
    {
        IInputActionProvider provider = player.InputActionProvider;

        Button forwardsButton = provider.Use(Keybinds.Forwards);
        Button backwardsButton = provider.Use(Keybinds.Backwards);
        Button strafeRightButton = provider.Use(Keybinds.StrafeRight);
        Button strafeLeftButton = provider.Use(Keybinds.StrafeLeft);

        movementInput = new InputAxis2(
            new InputAxis(forwardsButton, backwardsButton),
            new InputAxis(strafeRightButton, strafeLeftButton));

        sprintButton = provider.Use(Keybinds.Sprint);
        jumpButton = provider.Use(Keybinds.Jump);
        crouchButton = provider.Use(Keybinds.Crouch);

        interactOrPlaceButton = provider.Use(Keybinds.InteractOrPlace);
        destroyButton = provider.Use(Keybinds.Destroy);
        blockInteractButton = provider.Use(Keybinds.BlockInteract);

        placementModeToggle = provider.Use(Keybinds.PlacementMode);

        selectTargetedButton = provider.Use(Keybinds.SelectTargeted);

        nextSelectionButton = provider.Use(Keybinds.NextPlacement);
        previousSelectionButton = provider.Use(Keybinds.PreviousPlacement);
    }

    internal Boolean ShouldJump => jumpButton.IsActive;

    internal Boolean ShouldCrouch => crouchButton.IsActive;

    private Boolean IsCooldownOver => timer >= InteractionCooldown;

    internal Boolean ShouldInteract => IsCooldownOver && interactOrPlaceButton.IsActive;

    internal Boolean ShouldDestroy => IsCooldownOver && destroyButton.IsActive;

    internal Boolean ShouldChangePlacementMode => placementModeToggle.Changed;

    internal Boolean ShouldSelectTargeted => selectTargetedButton.Pushed;

    internal Boolean IsInteractionBlocked => blockInteractButton.IsActive;

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
            movement = sprintButton.IsActive
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
        return nextSelectionButton.PressCount - previousSelectionButton.PressCount;
    }

    #region DISPOSABLE

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (!disposing) return;

        movementInput.Dispose();

        sprintButton.Dispose();
        jumpButton.Dispose();
        crouchButton.Dispose();

        interactOrPlaceButton.Dispose();
        destroyButton.Dispose();
        blockInteractButton.Dispose();

        placementModeToggle.Dispose();

        selectTargetedButton.Dispose();
        nextSelectionButton.Dispose();
        previousSelectionButton.Dispose();
    }

    #endregion DISPOSABLE
}
