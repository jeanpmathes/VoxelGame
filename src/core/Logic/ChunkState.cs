﻿// <copyright file="ChunkState.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Abstract base class for chunk states.
/// </summary>
public abstract class ChunkState
{
    private const int NeighborWaitingTimeout = 10;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ChunkState>();

    private Guard? coreGuard;

    private int currentWaitingTime;
    private Guard? extendedGuard;

    /// <summary>
    ///     Whether this state has acquired all required access. This can be true when the state is waiting on something.
    /// </summary>
    private bool isAccessSufficient;

    /// <summary>
    ///     Whether this state has acquired all required access and is not waiting on anything.
    /// </summary>
    private bool isEntered;

    private ((ChunkState state, bool isRequired)? transition, TransitionDescription description, Func<ChunkState?>? activator)? next;

    private ChunkState? previous;

    private bool released;

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
    public virtual bool IsIntendingToGetReady => false;

    /// <summary>
    ///     Whether this state wants to delay entering when neighbors intend to activate.
    /// </summary>
    protected virtual bool WaitOnNeighbors => false;

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
    public bool IsActive => isEntered && CoreAccess == Access.Write && ExtendedAccess == Access.Write && AllowSharingAccess;

    /// <summary>
    ///     Whether this state allows sharing its access during one update.
    /// </summary>
    protected virtual bool AllowSharingAccess => false;

    /// <summary>
    ///     Whether this state allows that its access is stolen.
    ///     A state must hold write-access to its core and extended data to allow stealing.
    /// </summary>
    protected virtual bool AllowStealing => false;

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
    public bool CanStealAccess => AllowStealing && isAccessSufficient;

    /// <summary>
    ///     Perform updates.
    /// </summary>
    protected abstract void OnUpdate();

    private void Enter()
    {
        if (previous != null) logger.LogDebug(Events.ChunkOperation, "Chunk {Position} state changed from {PreviousState} to {State}", Chunk.Position, previous, this);

        isEntered = true;
        OnEnter();
    }

    /// <summary>
    ///     Called directly after entering this state.
    /// </summary>
    protected virtual void OnEnter() {}

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
    ///     Signal that this chunk is now ready. The transition is required, except if certain flags are set in the description.
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

        if (!ReferenceEquals(this, nextState)) ReleaseResources();

