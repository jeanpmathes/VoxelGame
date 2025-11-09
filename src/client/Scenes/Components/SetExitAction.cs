// <copyright file="SetExitAction.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Set the exit action on the <see cref="StartUserInterface" />.
/// </summary>
public partial class SetExitAction : SceneComponent
{
    private readonly StartUserInterface ui;

    [Constructible]
    private SetExitAction(Scene scene, StartUserInterface ui) : base(scene)
    {
        this.ui = ui;
    }

    /// <inheritdoc />
    public override void OnLoad()
    {
        ui.SetExitAction(() => Subject.Client.Close());
    }
}
