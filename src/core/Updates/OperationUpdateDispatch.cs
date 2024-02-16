// <copyright file="OperationUpdateDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Stores and updates all operations.
/// </summary>
public class OperationUpdateDispatch
{
    private readonly Bag<Operation> operations = new(null!);

    /// <summary>
    ///     Create a new operation update dispatch instance.
    /// </summary>
    /// <param name="singleton">Whether to make this the singleton instance.</param>
    public OperationUpdateDispatch(bool singleton = false)
    {
        if (!singleton) return;

        Debug.Assert(Instance == null);

        Instance = this;
    }

    /// <summary>
    ///     The singleton instance of the operation update dispatch.
    /// </summary>
    public static OperationUpdateDispatch? Instance { get; private set; }

    /// <summary>
    ///     Perform an update.
    /// </summary>
    public void Update()
    {
        operations.Apply(operation =>
        {
            operation.Update();

            return operation.IsRunning;
        });
    }

    /// <summary>
    ///     Add an operation. This will start the operation.
    /// </summary>
    /// <param name="operation">The operation to add.</param>
    public void Add(Operation operation)
    {
        operation.Start();

        operations.Add(operation);
    }
}
