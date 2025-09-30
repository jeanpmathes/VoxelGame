// <copyright file="HideWorldOnTermination.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Hides all sections in the world when the world is terminated.
///     This prevents rendering of the no longer needed sections.
/// </summary>
public class HideWorldOnTermination : WorldComponent, IConstructible<Core.Logic.World, HideWorldOnTermination>
{
    private HideWorldOnTermination(Core.Logic.World subject) : base(subject) {}

    /// <inheritdoc />
    public static HideWorldOnTermination Construct(Core.Logic.World input)
    {
        return new HideWorldOnTermination(input);
    }

    /// <inheritdoc />
    public override void OnTerminate()
    {
        foreach (Chunk chunk in Subject.Chunks.All)
            chunk.Cast().HideAllSections();
    }
}
