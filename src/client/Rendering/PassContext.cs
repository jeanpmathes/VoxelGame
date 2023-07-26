// <copyright file="PassContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Data for a render pass.
/// </summary>
/// <param name="ViewMatrix">The view matrix.</param>
/// <param name="ProjectionMatrix">The projection matrix.</param>
/// <param name="Frustum">The rendering cull frustum.</param>
public record PassContext(Matrix4d ViewMatrix, Matrix4d ProjectionMatrix, Frustum Frustum);
