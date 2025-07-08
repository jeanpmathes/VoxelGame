// <copyright file="Interaction.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
/// Implements the interaction logic of the player.
/// </summary>
public class Interaction : ActorComponent, IConstructible<Player, Interaction>
{
    private readonly Player player;

    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Targeting targeting;
    
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly PlacementSelection selection;
    
    private Interaction(Player player) : base(player) 
    {
        this.player = player;
        
        targeting = player.GetRequiredComponent<Targeting>();
        selection = player.GetRequiredComponent<PlacementSelection, Player>();
    }

    /// <inheritdoc />
    public static Interaction Construct(Player input)
    {
        return new Interaction(input);
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
    
    private void PlaceInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        if (!player.Input.ShouldInteract) return;

        Vector3i placePosition = targetedPosition;

        if (player.Input.IsInteractionBlocked || !targetedBlock.Block.IsInteractable)
        {
            if (!targetedBlock.Block.IsReplaceable) placePosition = targeting.Side.Offset(placePosition);

            // Prevent block placement if the block would intersect the player.
            if (selection is {IsBlockMode: true, ActiveBlock.IsSolid: true} && player.Body.Collider.Intersects(
                    selection.ActiveBlock.GetCollider(player.World, placePosition))) return;

            if (selection.IsBlockMode) selection.ActiveBlock.Place(player.World, placePosition, player);
            else selection.ActiveFluid.Fill(player.World, placePosition, FluidLevel.One, Side.Top, out _);

            player.Input.RegisterInteraction();
        }
        else if (targetedBlock.Block.IsInteractable)
        {
            targetedBlock.Block.ActorInteract(player, targetedPosition);

            player.Input.RegisterInteraction();
        }
    }

    private void DestroyInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        if (player.Input.ShouldDestroy)
        {
            if (selection.IsBlockMode) targetedBlock.Block.Destroy(player.World, targetedPosition, player);
            else TakeFluid(targetedPosition);

            player.Input.RegisterInteraction();
        }

        void TakeFluid(Vector3i position)
        {
            var level = FluidLevel.One;

            if (!targetedBlock.Block.IsReplaceable)
                position = targeting.Side.Offset(position);

            player.World.GetFluid(position)?.Fluid.Take(player.World, position, ref level);
        }
    }
}
