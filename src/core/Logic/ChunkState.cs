// <copyright file="ChunkState.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Abstract base class for chunk states.
/// </summary>
public abstract class ChunkState
{
    private (ChunkState state, bool isRequired)? next;
    private ChunkState? requested;

    /// <summary>
    ///     Get the chunk.
    /// </summary>
    protected Chunk Chunk { get; private set; } = null!;

    /// <summary>
    ///     Get the context.
    /// </summary>
    protected ChunkContext Context { get; private set; } = null!;

    /// <summary>
    ///     Get whether this chunk is active.
    /// </summary>
    public virtual bool IsActive => false;

    /// <summary>
    ///     Perform updates.
    /// </summary>
    protected abstract void OnUpdate();

    /// <summary>
    ///     Called when entering this state.
    /// </summary>
    public virtual void OnEnter() {}

    /// <summary>
    ///     Set the next state.
    /// </summary>
    /// <param name="state">The next state.</param>
    /// <param name="isRequired">
    ///     Whether the transition is required. If it is not required, a different state may be set
    ///     instead.
    /// </param>
    protected void SetNextState(ChunkState state, bool isRequired)
    {
        state.Chunk = Chunk;
        state.Context = Context;

        Debug.Assert(next == null);
        next = (state, isRequired);
    }

    /// <summary>
    ///     Set the next state.
    /// </summary>
    /// <typeparam name="T">The type of the next state.</typeparam>
    /// <param name="isRequired">
    ///     Whether the transition is required. If it is not required, a different state may be set
    ///     instead.
    /// </param>
    protected void SetNextState<T>(bool isRequired) where T : ChunkState, new()
    {
        SetNextState(new T(), isRequired);
    }

    /// <summary>
    ///     Signal that this chunk is now ready.
    /// </summary>
    protected void SetNextReady(bool isRequired)
    {
        SetNextState(Context.Activate(Chunk), isRequired);
    }

    /// <summary>
    ///     Set the next state to active. This transition is never required and can be understood as a "don't care"-transition.
    /// </summary>
    protected void SetNextActive()
    {
        if (IsActive) return;

        SetNextState<Chunk.Active>(isRequired: false);
    }

    /// <summary>
    ///     Request an external state transition on a chunk. Internal transitions are prioritized over external ones and
    ///     deactivation comes last.
    /// </summary>
    /// <param name="state">The next state.</param>
    public void RequestNextState(ChunkState state)
    {
        Debug.Assert(requested == null || requested.GetType() == state.GetType());

        state.Chunk = Chunk;
        state.Context = Context;

        requested = state;
    }

    /// <summary>
    ///     Request an external state transition on an active chunk. Internal transitions are prioritized over external ones
    ///     and deactivation comes last.
    /// </summary>
    /// <typeparam name="T">The type of the next state.</typeparam>
    public void RequestNextState<T>() where T : ChunkState, new()
    {
        RequestNextState(new T());
    }

    /// <summary>
    ///     Update the state.
    /// </summary>
    /// <returns>The new state.</returns>
    public ChunkState Update()
    {
        OnUpdate();

        (ChunkState internalNext, bool isInternalRequired) = next ?? (this, false);

        if (isInternalRequired)
        {
            next = null;

            return internalNext;
        }

        if (requested is {} externalNext)
        {
            requested = null;

            return externalNext;
        }

        if (Chunk.IsRequested) return internalNext;

        return CreateFinalState();
    }

    /// <summary>
    ///     Get the initial state.
    /// </summary>
    public static ChunkState CreateInitialState(Chunk chunk, ChunkContext context)
    {
        var state = new Chunk.Unloaded
        {
            Chunk = chunk,
            Context = context
        };

        return state;
    }

    private ChunkState CreateFinalState()
    {
        var state = new Chunk.Deactivating
        {
            Chunk = Chunk,
            Context = Context
        };

        return state;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return GetType().Name;
    }
}
