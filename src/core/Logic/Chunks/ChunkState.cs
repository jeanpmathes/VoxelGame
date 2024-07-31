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
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Chunks;

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
    ///     The waiting mode of this state.
    /// </summary>
    public StateWaitModes WaitMode { get; private set; }

    /// <summary>
    ///     Get the chunk.
    /// </summary>
    protected Chunk Chunk { get; private set; } = null!;

    /// <summary>
    ///     Get the context.
    /// </summary>
    protected ChunkContext Context { get; private set; } = null!;

    /// <summary>
    ///     Get whether the chunk with this state is active.
    ///     Is the case when the state is <see cref="IsActive"/> and entered.
    /// </summary>
    public Boolean IsChunkActive => IsEntered && IsActive;

    /// <summary>
    ///     Get whether this state will result in the chunk being active.
    /// </summary>
    private Boolean IsActive => RequiresFullAccess && AllowSharingAccess;

    /// <summary>
    /// Whether this state requires full access to all resources of the chunk.
    /// </summary>
    private Boolean RequiresFullAccess => CoreAccess == Access.Write && ExtendedAccess == Access.Write;

    /// <summary>
    ///     Get whether this state has been entered.
    ///     An entered state has acquired all required access.
    /// </summary>
    private Boolean IsEntered { get; set; }

    /// <summary>
    ///     Get whether this state has been exited.
    ///     Is used to ensure that <see cref="OnExit" /> is only called once.
    /// </summary>
    private Boolean IsExited { get; set; }

    /// <summary>
    ///     Whether this state allows sharing its access during one update.
    ///     Required for states to be considered active.
    /// </summary>
    protected virtual Boolean AllowSharingAccess => false;

    /// <summary>
    ///     Whether this state allows that its access is stolen.
    ///     A state must hold write-access to its core and extended data to allow stealing.
    ///     If a state performs work on another thread, it cannot allow stealing.
    /// </summary>
    protected virtual Boolean AllowStealing => false;

    /// <summary>
    ///     Whether this state is considered to be hiding the chunk.
    ///     Hidden states are used when activation is not possible at the moment.
    /// </summary>
    private Boolean IsHidden => RequiresFullAccess && !AllowSharingAccess;

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

        Context.UpdateList.Add(Chunk);

        IsEntered = true;
        OnEnter();

        if (CanStealAccess && CoreAccess == Access.Write && ExtendedAccess == Access.Write) Chunk.OnUsableState();
    }

    private void Exit()
    {
        if (IsExited) return;

        IsExited = true;

        OnExit();

        ReleaseResources();
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
        if (!IsHidden)
            // If the chunk is not hidden, this method will always result in a transition to a different state.
            // This is because we transition to hidden if the activation does not provide a next state.
            // As such, we can already release resources here, which will allow more options during activation.
            ReleaseResources();

        ChunkState? state = Context.ActivateStrongly(Chunk);
        Debug.Assert(state is not Chunk.Hidden);

        return TrySettingNextState(state, description);
    }

    /// <summary>
    ///     Try to set the next state according the world chunk activation rules.
    ///     If the rules determine that the chunk should enter a different state, that state is set as the next state.
    ///     In that case, the method returns <c>true</c>, otherwise <c>false</c>.
    ///     The weak activation rule will be used, which is meant for chunks that have already been activated before.
    /// </summary>
    protected Boolean TrySettingNextActive()
    {
        if (!IsHidden)
            // If the chunk is not hidden, this method will always result in a transition to a different state.
            // This is because we transition to hidden if the activation does not provide a next state.
            // As such, we can already release resources here, which will allow more option during activation.
            ReleaseResources();

        ChunkState? state = Context.ActivateWeakly(Chunk);
        Debug.Assert(state is not Chunk.Hidden);

        return TrySettingNextState(state, new TransitionDescription());
    }

    private Boolean TrySettingNextState(ChunkState? state, TransitionDescription description)
    {
        if (state == null)
        {
            if (!IsHidden) state = new Chunk.Hidden();
            else return false;
        }

        ReleaseResources();
        SetNextState(state, description, !state.IsActive);

        return true;
    }

    /// <summary>
    ///     Wait until the function is completed before calling update again.
    /// </summary>
    /// <param name="func">The function to wait for.</param>
    protected Future<T> WaitForCompletion<T>(Func<T> func)
    {
        Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForCompletion));

        WaitMode |= StateWaitModes.WaitForCompletion;

        return Future.Create(() =>
        {
            T result = func();

            Context.UpdateList.AddOnUpdate(Chunk, ClearFlag);

            return result;
        });

        static void ClearFlag(Chunk chunk)
        {
            chunk.State.WaitMode &= ~StateWaitModes.WaitForCompletion;
        }
    }

    /// <summary>
    ///     Wait until the action is completed before calling update again.
    /// </summary>
    /// <param name="action">The action to wait for.</param>
    protected Future WaitForCompletion(Action action)
    {
        Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForCompletion));

        WaitMode |= StateWaitModes.WaitForCompletion;

        return Future.Create(() =>
        {
            action();

            Context.UpdateList.AddOnUpdate(Chunk, ClearWait);
        });

        static void ClearWait(Chunk chunk)
        {
            chunk.State.WaitMode = StateWaitModes.None;
        }
    }

    /// <summary>
    ///     Wait until certain events occur before calling update again.
    ///     At least one of the events must be set.
    ///     If both are set, the state will wait until one of the events occurs.
    ///     Calling this method will not prevent a transition if the next state was already set.
    /// </summary>
    /// <param name="onNeighborUsable">
    ///     Wait until a neighbor becomes usable.
    ///     A neighbor is usable if it has write access to all data and allows stealing.
    /// </param>
    /// <param name="onTransitionRequest">
    ///     Wait until a transition request is made.
    /// </param>
    protected void WaitForEvents(Boolean onNeighborUsable = false, Boolean onTransitionRequest = false)
    {
        Debug.Assert(onNeighborUsable || onTransitionRequest);

        if (onNeighborUsable)
        {
            Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForNeighborUsability));

            WaitMode |= StateWaitModes.WaitForNeighborUsability;
        }

        if (onTransitionRequest)
        {
            Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForRequest));

            WaitMode |= StateWaitModes.WaitForRequest;
        }

        Context.UpdateList.Remove(Chunk);
    }

    private void WaitForResource()
    {
        Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForResource));

        WaitMode |= StateWaitModes.WaitForResource;

        Context.UpdateList.Remove(Chunk);
    }

    /// <summary>
    ///     Call this when a neighbor of the chunk owning this state becomes usable.
    /// </summary>
    internal void OnNeighborUsable()
    {
        if (!WaitMode.HasFlag(StateWaitModes.WaitForNeighborUsability)) return;

        WaitMode = StateWaitModes.None;

        Context.UpdateList.Add(Chunk);
    }

    /// <summary>
    ///     Call this when any of the chunk resources have been released.
    /// </summary>
    internal void OnChunkResourceReleased()
    {
        if (!WaitMode.HasFlag(StateWaitModes.WaitForResource)) return;

        WaitMode = StateWaitModes.None;

        Context.UpdateList.Add(Chunk);
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

        if (!WaitMode.HasFlag(StateWaitModes.WaitForRequest)) return;

        WaitMode = StateWaitModes.None;

        Context.UpdateList.Add(Chunk);
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
    ///     Request a transition to a given state, but only if the queue is empty.
    /// </summary>
    private void RequestIfQueueEmpty(ChunkState state)
    {
        if (requests.Empty) RequestNextState(state);
        else state.Cleanup();
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

            if (!isAccessSufficient)
            {
                WaitForResource();

                return this;
            }

            Debug.Assert((coreGuard == null && CoreAccess == Access.None) || (coreGuard != null && Chunk.IsCoreHeldBy(coreGuard, CoreAccess)));
            Debug.Assert((extendedGuard == null && ExtendedAccess == Access.None) || (extendedGuard != null && Chunk.IsExtendedHeldBy(extendedGuard, ExtendedAccess)));
        }

        if (IsDelaying()) return this;

        if (!IsEntered) Enter();

        if (!released) OnUpdate();

        ChunkState nextState = DetermineNextState();

        if (ReferenceEquals(this, nextState)) return nextState;

        Exit();

        WaitMode = StateWaitModes.None;
        Context.UpdateList.Add(Chunk);

        return nextState;
    }

    private Boolean IsDelaying()
    {
        if (IsEntered) return false;
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
    ///     If access is stolen, the current state is exited immediately.
    ///     Access can be stolen if the chunk is in a state that allows stealing and the state holds write-access to all its
    ///     resources.
    ///     A use-case of this is when threaded work on one chunk requires access to the resources of another chunk.
    /// </summary>
    /// <param name="state">The current state. Will be exited if access is stolen.</param>
    /// <param name="tracker">A tracker to profile state transitions.</param>
    /// <returns>Guards holding write-access to all resources, or null if access could not be stolen.</returns>
    public static (Guard core, Guard extended)? TryStealAccess(ref ChunkState state, StateTracker tracker)
    {
        Throw.IfNotOnMainThread(state.Chunk);

        if (!state.CanStealAccess) return null;

        (Guard core, Guard extended) access = state.StealAccess();

        state.Exit();

        state.RequestIfQueueEmpty(new Chunk.Used(state.IsChunkActive));
        state.AllowTransition();

        return access;
    }

    private (Guard core, Guard extended) StealAccess()
    {
        Debug.Assert(this is {CoreAccess: Access.Write, coreGuard: not null});
        Debug.Assert(this is {ExtendedAccess: Access.Write, extendedGuard: not null});

        Guard core = coreGuard!;
        Guard extended = extendedGuard!;

        coreGuard = null;
        extendedGuard = null;

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
        ///     Get whether the queue is empty.
        /// </summary>
        public Boolean Empty => requests.Count == 0;

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

            if (!current.IsEntered)
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
            Debug.Assert(!current.IsEntered);

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
