// <copyright file="SceneOperationDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Updates;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Specific variant of <see cref="OperationUpdateDispatch" /> for scene operations.
///     Scene operations are completed or canceled when the scene is changed.
/// </summary>
public partial class SceneOperationDispatch : OperationUpdateDispatch
{
    [Constructible]
    private SceneOperationDispatch(Core.App.Application application) : base(singleton: true, application) {}

    /// <inheritdoc />
    public override String Name => "Scene Operations";
}
