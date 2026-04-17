// <copyright file="ButtonBase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Commands;
using VoxelGame.GUI.Input;

namespace VoxelGame.GUI.Controls.Bases;

/// <summary>
///     Abstract base class for a button control, which is a control that can be clicked to perform an action.
/// </summary>
/// <typeparam name="TContent">The type of the content displayed on the button.</typeparam>
/// <typeparam name="TControl">The concrete type of the control.</typeparam>
public abstract class ButtonBase<TContent, TControl> : ContentControlBase<TContent, TControl> where TContent : class where TControl : ButtonBase<TContent, TControl>
{
    private readonly Slot<Boolean> isPressed;

    private ICommandExecution? currentExecution;

    /// <summary>
    ///     Creates a new instance of the <see cref="ButtonBase{TContent, TControl}" /> class.
    /// </summary>
    protected ButtonBase()
    {
        isPressed = new Slot<Boolean>(value: false, this);

        Command = Property.Create(this, default(ICommand<TContent>?));

        AddEnablementConstraint(Binding.To(Command)
            .Select(command => command?.CanExecute, defaultValue: false)
            .ApplyOr(Content, defaultOutput: false)
            .Compute(Enablements.FromBoolean));

        Content.ValueChanged += OnContentChanged;
    }

    #region PROPERTIES

    /// <summary>
    ///     The command to execute when the button is clicked.
    /// </summary>
    public Property<ICommand<TContent>?> Command { get; }

    #endregion PROPERTIES

    /// <summary>
    ///     Whether the button is currently pressed.
    ///     The button will be considered pressed
    /// </summary>
    public ReadOnlySlot<Boolean> IsPressed => isPressed;

    private Boolean IsExecuting => currentExecution != null && currentExecution.Status.GetValue() == Status.Running;

    /// <inheritdoc />
    public override void OnInput(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case PointerButtonEvent {Button: PointerButton.Left, IsDown: true} when !IsExecuting:
            {
                isPressed.SetValue(true);

                Context.PointerFocus.Set(this);

                break;
            }

            case KeyEvent {Key: Key.Enter, IsDown: true} when !IsExecuting:
            {
                isPressed.SetValue(true);

                ExecuteCommand();

                break;
            }

            case PointerButtonEvent {Button: PointerButton.Left, IsDown: false} pointerButtonEvent:
            {
                if (isPressed.GetValue())
                {
                    Boolean executing = false;

                    if (pointerButtonEvent.Hits(this))
                    {
                        executing = ExecuteCommand();
                    }

                    if (!executing)
                        isPressed.SetValue(false);
                }

                Context.PointerFocus.Unset(this);

                break;
            }

            default:
                return;
        }

        inputEvent.Handled = true;
    }

    private Boolean ExecuteCommand()
    {
        if (IsExecuting) return true;

        if (Content.GetValue() is not {} content) return false;

        currentExecution = Command.GetValue()?.Execute(content);

        if (currentExecution == null) return false;

        currentExecution.Status.ValueChanged += OnCurrentExecutionStatusChanged;

        // Execution might have already completed, e.g. when the command completes synchronously.
        if (currentExecution.Status.GetValue() != Status.Running)
        {
            OnCurrentExecutionStatusChanged(currentExecution.Status, EventArgs.Empty);
        }

        return true;
    }

    private void OnCurrentExecutionStatusChanged(Object? sender, EventArgs e)
    {
        if (currentExecution == null || currentExecution.Status.GetValue() == Status.Running) return;

        isPressed.SetValue(false);

        currentExecution.Status.ValueChanged -= OnCurrentExecutionStatusChanged;
        currentExecution.Dispose();
        currentExecution = null;
    }

    private void OnContentChanged(Object? sender, EventArgs e)
    {
        if (!IsExecuting) isPressed.SetValue(false);
    }
}
