// <copyright file="Clear.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    /// <summary>
    ///     Clears the console.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Clear : Command
    {
        /// <inheritdoc />
        public override string Name => "clear";

        /// <inheritdoc />
        public override string HelpText => "Clear the console.";

        /// <exclude />
        public void Invoke()
        {
            Context.Console.Clear();
        }
    }
}
