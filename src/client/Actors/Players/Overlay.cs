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
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Actors.Players;

/// <summary>
///     A block or fluid overlay that is rendered on top of the player view.
///     An overlay fills the entire horizontal screen space and has a flexible upper and lower bound.
/// </summary>
/// <param name="Size">The size of the overlay, which is the distance between the upper and lower bound.</param>
/// <param name="Texture">The texture of the overlay.</param>
/// <param name="IsBlock">Whether the overlay is a block or a fluid.</param>
/// <param name="Position">The position of the overlay block/fluid.</param>
public sealed record Overlay(double Size, OverlayTexture Texture, bool IsBlock, Vector3i Position)
{
    /// <summary>
    ///     Measure the size of the overlay to display with the given positions and their contents.
    /// </summary>
    /// <param name="positions">The positions to consider.</param>
    /// <param name="frustum">The frustum to use for the measurement.</param>
    /// <param name="lowerBound">The total lower bound of the final overlay.</param>
    /// <param name="upperBound">The total upper bound of the final overlay.</param>
    /// <returns>All overlays that can be displayed.</returns>
    public static IEnumerable<Overlay> MeasureOverlays(IEnumerable<(Content content, Vector3i position)> positions, Frustum frustum, ref double lowerBound, ref double upperBound)
    {
        List<Overlay> overlays = new();

        var anyIsBlock = false;

        foreach ((Content content, Vector3i position) in positions)
        {
            (double, double)? newBounds = null;
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

            (double newLowerBound, double newUpperBound) = newBounds.Value;
            OverlayTexture texture = overlayTextureProvider!.GetOverlayTexture(content);

            lowerBound = Math.Min(newLowerBound, lowerBound);
            upperBound = Math.Max(newUpperBound, upperBound);

            overlays.Add(new Overlay(newUpperBound - newLowerBound, texture, isBlock, position));
        }

        return anyIsBlock ? overlays.Where(x => x.IsBlock) : overlays;
    }

    private static (double lower, double upper)? GetOverlayBounds(BlockInstance block, Vector3d position, Frustum frustum)
    {
        var height = 15;

        if (block.Block is IHeightVariable heightVariable) height = heightVariable.GetHeight(block.Data);

        return GetOverlayBounds(height, position, inverted: false, frustum);
    }

    private static (double lower, double upper)? GetOverlayBounds(FluidInstance fluid, Vector3d position, Frustum frustum)
    {
        int height = fluid.Level.GetBlockHeight();

        return GetOverlayBounds(height, position, fluid.Fluid.Direction == VerticalFlow.Upwards, frustum);
    }

    private static (double lower, double upper)? GetOverlayBounds(int height, Vector3d position, bool inverted, Frustum frustum)
    {
        float actualHeight = (height + 1) * (1.0f / 16.0f);
        if (inverted) actualHeight = 1.0f - actualHeight;

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

        double ratio = VMath.InverseLerp(a.Y, b.Y, point.Y);

        (double newLowerBound, double newUpperBound) = inverted ? (ratio, 1.0) : (0.0, ratio);

        newLowerBound = Math.Max(newLowerBound, val2: 0);
        newUpperBound = Math.Min(newUpperBound, val2: 1);

        return (newLowerBound, newUpperBound);
    }

    /// <summary>
    ///     Apply the tint of the world position, if the overlay texture tint is neutral.
    /// </summary>
    /// <returns>A new overlay texture with the tint applied.</returns>
    public OverlayTexture GetWithAppliedTint(World world)
    {
        if (!Texture.Tint.IsNeutral) return Texture;

        (TintColor block, TintColor fluid) = world.Map.GetPositionTint(Position);

        if (IsBlock) return Texture with {Tint = block};

        return Texture with {Tint = fluid};
    }
}
