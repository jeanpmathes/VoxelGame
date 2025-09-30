// <copyright file="Command.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors.Components;

namespace VoxelGame.Client.Console;

/// <summary>
///     The base class of all callable commands. Commands with a zero-parameter constructor are automatically discovered.
///     Every command must have one or more Invoke methods.
/// </summary>
public abstract class Command : ICommand
{
    /// <summary>
    ///     Get the current command execution content. Is set when a command is invoked.
    /// </summary>
    protected Context Context { get; private set; } = null!;

    /// <inheritdoc />
    public abstract String Name { get; }

    /// <inheritdoc />
    public abstract String HelpText { get; }

    [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "Intentionally hidden.")]
    void ICommand.SetContext(Context context)
    {
        Context = context;
    }

    /// <summary>
    ///     Get a named position in the world.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <returns>The position, or null if it does not exist.</returns>
    protected Vector3d? GetNamedPosition(String name)
    {
        return name switch
        {
            "origin" => (0, 0, 0),
            "spawn" => Context.Player.World.SpawnPosition,
            "min-corner" => -Context.Player.World.Extents,
            "max-corner" => Context.Player.World.Extents,
            "self" => Context.Player.Body.Transform.Position,
            "prev-self" => GetPreviousPlayerPosition(),
            _ => null
        };
    }

    /// <summary>
    ///     Get the previous position of the player, e.g. before a teleportation.
    ///     This is only set by the command system, so normal movement does not change it.
    /// </summary>
    /// <returns>The previous position, or spawn position if not set.</returns>
    private Vector3d? GetPreviousPlayerPosition()
    {
        return Context.Player.GetComponent<PreviousPosition>()?.Value ?? Context.Player.World.SpawnPosition;
    }

    /// <summary>
    ///     Set the previous position of the player, e.g. before a teleportation.
    /// </summary>
    /// <param name="position">The position to set as previous.</param>
    public void SetPreviousPlayerPosition(Vector3d position)
    {
        var previousPosition = Context.Player.AddComponent<PreviousPosition>();
        previousPosition.Value = position;
    }
}

/// <summary>
///     An interface for all commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     Get the name of this command.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     Get the help text for this command.
    /// </summary>
    public String HelpText { get; }

    /// <summary>
    ///     Set the current command execution context.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    void SetContext(Context context);
}
