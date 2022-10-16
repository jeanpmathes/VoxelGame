// <copyright file="ChunkState.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Abstract base class for chunk states.
/// </summary>
public abstract class ChunkState
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkState>();

    private Guard? coreGuard;
    private Guard? extendedGuard;

    /// <summary>
    ///     Whether this state has acquired all required access and is therefore completely entered.
    /// </summary>
    private bool isEntered;

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
    public bool IsActive => CoreAccess == Access.Write && ExtendedAccess == Access.Write && AllowSharingAccess;

    /// <summary>
    ///     Whether this state allows sharing its access during one update.
    /// </summary>
    protected virtual bool AllowSharingAccess => false;

    /// <summary>
    ///     Whether this state is the final state.
    /// </summary>
    protected virtual bool IsFinal => false;

    /// <summary>
    ///     The required access level of this state to core chunk resources.
    /// </summary>
    protected abstract Access CoreAccess { get; }

    /// <summary>
    ///     The required access level of this state to extended chunk resources.
    /// </summary>
    protected abstract Access ExtendedAccess { get; }

    /// <summary>
    ///     Perform updates.
    /// </summary>
    protected abstract void OnUpdate();

    private void Enter()
    {
        if (IsFinal)
        {
            ReleaseResources();
            Context.Deactivate(Chunk);
        }
        else
        {
            OnEnter();
        }
    }

    /// <summary>
    ///     Called when this state is entered.
    /// </summary>
    protected virtual void OnEnter() {}

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
        SetNextState(Context.ActivateStrongly(Chunk), isRequired);
    }

    /// <summary>
    ///     Set the next state to active. This transition is never required and can be understood as a "don't care"-transition.
    /// </summary>
    protected void SetNextActive()
    {
        if (IsActive) next = (this, false);
        else SetNextState<Chunk.Active>(isRequired: false);
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
    private ChunkState Update()
    {
        bool isAccessSufficient = EnsureRequiredAccess();

        if (!isAccessSufficient) return this;

        if (!isEntered) Enter();
        isEntered = true;

        // If the chunk is deactivating, we do not want to perform any updates.
        if (IsFinal) return this;

        OnUpdate();

        ChunkState nextState = DetermineNextState();

        if (!ReferenceEquals(this, nextState)) ReleaseResources();

        return nextState;
    }

    private bool EnsureRequiredAccess()
    {
        var isAccessSufficient = true;

        if (CoreAccess != Access.None && coreGuard == null)
        {
            coreGuard = Chunk.CoreResource.TryAcquire(CoreAccess);
            isAccessSufficient &= coreGuard != null;
        }

        if (ExtendedAccess != Access.None && extendedGuard == null)
        {
            extendedGuard = Chunk.ExtendedResource.TryAcquire(ExtendedAccess);
            isAccessSufficient &= extendedGuard != null;
        }

        if (isEntered && !isAccessSufficient) Debug.Fail("Access was lost during state update.");

        return isAccessSufficient;
    }

    private void ReleaseResources()
    {
        coreGuard?.Dispose();
        extendedGuard?.Dispose();
    }

    private ChunkState DetermineNextState()
    {
        if (next == null) return this;

        ChunkState nextState;

        if (next.Value.isRequired)
        {
            nextState = next.Value.state;
        }
        else if (requested != null)
        {
            nextState = requested;
            requested = null;
        }
        else if (!Chunk.IsRequested)
        {
            nextState = CreateFinalState();
        }
        else
        {
            nextState = next.Value.state;
        }

        next = null;

        return nextState;
    }

    /// <summary>
    /// Update the state of a chunk.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    public static void Update(ref ChunkState state)
    {
        ChunkState previousState = state;
        state = previousState.Update();

        if (ReferenceEquals(previousState, state)) return;

        logger.LogDebug(Events.ChunkOperation, "Chunk {Position} state changed from {PreviousState} to {State}", state.Chunk.Position, previousState, state);
    }

    /// <summary>
    ///     Initialize the state of a chunk.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    /// <param name="chunk">The chunk.</param>
    /// <param name="context">The context.</param>
    public static void Initialize(out ChunkState state, Chunk chunk, ChunkContext context)
    {
        state = new Chunk.Unloaded
        {
            Chunk = chunk,
            Context = context
        };
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
