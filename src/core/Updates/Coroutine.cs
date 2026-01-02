// <copyright file="Coroutine.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A type of <see cref="Operation" /> that runs entirely on the main thread but can yield to wait.
///     By default, yielding <c>null</c> will wait for the next update cycle.
/// </summary>
public class Coroutine : IUpdateableProcess
{
    private readonly IEnumerator coroutine;

    private Coroutine(IEnumerable coroutine)
    {
        // ReSharper disable once GenericEnumeratorNotDisposed
        this.coroutine = coroutine.GetEnumerator();
    }

    /// <inheritdoc />
    public void Update()
    {
        IsRunning = coroutine.MoveNext();

        if (IsRunning)
        {
            // Nothing to do.
        }
        else if (coroutine is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <inheritdoc />
    public void Cancel()
    {
        IsRunning = false;

        if (coroutine is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <inheritdoc />
    public Boolean IsRunning { get; private set; }

    /// <summary>
    ///     Starts a new coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to start.</param>
    public static void Start(Func<IEnumerable> coroutine)
    {
        if (UpdateDispatch.Instance == null)
            throw new InvalidOperationException();

        UpdateDispatch.Instance.Add(new Coroutine(coroutine()));
    }
}
