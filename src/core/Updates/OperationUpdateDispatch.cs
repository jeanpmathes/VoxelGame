// <copyright file="OperationUpdateDispatch.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.App;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;

namespace VoxelGame.Core.Updates;

/// <summary>
///     Stores and updates all operations.
/// </summary>
public class OperationUpdateDispatch : ApplicationComponent
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<OperationUpdateDispatch>();

    #endregion LOGGING

    private readonly Bag<Operation> operations = new(null!);

    /// <summary>
    ///     Create a new operation update dispatch instance.
    /// </summary>
    /// <param name="singleton">Whether to make this the singleton instance.</param>
    /// <param name="application">The application instance.</param>
    public OperationUpdateDispatch(Boolean singleton, Application application) : base(application)
    {
        if (!singleton) return;

        Debug.Assert(Instance == null);

        Instance = this;
    }

    /// <summary>
    ///     The name of this dispatch, used for debugging and logging purposes.
    /// </summary>
    public virtual String Name => "Operations";

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
        Instance = new OperationUpdateDispatch(singleton: true, Application.Instance);
    }

    /// <summary>
    ///     Perform an update.
    /// </summary>
    public void Update(Timer? timer)
    {
        using (logger.BeginTimedSubScoped(Name, timer))
        {
            operations.Apply(operation =>
            {
                operation.Update();

                return operation.IsRunning;
            });
        }
    }

    /// <summary>
    ///     Try cancelling all currently running operations.
    ///     Note that operations can ignore this.
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
        Application.ThrowIfNotOnMainThread(this);

        while (operations.Count > 0)
            Update(timer: null);
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

    #region DISPOSING

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposed)
            return;

        if (disposing) CompleteAll();

        disposed = true;
    }

    #endregion DISPOSING
}
