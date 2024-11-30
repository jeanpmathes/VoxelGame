// <copyright file="Interaction.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     Contains interaction logic for the player.
/// </summary>
/// <param name="actor">The player actor.</param>
/// <param name="input">User input.</param>
/// <param name="targeting">The targeting system of the player.</param>
/// <param name="selector">The placement selection of the player.</param>
internal class Interaction(PhysicsActor actor, Input input, Targeting targeting, PlacementSelection selector)
{
    /// <summary>
    ///     Perform all interactions according to the current input and targeting.
    /// </summary>
    internal void Perform()
    {
        if (!targeting.HasTarget) return;

        PlaceInteract(targeting.Block!.Value, targeting.Position!.Value);
        DestroyInteract(targeting.Block!.Value, targeting.Position!.Value);
    }

    private void PlaceInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        if (!input.ShouldInteract) return;

        Vector3i placePosition = targetedPosition;

        if (input.IsInteractionBlocked || !targetedBlock.Block.IsInteractable)
        {
            if (!targetedBlock.Block.IsReplaceable) placePosition = targeting.Side.Offset(placePosition);

            // Prevent block placement if the block would intersect the player.
            if (selector is {IsBlockMode: true, ActiveBlock.IsSolid: true} && actor.Collider.Intersects(
                    selector.ActiveBlock.GetCollider(actor.World, placePosition))) return;

            if (selector.IsBlockMode) selector.ActiveBlock.Place(actor.World, placePosition, actor);
            else selector.ActiveFluid.Fill(actor.World, placePosition, FluidLevel.One, Side.Top, out _);

            input.RegisterInteraction();
        }
        else if (targetedBlock.Block.IsInteractable)
        {
            targetedBlock.Block.ActorInteract(actor, targetedPosition);

            input.RegisterInteraction();
        }
    }

    private void DestroyInteract(BlockInstance targetedBlock, Vector3i targetedPosition)
    {
        if (input.ShouldDestroy)
        {
            if (selector.IsBlockMode) targetedBlock.Block.Destroy(actor.World, targetedPosition, actor);
            else TakeFluid(targetedPosition);

            input.RegisterInteraction();
        }

        void TakeFluid(Vector3i position)
        {
            var level = FluidLevel.One;

            if (!targetedBlock.Block.IsReplaceable)
                position = targeting.Side.Offset(position);

            actor.World.GetFluid(position)?.Fluid.Take(actor.World, position, ref level);
        }
    }
}
