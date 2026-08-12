// <copyright file="Keybinds.cs" company="VoxelGame">
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

using VoxelGame.Client.Inputs.Actions;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     All keybinds that are used in the game.
/// </summary>
public static class Keybinds
{
    /// <summary>
    ///     Get the keybind that toggles fullscreen mode.
    /// </summary>
    public static Keybind<ToggleButton> Fullscreen { get; } =
        Keybind.Define<ToggleButton>("fullscreen", Language.KeyFullscreen, VirtualKeys.F11, Layer.Application);

    /// <summary>
    ///     Get the keybind that toggles the user interface.
    /// </summary>
    public static Keybind<ToggleButton> UI { get; } =
        Keybind.Define<ToggleButton>("ui", Language.KeyToggleUI, VirtualKeys.F10, Layer.Application);

    /// <summary>
    ///     Get the keybind that takes a screenshot.
    /// </summary>
    public static Keybind<PushButton> Screenshot { get; } =
        Keybind.Define<PushButton>("screenshot", Language.KeyScreenshot, VirtualKeys.F12, Layer.Application);

    /// <summary>
    ///     Get the keybind that toggles the console.
    /// </summary>
    public static Keybind<ToggleButton> Console { get; } =
        Keybind.Define<ToggleButton>("console", Language.KeyConsole, VirtualKeys.F1, Layer.Application);

    /// <summary>
    ///     Get the keybind that toggles the debug view.
    /// </summary>
    public static Keybind<ToggleButton> DebugView { get; } =
        Keybind.Define<ToggleButton>("debug_view", Language.KeyDebugView, VirtualKeys.F2, Layer.Application);

    /// <summary>
    ///     Get the keybind that releases the pointer from the game.
    /// </summary>
    public static Keybind<PushButton> UnlockMouse { get; } =
        Keybind.Define<PushButton>("unlock_mouse", Language.KeyUnlockMouse, VirtualKeys.F3, Layer.Application);

    /// <summary>
    ///     Get the keybind for the current scene's escape action.
    /// </summary>
    public static Keybind<PushButton> Escape { get; } =
        Keybind.Define<PushButton>("escape", Language.KeyEscape, VirtualKeys.Escape, Layer.Application);

    /// <summary>
    ///     Get the keybind that moves the player forward.
    /// </summary>
    public static Keybind<SimpleButton> Forwards { get; } =
        Keybind.Define<SimpleButton>("forwards", Language.KeyForwards, VirtualKeys.W, Layer.Game);

    /// <summary>
    ///     Get the keybind that moves the player backward.
    /// </summary>
    public static Keybind<SimpleButton> Backwards { get; } =
        Keybind.Define<SimpleButton>("backwards", Language.KeyBackwards, VirtualKeys.S, Layer.Game);

    /// <summary>
    ///     Get the keybind that moves the player to the right.
    /// </summary>
    public static Keybind<SimpleButton> StrafeRight { get; } =
        Keybind.Define<SimpleButton>("strafe_right", Language.KeyStrafeRight, VirtualKeys.D, Layer.Game);

    /// <summary>
    ///     Get the keybind that moves the player to the left.
    /// </summary>
    public static Keybind<SimpleButton> StrafeLeft { get; } =
        Keybind.Define<SimpleButton>("strafe_left", Language.KeyStrafeLeft, VirtualKeys.A, Layer.Game);

    /// <summary>
    ///     Get the keybind that makes the player sprint.
    /// </summary>
    public static Keybind<SimpleButton> Sprint { get; } =
        Keybind.Define<SimpleButton>("sprint", Language.KeySprint, VirtualKeys.LeftShift, Layer.Game);

    /// <summary>
    ///     Get the keybind that makes the player jump.
    /// </summary>
    public static Keybind<SimpleButton> Jump { get; } =
        Keybind.Define<SimpleButton>("jump", Language.KeyJump, VirtualKeys.Space, Layer.Game);

    /// <summary>
    ///     Get the keybind that makes the player crouch.
    /// </summary>
    public static Keybind<SimpleButton> Crouch { get; } =
        Keybind.Define<SimpleButton>("crouch", Language.KeyCrouch, VirtualKeys.C, Layer.Game);

    /// <summary>
    ///     Get the keybind that interacts with a block or places the selected block.
    /// </summary>
    public static Keybind<SimpleButton> InteractOrPlace { get; } =
        Keybind.Define<SimpleButton>("interact_or_place", Language.KeyInteractOrPlace, VirtualKeys.RightButton, Layer.Game);

    /// <summary>
    ///     Get the keybind that destroys the targeted block.
    /// </summary>
    public static Keybind<SimpleButton> Destroy { get; } =
        Keybind.Define<SimpleButton>("destroy", Language.KeyDestroy, VirtualKeys.LeftButton, Layer.Game);

    /// <summary>
    ///     Get the keybind that forces interaction with the targeted block.
    /// </summary>
    public static Keybind<SimpleButton> BlockInteract { get; } =
        Keybind.Define<SimpleButton>("block_interact", Language.KeyForceInteract, VirtualKeys.LeftControl, Layer.Game);

    /// <summary>
    ///     Get the keybind that toggles placement mode.
    /// </summary>
    public static Keybind<ToggleButton> PlacementMode { get; } =
        Keybind.Define<ToggleButton>("placement_mode", Language.KeyPlacementMode, VirtualKeys.R, Layer.Game);

    /// <summary>
    ///     Get the keybind that selects the next block for placement.
    /// </summary>
    public static Keybind<PushButton> NextPlacement { get; } =
        Keybind.Define<PushButton>("select_next_placement", Language.KeyNextPlacement, VirtualKeys.Add, Layer.Game);

    /// <summary>
    ///     Get the keybind that selects the previous block for placement.
    /// </summary>
    public static Keybind<PushButton> PreviousPlacement { get; } =
        Keybind.Define<PushButton>("select_previous_placement", Language.KeyPreviousPlacement, VirtualKeys.Subtract, Layer.Game);

    /// <summary>
    ///     Get the keybind that selects the targeted block for placement.
    /// </summary>
    public static Keybind<PushButton> SelectTargeted { get; } =
        Keybind.Define<PushButton>("select_targeted", Language.KeySelectTargeted, VirtualKeys.MiddleButton, Layer.Game);
}
