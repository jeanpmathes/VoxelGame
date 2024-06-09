// <copyright file="ChunkState.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Abstract base class for chunk states.
/// </summary>
public abstract partial class ChunkState
{
    private const Int32 NeighborWaitingTimeout = 10;

    private Guard? coreGuard;
    private Guard? extendedGuard;

    private Int32 currentWaitingTime;

    /// <summary>
    ///     Whether this state has acquired all required access. This can be true when the state is waiting on something.
    /// </summary>
    private Boolean isAccessSufficient;

    /// <summary>
    ///     Whether this state has acquired all required access and is not waiting on anything.
    /// </summary>
    private Boolean isEntered;

    private ((ChunkState state, Boolean isRequired)? transition, TransitionDescription description, Func<ChunkState?>? activator)? next;

    private ChunkState? previous;

    private Boolean released;

    private RequestQueue requests = null!;

    /// <summary>
    ///     Create a new chunk state.
    /// </summary>
    protected ChunkState() {}

    /// <summary>
    ///     Create a new chunk state with guards already acquired.
    /// </summary>
    /// <param name="core">The core guard.</param>
    /// <param name="extended">The extended guard.</param>
    protected ChunkState(Guard? core, Guard? extended)
    {
        coreGuard = core;
        extendedGuard = extended;
    }

    /// <summary>
    ///     Whether this state intends to perform a ready-transition.
    /// </summary>
    public virtual Boolean IsIntendingToGetReady => false;

    /// <summary>
    ///     Whether this state wants to delay entering when neighbors intend to activate.
    /// </summary>
    protected virtual Boolean WaitOnNeighbors => false;

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
    public Boolean IsActive => isEntered && CoreAccess == Access.Write && ExtendedAccess == Access.Write && AllowSharingAccess;

    /// <summary>
    ///     Whether this state allows sharing its access during one update.
    /// </summary>
    protected virtual Boolean AllowSharingAccess => false;

    /// <summary>
    ///     Whether this state allows that its access is stolen.
    ///     A state must hold write-access to its core and extended data to allow stealing.
    ///     If a state performs work on another thread, it cannot allow stealing.
    /// </summary>
    protected virtual Boolean AllowStealing => false;

    /// <summary>
    ///     The required access level of this state to core chunk resources.
    /// </summary>
    protected abstract Access CoreAccess { get; }

    /// <summary>
    ///     The required access level of this state to extended chunk resources.
    /// </summary>
    protected abstract Access ExtendedAccess { get; }

    /// <summary>
    ///     Whether it is currently possible to steal access from this state.
    /// </summary>
    public Boolean CanStealAccess => AllowStealing && isAccessSufficient;

    /// <summary>
    ///     Check if two states are duplicates, if yes, return the state that should be kept.
    /// </summary>
    /// <param name="a">The first state.</param>
    /// <param name="b">The second state.</param>
    /// <returns>The state that should be kept, or null if both states should be kept.</returns>
    private static ChunkState? ResolveDuplicate(ChunkState a, ChunkState b)
    {
        return IsSameStateType(a, b) ? a.ResolveDuplicate(b) : null;
    }

    /// <summary>
    ///     Resolves a duplicate state.
    /// </summary>
    /// <param name="other">The other state.</param>
    /// <returns>The state that should be kept, or null if both states should be kept.</returns>
    protected virtual ChunkState ResolveDuplicate(ChunkState other)
    {
        return this;
    }

    private static Boolean IsSameStateType(ChunkState a, ChunkState b)
    {
        return a.GetType() == b.GetType();
    }

    /// <summary>
    ///     Perform updates.
    ///     This is where the state logic, e.g. the work associated with the state as well as transitions, is performed.
    /// </summary>
    protected abstract void OnUpdate();

    private void Enter()
    {
        if (previous != null) LogChunkStateChange(logger, Chunk.Position, previous, this);

        isEntered = true;
        OnEnter();
    }

    /// <summary>
    ///     Called directly after entering this state.
    /// </summary>
    protected virtual void OnEnter() {}

    /// <summary>
    ///     Called directly before leaving this state.
    /// </summary>
    protected virtual void OnExit() {}

