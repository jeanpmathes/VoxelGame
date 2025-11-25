// <copyright file = "SetRenderDebugMode.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
/// Set varying debug modes to visualize internal aspects of the rendering process.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetRenderDebugMode : Command
{
    /// <inheritdoc />
    public override String Name => "set-render-debug-mode";

    /// <inheritdoc />
    public override String HelpText => "Sets render debug modes to visualize internal aspects of the rendering process.";

    /// <exclude />
    public void Invoke(String what, Boolean enable)
    {
        switch (what)
        {
            case "wireframe":
                Visuals.Graphics.Instance.SetWireframe(enable);
                break;

            case "sampling":
                Visuals.Graphics.Instance.SetSamplingDisplay(enable);
                break;

            case "lod":
                Visuals.Graphics.Instance.SetLevelOfDetailDisplay(enable);
                break;
            
            default:
                Context.Output.WriteError($"Unknown render debug mode '{what}'.");
                break;
        }
    }
}
