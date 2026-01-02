// <copyright file="OperationUpdateDispatch.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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

    private void Update()
    {
        operations.Apply(operation =>
        {
            operation.Update();

            return operation.IsRunning;
        });
    }

    /// <summary>
    ///     Set up a mock instance for testing.
    ///     It will override the singleton instance.
    /// </summary>
    public static void SetUpMockInstance()
    {
        Instance = new OperationUpdateDispatch(singleton: true, Application.Instance);
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
            Update();
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

    /// <inheritdoc />
    public override void OnLogicUpdate(Double delta, Timer? timer)
    {
        using (logger.BeginTimedSubScoped(Name, timer))
        {
            Update();
        }
    }

    #region DISPOSABLE

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

    #endregion DISPOSABLE
}