    /// <summary>
    ///     Set the next state. The transition is required, except if certain flags are set in the description.
    /// </summary>
    /// <param name="state">The next state.</param>
    /// <param name="description">A description of the transition.</param>
    protected void SetNextState(ChunkState state, TransitionDescription description = new())
    {
        state.Chunk = Chunk;
        state.Context = Context;

        Debug.Assert(next == null);
        next = ((state, isRequired: true), description, null);
    }

    /// <summary>
    ///     Set the next state. The transition is required, except if certain flags are set in the description.
    /// </summary>
    /// <typeparam name="T">The type of the next state.</typeparam>
    /// <param name="description">A description of the transition.</param>
    protected void SetNextState<T>(TransitionDescription description = new()) where T : ChunkState, new()
    {
        SetNextState(new T(), description);
    }

    private void SetNextState(Func<ChunkState?> activator, TransitionDescription description = new())
    {
        Debug.Assert(next == null);
        next = (null, description, activator);
    }

    /// <summary>
    ///     Signal that this chunk is now ready. The transition is required, except if certain flags are set in the
    ///     description.
    ///     This is a strong activation.
    /// </summary>
    protected void SetNextReady(TransitionDescription description = new())
    {
        ReleaseResources();
        SetNextState(() => Context.ActivateStrongly(Chunk), description);
    }

    /// <summary>
    ///     Set the next state to active. This transition is never required and can be understood as a "don't care"-transition.
    ///     This is a weak activation.
    /// </summary>
    protected void SetNextActive()
    {
        ReleaseResources();
        SetNextState(() => Context.ActivateWeakly(Chunk));
    }

    /// <summary>
    ///     Indicate that this state allows to transition if there is a request.
    /// </summary>
    protected void AllowTransition()
    {
        next = ((this, isRequired: false), new TransitionDescription(), null);
    }

    /// <summary>
    ///     Request an external state transition on a chunk. Internal transitions are prioritized over external ones and
    ///     deactivation comes last.
    /// </summary>
    /// <param name="state">The next state.</param>
    /// <param name="description">The request description.</param>
    public void RequestNextState(ChunkState state, RequestDescription description = new())
    {
        state.Chunk = Chunk;
        state.Context = Context;

        requests.Enqueue(this, state, description);
    }

    /// <summary>
    ///     Request an external state transition on an active chunk. Internal transitions are prioritized over external ones
    ///     and deactivation comes last.
    /// </summary>
    /// <typeparam name="T">The type of the next state.</typeparam>
    public void RequestNextState<T>(RequestDescription description = new()) where T : ChunkState, new()
    {
        RequestNextState(new T(), description);
    }

    /// <summary>
    ///     Update the state.
    /// </summary>
    /// <returns>The new state.</returns>
    private ChunkState Update()
    {
        if (!released)
        {
            isAccessSufficient = EnsureRequiredAccess();

            if (!isAccessSufficient) return this;

            Debug.Assert((coreGuard == null && CoreAccess == Access.None) || (coreGuard != null && Chunk.IsCoreHeldBy(coreGuard, CoreAccess)));
            Debug.Assert((extendedGuard == null && ExtendedAccess == Access.None) || (extendedGuard != null && Chunk.IsExtendedHeldBy(extendedGuard, ExtendedAccess)));
        }

        if (IsWaitingOnNeighbors()) return this;

        if (!isEntered) Enter();

        if (!released) OnUpdate();

        ChunkState nextState = DetermineNextState();

        if (ReferenceEquals(this, nextState)) return nextState;

        OnExit();
        ReleaseResources();

        return nextState;
    }

    private Boolean IsWaitingOnNeighbors()
    {
        if (isEntered) return false;
        if (!WaitOnNeighbors) return false;

        currentWaitingTime++;

        if (currentWaitingTime >= NeighborWaitingTimeout) return false;

        foreach (BlockSide side in BlockSide.All.Sides())
            if (Chunk.World.TryGetChunk(side.Offset(Chunk.Position), out Chunk? neighbor) && neighbor.IsIntendingToGetReady)
                return true;

        return false;
    }