        return nextState;
    }

    private bool IsWaitingOnNeighbors()
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

    private bool EnsureRequiredAccess()
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
    /// Deactivate the chunk.
    /// </summary>
    protected void Deactivate()
    {
        ReleaseResources();
        Context.Deactivate(Chunk);
    }

    private bool PerformActivation()
    {
        Debug.Assert(next != null);

        if (next.Value.transition != null) return true;

        Debug.Assert(next.Value.activator != null);

        if (!Chunk.CanAcquireCore(Access.Write) || !Chunk.CanAcquireExtended(Access.Write)) return false;

        ChunkState? activatedNext = next.Value.activator();
        bool isRequired = activatedNext != null;
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

        if (next.Value.description.PrioritizeDeactivation && !Chunk.IsRequested)
        {
            nextState = requests.Dequeue(this, isLooping: false, isDeactivating: true) ?? CreateFinalState();
        }
        else if (next.Value.description.PrioritizeLoop && (requestedState = requests.Dequeue(this, isLooping: true, isDeactivating: false)) != null)
        {
            nextState = requestedState;
        }
        else if (next.Value.transition.Value.isRequired)
        {
            nextState = next.Value.transition.Value.state;
        }
        else if ((requestedState = requests.Dequeue(this, isLooping: false, isDeactivating: false)) != null)
        {
            nextState = requestedState;
        }
        else if (!Chunk.IsRequested)
        {
            nextState = CreateFinalState();
        }
        else
        {
            nextState = next.Value.transition.Value.state;
        }

        if (nextState != next.Value.transition.Value.state)
            next.Value.description.Cleanup?.Invoke();

        next = null;

        return nextState;
    }

    #pragma warning restore S1871

    /// <summary>
    /// Update the state of a chunk.
    /// </summary>
    /// <param name="state">A reference to the state.</param>
    public static void Update(ref ChunkState state)
    {
        const int maxRepeatCount = 3;

        ChunkState previousState;
        var count = 0;

        do
        {
            previousState = state;
            state = state.Update();
            state.previous ??= previousState;
            state.requests = previousState.requests;
        } while (!ReferenceEquals(previousState, state) && ++count < maxRepeatCount);
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
    ///     If access is stolen, the state is changed.
    /// </summary>
    /// <returns>Guards holding write-access to all resources, or null if access could not be stolen.</returns>
    public static (Guard core, Guard extended)? TryStealAccess(ref ChunkState state)
    {
        if (!ApplicationInformation.Instance.EnsureMainThread($"ChunkState.TryStealAccess({state})", state.Chunk)) return null;
        if (!state.CanStealAccess) return null;

        Debug.Assert(state is {CoreAccess: Access.Write, coreGuard: {}});
        Debug.Assert(state is {ExtendedAccess: Access.Write, extendedGuard: {}});

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

        return (core, extended);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return GetType().Name;
    }

    private static bool IsSameState(ChunkState a, ChunkState b)
    {
        return a.GetType() == b.GetType();
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
        ///     Whether to prioritize looping transitions (to this state) before this transitions, even if this transition is
        ///     required.
        /// </summary>
        public bool PrioritizeLoop { get; init; }

        /// <summary>
        ///     Whether to prioritize chunk deactivation over this transition, even if this transition is required.
        /// </summary>
        public bool PrioritizeDeactivation { get; init; }
    }

    /// <summary>
    ///     Describes a transition request.
    /// </summary>
    public record struct RequestDescription
    {
        /// <summary>
        ///     Whether to keep the current request even if the same state type is already requested.
        /// </summary>
        public bool AllowDuplicateTypes { get; init; }

        /// <summary>
        ///     Whether to skip this request when deactivating the chunk.
        /// </summary>
        public bool AllowSkipOnDeactivation { get; init; }

        /// <summary>
        ///     Whether to allow to discard this request if the next required state is the same as this request.
        /// </summary>
        public bool AllowDiscardOnLoop { get; init; }
    }

    /// <summary>
    ///     Holds all transition requests.
    /// </summary>
    private sealed class RequestQueue
    {
        private readonly List<(ChunkState state, RequestDescription description)> requests = new();

        /// <summary>
        ///     Enqueue a new request. If the same state type is already requested, the request is ignored, unless the correct
        ///     flags are set.
        /// </summary>
        /// <param name="current">The current state.</param>
        /// <param name="state">The state to request.</param>
        /// <param name="description">The description of the request.</param>
        public void Enqueue(ChunkState current, ChunkState state, RequestDescription description)
        {
            if (description.AllowDiscardOnLoop)
            {
                bool nextIsLoop = current.next is {transition: {state: {} next, isRequired: true}} && IsSameState(next, state);
                bool currentIsLoop = !current.isEntered && IsSameState(current, state);

                if (nextIsLoop || currentIsLoop) return;
            }

            if (!description.AllowDuplicateTypes)
            {
                bool isDuplicate = requests.Any(request => IsSameState(request.state, state));

                if (isDuplicate) return;
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
        public ChunkState? Dequeue(ChunkState current, bool isLooping, bool isDeactivating)
        {
            int target = -1;

            for (var index = 0; index < requests.Count; index++)
            {
                (ChunkState state, RequestDescription description) = requests[index];

                if (isDeactivating && description.AllowSkipOnDeactivation) continue;
                if (isLooping && !IsSameState(current, state)) continue;

                target = index;

                break;
            }

            if (target == -1) return null;

            (ChunkState state, RequestDescription description) request = requests[target];
            requests.RemoveAt(target);

            return request.state;
        }
    }
}
