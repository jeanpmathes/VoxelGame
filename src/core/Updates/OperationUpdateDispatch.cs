// <copyright file="OperationUpdateDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    public OperationUpdateDispatch(Boolean singleton = false)
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
    /// Set up a mock instance for testing.
    /// It will override the singleton instance.
    /// </summary>
    public static void SetUpMockInstance()
    {
        Instance = new OperationUpdateDispatch();
    }

    /// <summary>
    ///     Perform an update.
    /// </summary>
    public void LogicUpdate()
    {
        operations.Apply(operation =>
        {
            operation.Update();

            return operation.IsRunning;
        });
    }

    /// <summary>
    ///     Try cancelling all currently running operations.
    ///     Note that operations can be un-cancelable and may thus ignore this.
    /// </summary>
    public void CancelAll()
    {
        operations.Apply(operation =>
        {
            operation.Cancel();

            return operation.IsRunning;
        });
    }

    /// <summary>
    ///     Wait for all operations to complete.
    ///     Must be called from the main thread.
    ///     This will block the current thread.
    /// </summary>
    public void CompleteAll()
    {
        ApplicationInformation.ThrowIfNotOnMainThread(this);

        while (operations.Count > 0)
            LogicUpdate();
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
