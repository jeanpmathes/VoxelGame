// <copyright file="Transform.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
///     Gives an <see cref="Actor" /> a position and orientation in the world.
///     Transforms can be nested to create parent-child relationships between actors.
/// </summary>
public partial class Transform : ActorComponent
{
    private readonly List<Transform> children = [];

    private Transform? parent;

    private Vector3d localPosition = Vector3d.Zero;
    private Quaterniond localRotation = Quaterniond.Identity;

    /// <summary>
    ///     Number of transforms with a listener on the <see cref="OnTransformChanged" /> in the subtree starting from and
    ///     including this transform.
    /// </summary>
    private Int32 listenerCount;

    private EventHandler? onTransformChanged;

    [Constructible]
    private Transform(Actor subject) : base(subject) {}

    /// <summary>
    /// Get or set the local position of this transform relative to its parent.
    /// </summary>
    public Vector3d LocalPosition
    {
        get => localPosition;

        set
        {
            localPosition = value;

            NotifyTransformChanged();
        }
    }

    /// <summary>
    /// Get or set the local rotation of this transform relative to its parent.
    /// </summary>
    public Quaterniond LocalRotation
    {
        get => localRotation;

        set
        {
            localRotation = value;

            NotifyTransformChanged();
        }
    }

    /// <summary>
    /// Get or set the world position of this transform.
    /// </summary>
    public Vector3d Position
    {
        get
        {
            if (parent == null) return localPosition;

            return parent.Rotation * localPosition + parent.Position;
        }

        set
        {
            localPosition = WorldToLocalPosition(value);

            NotifyTransformChanged();
        }
    }

    /// <summary>
    ///     Get or set the world rotation of this transform.
    /// </summary>
    public Quaterniond Rotation
    {
        get
        {
            if (parent == null) return localRotation;

            return parent.Rotation * localRotation;
        }

        set
        {
            localRotation = WorldToLocalRotation(value);

            NotifyTransformChanged();
        }
    }

    /// <summary>
    ///     The forward direction of this transform in world space.
    /// </summary>
    /// <remarks>
    ///     The underlying coordinate system is right-handed, which means that the Z-axis points into the view plane.
    ///     But forward is understood as going away from the viewer, hence the negation of the Z-axis.
    /// </remarks>
    public Vector3d Forward => Rotation * -Vector3d.UnitZ;

    /// <summary>
    ///     The right direction of this transform in world space.
    /// </summary>
    public Vector3d Right => Rotation * Vector3d.UnitX;

    /// <summary>
    ///     The up direction of this transform in world space.
    /// </summary>
    public Vector3d Up => Rotation * Vector3d.UnitY;

    /// <summary>
    ///     Set the parent transform of this transform, or <c>null</c> to remove the parent.
    /// </summary>
    /// <param name="newParent">The new parent transform, or <c>null</c>.</param>
    public void SetParent(Transform? newParent)
    {
        if (newParent == parent) return;

        Debug.Assert(newParent != this);
        Debug.Assert(newParent == null || newParent.Subject.World == Subject.World);
        Debug.Assert(newParent == null || !newParent.IsDescendantOf(this));

        Vector3d worldPosition = Position;
        Quaterniond worldRotation = Rotation;

        parent?.children.Remove(this);

        ApplyListenerDeltaToAncestors(-listenerCount);

        parent = newParent;

        ApplyListenerDeltaToAncestors(listenerCount);

        localPosition = WorldToLocalPosition(worldPosition);
        localRotation = WorldToLocalRotation(worldRotation);

        parent?.children.Add(this);
    }

    private Boolean IsDescendantOf(Transform? potentialAncestor)
    {
        Transform? current = parent;

        while (current != null)
        {
            if (current == potentialAncestor) return true;

            current = current.parent;
        }

        return false;
    }

    private void ApplyListenerDeltaToAncestors(Int32 delta)
    {
        if (delta == 0) return;

        Transform? current = parent;

        while (current != null)
        {
            current.listenerCount += delta;
            Debug.Assert(current.listenerCount >= 0);
            current = current.parent;
        }
    }

    private Vector3d WorldToLocalPosition(Vector3d worldPosition)
    {
        return parent == null
            ? worldPosition
            : Quaterniond.Invert(parent.Rotation) * (worldPosition - parent.Position);
    }

    private Quaterniond WorldToLocalRotation(Quaterniond worldRotation)
    {
        return parent == null
            ? worldRotation
            : Quaterniond.Invert(parent.Rotation) * worldRotation;
    }

    /// <inheritdoc />
    public override void OnAdd()
    {
        if (parent != null && parent.Subject.World != Subject.World)
            SetParent(newParent: null);

        // We assume that actors are either added or removed, never directly moved between worlds.
        // Operations breaking this assumption are defined as invalid.
        // Calling OnAdd/OnRemove on the subject will trigger the respective operations on all components, including the child transforms.

        foreach (Transform child in children) child.Subject.OnAdd(Subject.World);
    }

    /// <inheritdoc />
    public override void OnRemove()
    {
        if (parent != null && parent.Subject.World != Subject.World)
            SetParent(newParent: null);

        // See comment in OnAdd().

        foreach (Transform child in children) child.Subject.OnRemove();
    }

    private void NotifyTransformChanged()
    {
        if (listenerCount == 0) return;

        onTransformChanged?.Invoke(this, EventArgs.Empty);

        foreach (Transform child in children)
        {
            if (child.listenerCount == 0) continue;

            child.NotifyTransformChanged();
        }
    }

    /// <summary>
    ///     Invoked when the position or rotation of this transform or any of its ancestors changes.
    /// </summary>
    public event EventHandler? OnTransformChanged
    {
        add
        {
            Boolean wasEmpty = onTransformChanged == null;
            onTransformChanged += value;

            if (!wasEmpty) return;

            listenerCount += 1;
            ApplyListenerDeltaToAncestors(delta: 1);
        }

        remove
        {
            Boolean wasEmpty = onTransformChanged == null;
            onTransformChanged -= value;

            if (wasEmpty || onTransformChanged != null) return;

            listenerCount -= 1;
            ApplyListenerDeltaToAncestors(delta: -1);
        }
    }
}