    private Boolean EnsureRequiredAccess()
    {
        var isSufficient = true;

        if (CoreAccess != Access.None && coreGuard == null)
        {
            coreGuard = Chunk.AcquireCore(CoreAccess);
            isSufficient &= coreGuard != null;
        }

        if (ExtendedAccess != Access.None && extendedGuard == null)
        {
            extendedGuard = Chunk.AcquireExtended(ExtendedAccess);
            isSufficient &= extendedGuard != null;
        }

        return isSufficient;
    }

    /// <summary>
    ///     Release all held resources. A state will not be updated when released, and must transition until the next update.
    /// </summary>
    private void ReleaseResources()
    {
        coreGuard?.Dispose();
        extendedGuard?.Dispose();

        coreGuard = null;
        extendedGuard = null;

        released = true;
        isAccessSufficient = false;
    }

    /// <summary>
    ///     Deactivate the chunk.
    /// </summary>
    protected void Deactivate()
    {
        ReleaseResources();
        Context.Deactivate(Chunk);
    }

    private Boolean PerformActivation()
    {
        Debug.Assert(next != null);

        if (next.Value.transition != null) return true;

        Debug.Assert(next.Value.activator != null);

        if (!Chunk.CanAcquireCore(Access.Write) || !Chunk.CanAcquireExtended(Access.Write)) return false;

        ChunkState? activatedNext = next.Value.activator();
        Boolean isRequired = activatedNext != null;
        activatedNext ??= new Chunk.Active();

        activatedNext.Chunk = Chunk;
        activatedNext.Context = Context;

        next = ((activatedNext, isRequired), next.Value.description, null);

        return true;
    }

    #pragma warning disable S1871 // Readability.
    private ChunkState DetermineNextState()
    {
        if (next == null) return this;

        ChunkState nextState;
        ChunkState? requestedState;

        if (!PerformActivation()) return this;

        Debug.Assert(next.Value.transition != null);

        if (next.Value.description.PrioritizeDeactivation && !Chunk.IsRequested) nextState = requests.Dequeue(this, isLooping: false, isDeactivating: true) ?? CreateFinalState();
        else if (next.Value.description.PrioritizeLoop && (requestedState = requests.Dequeue(this, isLooping: true, isDeactivating: false)) != null) nextState = requestedState;
        else if (next.Value.transition.Value.isRequired) nextState = next.Value.transition.Value.state;
        else if ((requestedState = requests.Dequeue(this, isLooping: false, isDeactivating: false)) != null) nextState = requestedState;
        else if (!Chunk.IsRequested) nextState = CreateFinalState();
        else nextState = next.Value.transition.Value.state;

        if (nextState != next.Value.transition.Value.state)
            next.Value.description.Cleanup?.Invoke();

        next = null;

        return nextState;
    }

    #pragma warning restore S1871

    /// <summary>
    ///     Update the state of a chunk. This can change the state.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    /// <param name="tracker">A tracker to profile state transitions.</param>
    public static void Update(ref ChunkState state, StateTracker tracker)
    {
        const Int32 maxTransitions = 3;

        var count = 0;

        while (count < maxTransitions)
        {
            ChunkState previousState = state;

            state = state.Update();

            state.previous ??= previousState;
            state.requests = previousState.requests;

            if (ReferenceEquals(previousState, state)) break;

            tracker.Transition(previousState, state);

            count++;
        }
    }

