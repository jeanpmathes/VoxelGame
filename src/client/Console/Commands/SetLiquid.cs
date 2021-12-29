// <copyright file="SetLiquid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    /// <summary>
    ///     Sets the liquid at the target position. Can cause invalid liquid state.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SetLiquid : Command
    {
        /// <inheritdoc />
        public override string Name => "set-liquid";

        /// <inheritdoc />
        public override string HelpText => "Sets the liquid at the target position. Can cause invalid liquid state.";

        /// <exclude />
        public void Invoke(string namedID, int level, int x, int y, int z)
        {
            Set(namedID, level, (x, y, z));
        }

        /// <exclude />
        public void Invoke(string namedID, int level)
        {
            Set(namedID, level, Context.Player.TargetPosition);
        }

        private void Set(string namedID, int levelData, Vector3i position)
        {
            Liquid? liquid = Liquid.TranslateNamedID(namedID);

            if (liquid == null)
            {
                Context.Console.WriteError("Cannot find liquid.");

                return;
            }

            var level = (LiquidLevel) levelData;

            if (level is < LiquidLevel.One or > LiquidLevel.Eight)
            {
                Context.Console.WriteError("Invalid level.");

                return;
            }

            Context.Player.World.SetLiquid(liquid.AsInstance(level, isStatic: true), position);
        }
    }
}