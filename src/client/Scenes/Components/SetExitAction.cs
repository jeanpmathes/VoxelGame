// <copyright file="SetExitAction.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Set the exit action on the <see cref="StartUserInterface" />.
/// </summary>
public class SetExitAction : SceneComponent, IConstructible<Scene, StartUserInterface, SetExitAction>
{
    private readonly StartUserInterface ui;

    private SetExitAction(Scene scene, StartUserInterface ui) : base(scene)
    {
        this.ui = ui;
    }

    /// <inheritdoc />
    public static SetExitAction Construct(Scene input1, StartUserInterface input2)
    {
        return new SetExitAction(input1, input2);
    }

    /// <inheritdoc />
    public override void OnLoad()
    {
        ui.SetExitAction(() => Subject.Client.Close());
    }
}
