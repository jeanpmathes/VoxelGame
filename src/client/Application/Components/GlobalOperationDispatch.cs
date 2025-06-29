// <copyright file="GlobalOperationDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Updates;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Application.Components;

/// <summary>
///     Specific variant of <see cref="OperationUpdateDispatch" /> for global operations.
///     This dispatch is used for operations that are not tied to a specific scene.
///     Using this dispatch is necessary when operations should continue even when the scene changes.
///     Otherwise, using the default dispatch is recommended.
/// </summary>
public class GlobalOperationDispatch(Core.App.Application application) : OperationUpdateDispatch(singleton: false, application), IConstructible<Core.App.Application, GlobalOperationDispatch>
{
    /// <inheritdoc />
    public override String Name => "Global Operations";

    /// <inheritdoc />
    public static GlobalOperationDispatch Construct(Core.App.Application input)
    {
        return new GlobalOperationDispatch(input);
    }
}
