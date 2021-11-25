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
        protected CommandContext Context { get; private set; } = null!;
        public abstract string Name { get; }
        public abstract string HelpText { get; }

        void ICommand.SetContext(CommandContext context)
        {
            Context = context;
        }
    }

    public interface ICommand
    {
        public string Name { get; }
        public string HelpText { get; }
        void SetContext(CommandContext context);
    }
}