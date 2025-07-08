// <copyright file="SceneOperationDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Updates;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Specific variant of <see cref="OperationUpdateDispatch" /> for scene operations.
///     Scene operations are completed or canceled when the scene is changed.
/// </summary>
public class SceneOperationDispatch : OperationUpdateDispatch, IConstructible<Core.App.Application, SceneOperationDispatch>
{
    private SceneOperationDispatch(Core.App.Application application) : base(singleton: true, application) {
    }

    /// <inheritdoc />
    public override String Name => "Scene Operations";

    /// <inheritdoc />
    public static SceneOperationDispatch Construct(Core.App.Application input)
    {
        return new SceneOperationDispatch(input);
    }
}
