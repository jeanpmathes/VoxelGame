// <copyright file="HideWorldOnTermination.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Hides all sections in the world when the world is terminated.
///     This prevents rendering of the no longer needed sections.
/// </summary>
public partial class HideWorldOnTermination : WorldComponent
{
    [Constructible]
    private HideWorldOnTermination(Core.Logic.World subject) : base(subject) {}

    /// <inheritdoc />
    public override void OnTerminate()
    {
        foreach (Chunk chunk in Subject.Chunks.All)
            chunk.Cast().HideAllSections();
    }
}
