// <copyright file="Help.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Help : Command
    {
        private CommandInvoker commandInvoker;

        public Help(CommandInvoker invoker)
        {
            commandInvoker = invoker;
        }

        public override string Name => "help";
        public override string HelpText => "Provides help with using the commands.";
    }
}
