// <copyright file="Command.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Client.Console
{
    /// <summary>
    ///     The base class of all callable commands. Commands with a zero-parameter constructor are automatically discovered.
    ///     Every command must have one or more Invoke methods.
    /// </summary>
    public abstract class Command : ICommand
    {
        /// <summary>
        ///     Get the current command execution content. Is set when a command is invoked.
        /// </summary>
        protected CommandContext Context { get; private set; } = null!;

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string HelpText { get; }

        void ICommand.SetContext(CommandContext context)
        {
            Context = context;
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
        public string Name { get; }

        /// <summary>
        ///     Get the help text for this command.
        /// </summary>
        public string HelpText { get; }

        /// <summary>
        ///     Set the current command execution context.
        /// </summary>
        /// <param name="context">The command execution context.</param>
        void SetContext(CommandContext context);
    }
}