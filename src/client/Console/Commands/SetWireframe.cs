// <copyright file="SetWireframe.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using VoxelGame.Client.Rendering;

namespace VoxelGame.Client.Console.Commands
{
     #pragma warning disable CA1822

    /// <summary>
    ///     Allows to enable or disable wireframe rendering.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SetWireframe : Command
    {
        /// <inheritdoc />
        public override string Name => "set-wireframe";

        /// <inheritdoc />
        public override string HelpText => "Allows to enable or disable wireframe rendering.";

        /// <exclude />
        public void Invoke(bool enable)
        {
            Screen.SetWireframe(enable);
        }
    }
}