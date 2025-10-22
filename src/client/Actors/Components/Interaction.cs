// <copyright file="Interaction.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Implements the interaction logic of the player.
/// </summary>
public partial class Interaction : ActorComponent
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlayerInput input;

    private readonly Player player;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlacementSelection selection;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;

    [Constructible]
    private Interaction(Player player) : base(player)
    {
        this.player = player;

        targeting = player.GetRequiredComponent<Targeting>();
        input = player.GetRequiredComponent<PlayerInput, Player>();
        selection = player.GetRequiredComponent<PlacementSelection, Player>();
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        if (!player.Scene.CanHandleGameInput)
            return;

        if (!targeting.HasTarget) return;

        PlaceInteract(targeting.Block!.Value, targeting.Position!.Value);
        DestroyInteract(targeting.Block!.Value, targeting.Position!.Value);
    }

    private void PlaceInteract(State targetedBlock, Vector3i targetedPosition)
    {
        if (!input.ShouldInteract) return;

        Vector3i placePosition = targetedPosition;

        if (input.IsInteractionBlocked || !targetedBlock.Block.IsInteractable)
        {
            if (!targetedBlock.IsReplaceable) placePosition = placePosition.Offset(targeting.Side);

            // Prevent block placement if the block would intersect the player.
            if (selection is {IsBlockMode: true, ActiveBlock.IsSolid: true} && player.Body.Collider.Intersects(
                    selection.ActiveBlock.GetCollider(player.World, placePosition))) return;

            if (selection.IsBlockMode) selection.ActiveBlock.Place(player.World, placePosition, player);
            else selection.ActiveFluid.Fill(player.World, placePosition, FluidLevel.One, Side.Top, out _);

            input.RegisterInteraction();
        }
        else if (targetedBlock.Block.IsInteractable)
        {
            targetedBlock.Block.OnActorInteract(player, targetedPosition);

            input.RegisterInteraction();
        }
    }

    private void DestroyInteract(State targetedBlock, Vector3i targetedPosition)
    {
        if (input.ShouldDestroy)
        {
            if (selection.IsBlockMode) targetedBlock.Block.Destroy(player.World, targetedPosition, player);
            else TakeFluid(targetedPosition);

            input.RegisterInteraction();
        }

        void TakeFluid(Vector3i position)
        {
            var level = FluidLevel.One;

            if (!targetedBlock.IsReplaceable)
                position = position.Offset(targeting.Side);

            player.World.GetFluid(position)?.Fluid.Take(player.World, position, ref level);
        }
    }
}
