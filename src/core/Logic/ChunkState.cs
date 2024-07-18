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
    private const Int32 DelayTimeout = 40;

    private Guard? coreGuard;
    private Guard? extendedGuard;

    private Int32 currentDelayTime;

    /// <summary>
    ///     Whether this state has acquired all required access. This can be true when the state is waiting on something.
    /// </summary>
    private Boolean isAccessSufficient;

    /// <summary>
    ///     Whether this state has acquired all required access and is not waiting on anything.
    /// </summary>
    private Boolean isEntered;

    /// <summary>
    ///     Whether this state has exited and released all resources.
    /// </summary>
    private Boolean released;

    private NextStateTarget? next;
    private ChunkState? previous;
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
        requests.RemoveDuplicates(this);

        if (previous != null) LogChunkStateChange(logger, Chunk.Position, previous, this);

        isEntered = true;
        OnEnter();
    }

    /// <summary>
    ///     Allows a state to delay being entered.
    ///     This can be useful if the short-term future might have more favorable conditions.
    ///     Maximum delay time is defined by <see cref="DelayTimeout" />.
    /// </summary>
    /// <returns><c>true</c> if the state should be delayed, <c>false</c> otherwise.</returns>
    protected virtual Boolean DelayEnter()
    {
        return false;
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
    /// Perform cleanup in the case that the state is not transitioned to.
    /// </summary>
    protected virtual void Cleanup() {}

    /// <summary>
    ///     Set the next state.
    /// </summary>
    /// <param name="state">The next state.</param>
    /// <param name="description">A description of the transition.</param>
    /// <param name="isRequired">Whether the transition is required.</param>
    protected void SetNextState(ChunkState state, TransitionDescription description = new(), Boolean isRequired = true)
    {
        state.Chunk = Chunk;
        state.Context = Context;

        Debug.Assert(next == null);
        next = NextStateTarget.CreateDirectTransition(state, isRequired, description);
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

    /// <summary>
    ///     Try to set the next state according the world chunk activation rules.
    ///     If the rules determine that the chunk should enter a different state, that state is set as the next state.
    ///     In that case, the method returns <c>true</c>, otherwise <c>false</c>.
    ///     The strong activation rule will be used, which is meant for chunks that have not been activated yet.
    /// </summary>
    protected Boolean TrySettingNextReady(TransitionDescription description = new())
    {
        ChunkState? state = Context.ActivateStrongly(Chunk);

        Debug.Assert(state is not Chunk.Hidden);

        if (state == null && this is not Chunk.Hidden) state = new Chunk.Hidden();

        if (state == null) return false;

        ReleaseResources();
        SetNextState(state, description, state is not Chunk.Active);

        return true;
    }

    /// <summary>
    ///     Try to set the next state according the world chunk activation rules.
    ///     If the rules determine that the chunk should enter a different state, that state is set as the next state.
    ///     In that case, the method returns <c>true</c>, otherwise <c>false</c>.
    ///     The weak activation rule will be used, which is meant for chunks that have already been activated before.
    /// </summary>
    protected Boolean TrySettingNextActive()
    {
        ChunkState? state = Context.ActivateWeakly(Chunk);

        Debug.Assert(state is not Chunk.Hidden);

        if (state == null && this is not Chunk.Hidden) state = new Chunk.Hidden();

        if (state == null) return false;

        ReleaseResources();
        SetNextState(state, isRequired: state is not Chunk.Active);

        return true;
    }

    /// <summary>
    ///     Indicate that this state allows to transition if there is a request.
    ///     The transition is never required and can be understood as a "don't care"-transition.
    /// </summary>
    protected void AllowTransition()
    {
        Debug.Assert(next == null);
        next = NextStateTarget.CreateDirectTransition(this, isRequired: false, new TransitionDescription());
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

        if (IsDelaying()) return this;

        if (!isEntered) Enter();

        if (!released) OnUpdate();

        ChunkState nextState = DetermineNextState();

        if (ReferenceEquals(this, nextState)) return nextState;

        OnExit();
        ReleaseResources();

        return nextState;
    }

    private Boolean IsDelaying()
    {
        if (isEntered) return false;
        if (currentDelayTime >= DelayTimeout) return false;

        currentDelayTime += 1;

        return DelayEnter();
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

    private ChunkState DetermineNextState()
    {
        if (next == null) return this;

        Debug.Assert(next.Transition != null);

        ChunkState nextState = DetermineNextState(next.Transition, next.Description);

        if (nextState != next.Transition.State)
        {
            next.Transition.State.Cleanup();
            next.Transition.State.ReleaseResources();
        }

        next = null;

        return nextState;
    }

    private ChunkState DetermineNextState(NextStateTarget.DirectTransition transition, TransitionDescription description)
    {
        if (description.PrioritizeDeactivation && !Chunk.IsRequestedToLoad)
            return CreateFinalState();

        if (description.PrioritizeLoop)
        {
            ChunkState? potentialLoop = requests.Dequeue(this, isLooping: true, isDeactivating: false);

            if (potentialLoop != null) return potentialLoop;
        }

        if (transition.IsRequired) return transition.State;

        ChunkState? requestedState = requests.Dequeue(this, isLooping: false, isDeactivating: false);

        if (requestedState != null) return requestedState;

        return Chunk.IsRequestedToLoad ? transition.State : CreateFinalState();
    }

    /// <summary>
    ///     Update the state of a chunk. This can change the state.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    /// <param name="tracker">A tracker to profile state transitions.</param>
    public static void Update(ref ChunkState state, StateTracker tracker)
    {
        ChunkState previousState = state;

        state = state.Update();

        state.previous ??= previousState;
        state.requests = previousState.requests;

        if (ReferenceEquals(previousState, state)) return;

        tracker.Transition(previousState, state);
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

    private sealed record NextStateTarget(
        NextStateTarget.DirectTransition Transition,
        TransitionDescription Description)
    {
        public static NextStateTarget CreateDirectTransition(ChunkState state, Boolean isRequired, TransitionDescription description)
        {
            return new NextStateTarget(new DirectTransition(state, isRequired), description);
        }

        public sealed record DirectTransition(ChunkState State, Boolean IsRequired);
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

            if (current.next is {Transition: {State: {} next, IsRequired: true}})
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

        /// <summary>
        ///     Remove all duplicate requests.
        ///     This should be called right before entering a new state.
        ///     Duplicates can happen when the new state was not in the request queue before.
        /// </summary>
        public void RemoveDuplicates(ChunkState current)
        {
            Debug.Assert(!current.isEntered);

            var index = 0;

            while (index < requests.Count)
            {
                ChunkState? keep = ResolveDuplicate(current, requests[index].state);

                if (keep == current)
                    requests.RemoveAt(index);
                else index++;
            }
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkState>();

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Chunk {Position} state changed from {PreviousState} to {State}")]
    private static partial void LogChunkStateChange(ILogger logger, ChunkPosition position, ChunkState previousState, ChunkState state);

    #endregion LOGGING
}
