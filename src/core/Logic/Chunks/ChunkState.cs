// <copyright file="ChunkState.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Abstract base class for chunk states.
/// </summary>
public abstract class ChunkState
{
    private Guard? guard;

    /// <summary>
    ///     Whether this state has acquired all required access. This can be true when the state is waiting on something.
    /// </summary>
    private Boolean isAccessSufficient;

    /// <summary>
    ///     Whether this state has exited and released all resources.
    /// </summary>
    private Boolean released;

    private ChunkState? next;
    private ChunkState? previous;
    private RequestQueue requests = null!;

    /// <summary>
    ///     Create a new chunk state.
    /// </summary>
    protected ChunkState() {}

    /// <summary>
    ///     Create a new chunk state with guard already acquired.
    ///     The acquired guard must fit the access requirements of the state.
    /// </summary>
    /// <param name="guard">The guard.</param>
    protected ChunkState(Guard? guard)
    {
        this.guard = guard;
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
    ///     Is the case when the state is <see cref="IsActive" /> and entered.
    /// </summary>
    public Boolean IsChunkActive => IsEntered && IsActive && !IsExited;

    /// <summary>
    ///     Get whether this state will result in the chunk being active.
    /// </summary>
    private Boolean IsActive => Access == Access.Write && AllowSharingAccess;

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
    ///     Whether it is acceptable to discard this state and deactivate the chunk instead.
    /// </summary>
    protected virtual Boolean CanDiscard => false;

    /// <summary>
    ///     Whether this state is a hidden state.
    ///     Hidden states are used when activation is not possible at the moment.
    /// </summary>
    protected virtual Boolean IsHidden => false;

    /// <summary>
    ///     The required access level of this state to chunk resources.
    /// </summary>
    protected abstract Access Access { get; }

    /// <summary>
    ///     Whether it is currently possible to steal access from this state.
    /// </summary>
    public Boolean CanStealAccess => AllowStealing && isAccessSufficient;

    private Boolean IsInHiddenState => IsHidden && IsEntered && !IsExited;

    /// <summary>
    ///     Perform updates.
    ///     This is where the state logic, e.g. the work associated with the state as well as transitions, is performed.
    /// </summary>
    protected abstract void OnUpdate();

    private void Enter()
    {
        requests.RemoveDuplicates(this);

        Chunk.OnStateTransition(previous, this);

        Context.UpdateList.Add(Chunk);

        IsEntered = true;
        OnEnter();

        if (CanStealAccess && Access == Access.Write) Chunk.OnUsableState();
    }

    private void Exit()
    {
        if (IsExited) return;

        IsExited = true;

        OnExit();

        CleanupAndRelease(this);
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
    ///     Perform cleanup in the case that the state is not transitioned to.
    /// </summary>
    protected virtual void Cleanup() {}

    /// <summary>
    ///     Set the next state.
    /// </summary>
    /// <param name="state">The next state.</param>
    protected void SetNextState(ChunkState state)
    {
        state.Chunk = Chunk;
        state.Context = Context;

        Debug.Assert(next == null);
        next = state;
    }

    /// <summary>
    ///     Set the next state.
    /// </summary>
    /// <typeparam name="T">The type of the next state.</typeparam>
    protected void SetNextState<T>() where T : ChunkState, new()
    {
        SetNextState(new T());
    }

    /// <summary>
    ///     Try to set the next state according to the world chunk activation rules.
    ///     Either <see cref="TryStrongActivation" /> or <see cref="TryWeakActivation" /> will be used.
    /// </summary>
    /// <returns><c>true</c> if this results in a transition to a different state, otherwise <c>false</c>.</returns>
    protected Boolean TryActivation()
    {
        return Chunk.HasBeenActive ? TryWeakActivation() : TryStrongActivation();
    }

    /// <summary>
    ///     Try to set the next state according to the world chunk activation rules.
    ///     If the rules determine that the chunk should enter a different state, that state is set as the next state.
    ///     In that case, the method returns <c>true</c>, otherwise <c>false</c>.
    ///     The strong activation rule will be used, which is meant for chunks that have not been activated yet.
    ///     If the current state is not the hidden state, this method will always result in a transition to a different state.
    /// </summary>
    private Boolean TryStrongActivation()
    {
        if (!IsInHiddenState)
            // If the chunk is not hidden, this method will always result in a transition to a different state.
            // This is because we transition to hidden if the activation does not provide a next state.
            // As such, we can already release resources here, which will allow more options during activation.
            ReleaseResource();

        ChunkState? state = Context.ActivateStrongly(Chunk);
        Debug.Assert(state is not Chunk.Hidden);

        return TrySettingNextState(state);
    }

    /// <summary>
    ///     Try to set the next state according to the world chunk activation rules.
    ///     If the rules determine that the chunk should enter a different state, that state is set as the next state.
    ///     In that case, the method returns <c>true</c>, otherwise <c>false</c>.
    ///     The weak activation rule will be used, which is meant for chunks that have already been activated before.
    ///     If the current state is not the hidden state, this method will always result in a transition to a different state.
    /// </summary>
    private Boolean TryWeakActivation()
    {
        if (!IsInHiddenState)
            // If the chunk is not hidden, this method will always result in a transition to a different state.
            // This is because we transition to hidden if the activation does not provide a next state.
            // As such, we can already release resources here, which will allow more option during activation.
            ReleaseResource();

        ChunkState? state = Context.ActivateWeakly(Chunk);
        Debug.Assert(state is not Chunk.Hidden);

        return TrySettingNextState(state);
    }

    private Boolean TrySettingNextState(ChunkState? state)
    {
        if (state == null)
        {
            if (!IsInHiddenState) state = new Chunk.Hidden();
            else return false;
        }

        ReleaseResource();
        SetNextState(state);

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
    /// <param name="onRequestLevelChange">
    ///     Wait until the request level of the chunk changes.
    /// </param>
    protected void WaitForEvents(
        Boolean onNeighborUsable = false,
        Boolean onTransitionRequest = false,
        Boolean onRequestLevelChange = false)
    {
        Debug.Assert(onNeighborUsable || onTransitionRequest);

        if (onNeighborUsable)
        {
            Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForNeighborUsability));

            WaitMode |= StateWaitModes.WaitForNeighborUsability;
        }

        if (onTransitionRequest)
        {
            Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForTransitionRequest));

            WaitMode |= StateWaitModes.WaitForTransitionRequest;
        }