    /// <summary>
    ///     Initialize the state of a chunk.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    /// <param name="chunk">The chunk of which the state is initialized.</param>
    /// <param name="context">The current context.</param>
    public static void Initialize(out ChunkState state, Chunk chunk, ChunkContext context)
    {
        state = new Chunk.Unloaded
        {
            Chunk = chunk,
            Context = context,
            requests = new RequestQueue()
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

    /// <summary>
    ///     Try to steal access from the current state.
    ///     If access is stolen, the state is changed to the <see cref="Chunk.Used" /> state.
    ///     Access can be stolen if the chunk is in a state that allows stealing and the state holds write-access to all its
    ///     resources.
    ///     A use-case of this is when threaded work on one chunk requires access to the resources of another chunk.
    /// </summary>
    /// <param name="state">The current state. Will be set to <see cref="Chunk.Used" /> if access is stolen.</param>
    /// <param name="tracker">A tracker to profile state transitions.</param>
    /// <returns>Guards holding write-access to all resources, or null if access could not be stolen.</returns>
    public static (Guard core, Guard extended)? TryStealAccess(ref ChunkState state, StateTracker tracker)
    {
        Throw.IfNotOnMainThread(state.Chunk);

        if (!state.CanStealAccess) return null;

        state.OnExit();

        Debug.Assert(state is {CoreAccess: Access.Write, coreGuard: not null});
        Debug.Assert(state is {ExtendedAccess: Access.Write, extendedGuard: not null});

        Guard? core = state.coreGuard;
        Guard? extended = state.extendedGuard;

        state.coreGuard = null;
        state.extendedGuard = null;

        ChunkState previousState = state;

        state = new Chunk.Used(previousState.IsActive)
        {
            Chunk = state.Chunk,
            Context = state.Context
        };

        state.previous = previousState;
        state.requests = previousState.requests;

        tracker.Transition(previousState, state);

        return (core, extended);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return GetType().Name;
    }

    /// <summary>
    ///     Describes how to take the transition to a state.
    /// </summary>
    protected record struct TransitionDescription
    {
        /// <summary>
        ///     Cleanup action to perform it the transition is not taken. Only required if the transition itself is not strictly
        ///     required.
        /// </summary>
        public Action? Cleanup { get; init; }

        /// <summary>
        ///     Whether to prioritize looping transitions (to same type) over this transition, even if this transition is
        ///     required.
        /// </summary>
        public Boolean PrioritizeLoop { get; init; }

        /// <summary>
        ///     Whether to prioritize chunk deactivation over this transition, even if this transition is required.
        /// </summary>
        public Boolean PrioritizeDeactivation { get; init; }
    }

    /// <summary>
    ///     Describes a transition request.
    /// </summary>
    public record struct RequestDescription
    {
        /// <summary>
        ///     Whether to skip this request when deactivating the chunk.
        /// </summary>
        public Boolean AllowSkipOnDeactivation { get; init; }
    }

    /// <summary>
    ///     Holds all transition requests.
    /// </summary>
    private sealed class RequestQueue
    {
        private readonly List<(ChunkState state, RequestDescription description)> requests = [];

        /// <summary>
        ///     Enqueue a new request. If the same state type is already requested, the request is ignored, unless the correct
        ///     flags are set.
        /// </summary>
        /// <param name="current">The current state.</param>
        /// <param name="state">The state to request.</param>
        /// <param name="description">The description of the request.</param>
        public void Enqueue(ChunkState current, ChunkState state, RequestDescription description)
        {
            ChunkState? keep;

            if (current.next is {transition: {state: {} next, isRequired: true}})
            {
                keep = ResolveDuplicate(next, state);

                if (keep == next) return;
            }

            if (!current.isEntered)
            {
                keep = ResolveDuplicate(current, state);

                if (keep == current) return;
            }

            for (var request = 0; request < requests.Count; request++)
            {
                keep = ResolveDuplicate(requests[request].state, state);

                if (keep == null) continue;

                if (keep == state) requests[request] = (state, description);

                return;
            }

            requests.Add((state, description));
        }

        /// <summary>
        ///     Dequeue the first request.
        /// </summary>
        /// <param name="current">The current state.</param>
        /// <param name="isLooping">Whether the current state prioritizes looping transitions.</param>
        /// <param name="isDeactivating">
        ///     Whether the chunk is deactivating. This will filter out all requests that are not required
        ///     before deactivation.
        /// </param>
        /// <returns>The first request, or null if no request is available.</returns>
        public ChunkState? Dequeue(ChunkState current, Boolean isLooping, Boolean isDeactivating)
        {
            Int32 target = -1;

            for (var index = 0; index < requests.Count; index++)
            {
                (ChunkState state, RequestDescription description) = requests[index];

                if (isDeactivating && description.AllowSkipOnDeactivation) continue;
                if (isLooping && !IsSameStateType(current, state)) continue;

                target = index;

                break;
            }

            if (target == -1) return null;

            (ChunkState state, RequestDescription description) request = requests[target];
            requests.RemoveAt(target);

            return request.state;
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkState>();

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Chunk {Position} state changed from {PreviousState} to {State}")]
    private static partial void LogChunkStateChange(ILogger logger, ChunkPosition position, ChunkState previousState, ChunkState state);

    #endregion LOGGING
}
