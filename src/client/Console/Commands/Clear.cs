// <copyright file="Clear.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Clear : Command
    {
        public override string Name => "clear";
        public override string HelpText => "Clear the console.";

        public void Invoke()
        {
            Context.Console.Clear();
        }
    }
}
