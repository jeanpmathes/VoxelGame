// <copyright file="Overlay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Graphics;

namespace VoxelGame.Client.Actors;

/// <summary>
///     A block or fluid overlay that is rendered on top of the player view.
///     An overlay fills the entire horizontal screen space and has a flexible upper and lower bound.
/// </summary>
/// <param name="Size">The size of the overlay, which is the distance between the upper and lower bound.</param>
/// <param name="Texture">The texture of the overlay.</param>
/// <param name="IsBlock">Whether the overlay is a block or a fluid.</param>
/// <param name="Position">The position of the overlay block/fluid that is causing the overlay.</param>
/// <param name="Content">The content of the world causing the overlay, meaning the block and fluid at the position.</param>
public sealed record Overlay(Double Size, OverlayTexture Texture, Boolean IsBlock, Vector3i Position, Content Content)
{
    /// <summary>
    ///     Whether the overlay is a fluid.
    /// </summary>
    public Boolean IsFluid => !IsBlock;

    /// <summary>
    ///     Measure the size of the overlay to display with the given positions and their contents.
    /// </summary>
    /// <param name="positions">The positions to consider.</param>
    /// <param name="view">The view to measure the overlays in.</param>
    /// <param name="lowerBound">The total lower bound of the final overlay.</param>
    /// <param name="upperBound">The total upper bound of the final overlay.</param>
    /// <returns>All overlays that can be displayed.</returns>
    public static IEnumerable<Overlay> MeasureOverlays(IEnumerable<(Content content, Vector3i position)> positions, IView view, ref Double lowerBound, ref Double upperBound)
    {
        IView.Parameters definition = view.Definition;

        // The following multiplier is a somewhat dirty hack to improve alignment of the overlay with the actual surface.
        // A potential reason for the misalignment could be the float-based calculations on the native side.

        Frustum frustum = (definition with {Clipping = (definition.Clipping.near * 1.022, definition.Clipping.far)}).Frustum;

        List<Overlay> overlays = [];

        var anyIsBlock = false;

        foreach ((Content content, Vector3i position) in positions)
        {
            (Double, Double)? newBounds = null;
            IOverlayTextureProvider? overlayTextureProvider = null;
            var isBlock = false;

            if (content.Block.Block is IOverlayTextureProvider overlayBlockTextureProvider)
            {
                newBounds = GetOverlayBounds(content.Block, position, frustum);
                overlayTextureProvider = overlayBlockTextureProvider;

                isBlock = true;
                anyIsBlock = true;
            }

            if (newBounds == null && content.Fluid.Fluid is IOverlayTextureProvider overlayFluidTextureProvider)
            {
                newBounds = GetOverlayBounds(content.Fluid, position, frustum);
                overlayTextureProvider = overlayFluidTextureProvider;
            }

            if (newBounds is null) continue;

            (Double newLowerBound, Double newUpperBound) = newBounds.Value;
            OverlayTexture texture = overlayTextureProvider!.GetOverlayTexture(content);

            lowerBound = Math.Min(newLowerBound, lowerBound);
            upperBound = Math.Max(newUpperBound, upperBound);

            overlays.Add(new Overlay(newUpperBound - newLowerBound, texture, isBlock, position, content));
        }

        return anyIsBlock ? overlays.Where(x => x.IsBlock) : overlays;
    }

    private static (Double lower, Double upper)? GetOverlayBounds(State block, Vector3d position, Frustum frustum)
    {
        BlockHeight height = BlockHeight.Maximum;

        if (block.Block.Get<PartialHeight>() is {} partialHeight) height = partialHeight.GetCurrentHeight(block);

        return GetOverlayBounds(height, position, inverted: false, frustum);
    }

    private static (Double lower, Double upper)? GetOverlayBounds(FluidInstance fluid, Vector3d position, Frustum frustum)
    {
        BlockHeight height = fluid.Level.BlockHeight;

        return GetOverlayBounds(height, position, fluid.Fluid.Direction == VerticalFlow.Upwards, frustum);
    }

    private static (Double lower, Double upper)? GetOverlayBounds(BlockHeight height, Vector3d position, Boolean inverted, Frustum frustum)
    {
        Double actualHeight = height.Ratio;
        if (inverted) actualHeight = 1.0 - actualHeight;

        Plane topPlane = new(Vector3d.UnitY, position + Vector3d.UnitY * actualHeight);
        Plane viewPlane = frustum.Near;

        Line? bound = topPlane.Intersects(viewPlane);

        if (bound == null) return null;

        Vector3d axis = frustum.RightDirection;
        (Vector3d a, Vector3d b) dimensions = frustum.NearDimensions;

        // Assume the bound is parallel to the view horizon.
        Vector2d point = viewPlane.Project2D(bound.Value.Any, axis);
        Vector2d a = viewPlane.Project2D(dimensions.a, axis);
        Vector2d b = viewPlane.Project2D(dimensions.b, axis);

        Double ratio = MathTools.InverseLerp(a.Y, b.Y, point.Y);

        (Double newLowerBound, Double newUpperBound) = inverted ? (ratio, 1.0) : (0.0, ratio);

        newLowerBound = Math.Max(newLowerBound, val2: 0);
        newUpperBound = Math.Min(newUpperBound, val2: 1);

        return (newLowerBound, newUpperBound);
    }

    /// <summary>
    ///     Apply the tint of the world position, if the overlay texture tint is neutral.
    /// </summary>
    /// <param name="world">The world in which the overlay is rendered.</param>
    /// <returns>A new overlay texture with the tint applied.</returns>
    public OverlayTexture GetWithAppliedTint(World world)
    {
        if (!Texture.Tint.IsNeutral) return Texture;

        (ColorS block, ColorS fluid) = world.Map.GetPositionTint(Position);

        return Texture with {Tint = IsBlock ? block : fluid};
    }

    /// <summary>
    ///     Get the color of the fog caused by the block or fluid overlay.
    /// </summary>
    /// <param name="world">The world in which the overlay is rendered.</param>
    /// <returns>The color of the fog, or null if no fog is caused.</returns>
    public ColorS? GetFogColor(World world)
    {
        if (IsBlock) return null;

        (ColorS _, ColorS fluid) = world.Map.GetPositionTint(Position);

        return Content.Fluid.Fluid.GetColor(fluid);
    }
}