        if (onRequestLevelChange)
        {
            Debug.Assert(!WaitMode.HasFlag(StateWaitModes.WaitForRequestLevelChange));

            WaitMode |= StateWaitModes.WaitForRequestLevelChange;
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

        ScheduleUpdate();
    }

    internal void OnRequestLevelChange()
    {
        if (!WaitMode.HasFlag(StateWaitModes.WaitForRequestLevelChange)) return;

        ScheduleUpdate();
    }

    /// <summary>
    ///     Call this when any of the chunk resources have been released.
    /// </summary>
    internal void OnChunkResourceReleased()
    {
        if (!WaitMode.HasFlag(StateWaitModes.WaitForResource)) return;

        ScheduleUpdate();
    }

    /// <summary>
    ///     Indicate that this state allows to transition if there is a request.
    ///     The transition is never required and can be understood as a "don't care"-transition.
    /// </summary>
    protected void AllowTransition()
    {
        Debug.Assert(next == null);
        next = this;
    }

    /// <summary>
    ///     Request an external state transition on a chunk, which is added to request the queue.
    /// </summary>
    /// <param name="state">The next state.</param>
    public void RequestNextState(ChunkState state)
    {
        state.Chunk = Chunk;
        state.Context = Context;

        requests.Enqueue(this, state);

        if (!WaitMode.HasFlag(StateWaitModes.WaitForTransitionRequest)) return;

        ScheduleUpdate();
    }

    /// <summary>
    ///     Request an external state transition on an active chunk. Internal transitions are prioritized over external ones
    ///     and deactivation comes last.
    /// </summary>
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
        if (IsWaitingForAccess()) return this;

        if (!IsEntered) Enter();

        DoUpdateIfNeeded();

        ChunkState nextState = DetermineNextState();

        if (nextState == this) return nextState;

        Exit();

        ScheduleUpdate();

        return nextState;
    }

    private void ScheduleUpdate()
    {
        WaitMode = StateWaitModes.None;

        Context.UpdateList.Add(Chunk);
    }

    private Boolean IsWaitingForAccess()
    {
        if (released) return false;

        isAccessSufficient = EnsureRequiredAccess();

        if (!isAccessSufficient)
        {
            WaitForResource();

            return true;
        }

        Debug.Assert((guard == null && Access == Access.None) || (guard != null && Chunk.IsHeldBy(guard, Access)));

        return false;
    }

    private void DoUpdateIfNeeded()
    {
        if (released) return;

        OnUpdate();
    }

    private Boolean EnsureRequiredAccess()
    {
        var isSufficient = true;
        var canAcquire = true;

        if (Access != Access.None && guard == null)
        {
            isSufficient = false;
            canAcquire &= Chunk.CanAcquire(Access);
        }

        if (isSufficient || !canAcquire)
            return isSufficient;

        if (Access != Access.None)
            guard = Chunk.Acquire(Access);

        return true;
    }

    /// <summary>
    ///     Release all held resources. A state will not be updated when released, and must transition until the next update.
    /// </summary>
    private void ReleaseResource()
    {
        guard?.Dispose();
        guard = null;

        released = true;
        isAccessSufficient = false;
    }

    /// <summary>
    ///     Deactivate the chunk.
    /// </summary>
    protected void Deactivate()
    {
        CleanupAndRelease(this);
        Context.Deactivate(Chunk);
    }

    private ChunkState DetermineNextState()
    {
        if (next == null)
        {
            if (IsExited)
            {
                // Chunk access was stolen, as stealing exits the state but does not set a next state.
                // The only other way to exit a state is to transition to another state, which would set the next state.

                TryActivation();

                Debug.Assert(next != null);
            }
            else
            {
                return this;
            }
        }

        ChunkState nextState = DetermineNextState(next);

        if (nextState != next && nextState != this)
            CleanupAndRelease(next);

        next = null;

        return nextState;
    }

    private ChunkState DetermineNextState(ChunkState transition)
    {
        Boolean prioritizeRequests = transition == this;

        if (!prioritizeRequests && !transition.CanDiscard)
            return transition;

        if (!Chunk.IsRequestedToLoad)
            return requests.Dequeue(isDeactivating: true) ?? CreateFinalState();

        return requests.Dequeue(isDeactivating: false) ?? transition;
    }

    /// <summary>
    ///     Update the state of a chunk. This can change the state.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    /// <returns>If the state has changed, this will be the previous state, otherwise <c>null</c>.</returns>
    public static ChunkState? Update(ref ChunkState state)
    {
        ChunkState previousState = state;

        state = state.Update();

        state.previous ??= previousState;
        state.requests = previousState.requests;

        if (state == previousState)
            return null;

        ChunkState? previous = previousState.previous;
        previousState.previous = null;

        return previous;
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
    /// <returns>Guards holding write-access to all resources, or null if access could not be stolen.</returns>
    public static Guard? TryStealAccess(ref ChunkState state)
    {
        ApplicationInformation.ThrowIfNotOnMainThread(state.Chunk);

        if (!state.CanStealAccess) return null;

        Guard access = state.StealAccess();

        state.Exit();
        state.ScheduleUpdate();

        return access;
    }

    private Guard StealAccess()
    {
        Debug.Assert(this is {Access: Access.Write, guard: not null});

        Guard stolen = guard!;

        guard = null;

        return stolen;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return GetType().Name;
    }

    private static void CleanupAndRelease(ChunkState? state)
    {
        state?.Cleanup();
        state?.ReleaseResource();
    }

    /// <summary>
    ///     Holds all transition requests.
    /// </summary>
    private sealed class RequestQueue
    {
        private readonly List<ChunkState> requests = [];

        /// <summary>
        ///     Enqueue a new request. If the same state type is already requested, the request is ignored, unless the correct
        ///     flags are set.
        /// </summary>
        /// <param name="current">The current state.</param>
        /// <param name="state">The state to request.</param>
        public void Enqueue(ChunkState current, ChunkState state)
        {
            // Requesting a state that already has access to itself is not allowed.
            // This is because transitions taken before the request is processed might also need that access.
            // As such, a deadlock could occur.

            Debug.Assert(state.guard == null);

            if (current.next is {CanDiscard: false} next && IsSameStateType(next, state))
            {
                CleanupAndRelease(state);

                return;
            }

            if (!current.IsEntered && IsSameStateType(current, state))
            {
                CleanupAndRelease(state);

                return;
            }

            foreach (ChunkState request in requests)
            {
                if (!IsSameStateType(request, state)) continue;

                CleanupAndRelease(state);

                return;
            }

            requests.Add(state);
        }

        /// <summary>
        ///     Dequeue the first request.
        /// </summary>
        /// <param name="isDeactivating">
        ///     Whether the chunk is deactivating. This will filter out all requests that are not required
        ///     before deactivation.
        /// </param>
        /// <returns>The first request, or null if no request is available.</returns>
        public ChunkState? Dequeue(Boolean isDeactivating)
        {
            if (requests.Count == 0) return null;

            Int32 target = -1;

            for (var index = 0; index < requests.Count; index++)
            {
                ChunkState state = requests[index];

                if (isDeactivating && state.CanDiscard) continue;

                target = index;

                break;
            }

            if (target == -1) return null;

            ChunkState request = requests[target];
            requests.RemoveAt(target);

            return request;
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
                if (IsSameStateType(current, requests[index]))
                {
                    CleanupAndRelease(requests[index]);
                    requests.RemoveAt(index);
                }
                else
                {
                    index++;
                }
        }

        private static Boolean IsSameStateType(ChunkState a, ChunkState b)
        {
            return a.GetType() == b.GetType();
        }
    }
}
